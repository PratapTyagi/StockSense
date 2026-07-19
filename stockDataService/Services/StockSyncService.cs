using Interfaces;
using Microsoft.EntityFrameworkCore;
using StockDataService.Data;
using StockDataService.Entities;

namespace StockDataService.Services;

/// <summary>
/// Runs once at application startup.
///  1. Fetches the Kite instruments CSV, filters for NSE equities
///  2. Ensures each stock exists in the Stocks table and creates a matching StockSyncStatus row if missing
/// </summary>
public class StockSyncService : IStockSyncService
{
    private readonly IStocksRepository _stocksRepo;
    private readonly IStockSyncRepository _stockSyncRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<StockSyncService> _logger;
    public StockSyncService(
        IStocksRepository stocksRepo,
        IStockSyncRepository stockSyncRepo,
        IHttpClientFactory httpClientFactory,
        ILogger<StockSyncService> logger)
    {
        _stocksRepo = stocksRepo;
        _stockSyncRepo = stockSyncRepo;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Fetches the Kite instruments CSV, filters for NSE equities, ensures each stock exists in the Stocks table and creates a matching StockSyncStatus row if missing.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SyncStocksAsync(CancellationToken cancellationToken)
    {
        try
        {
            var parsedStocks = await FetchAndParseInstrumentsAsync(cancellationToken);
            _logger.LogInformation("Parsed {Count} NSE equity instruments from Kite.", parsedStocks.Count);

            await UpsertStocksAndSyncStatusesAsync(parsedStocks, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Kite instruments on startup.");
            throw;
        }
    }

    /// <summary>
    /// Fetches the Kite instruments CSV, filters for NSE equities, and returns a list of parsed StockRecord objects (without Ids).
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<List<StockRecord>> FetchAndParseInstrumentsAsync(CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient(Constants.ZerodhaInstrumentsClient);
        using var response = await client.GetAsync(Constants.InstrumentsEndpoint, ct);
        response.EnsureSuccessStatusCode();

        var csvContent = await response.Content.ReadAsStringAsync(ct);
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // CSV column indices (0-based)
        const int colInstrumentToken = 0;
        const int colTradingSymbol = 2;
        const int colCompanyName = 3;
        const int colExpiry = 5;
        const int colInstrumentType = 9;
        const int colSegment = 10;
        const int colExchange = 11;

        var results = new List<StockRecord>(lines.Length);
        var isHeader = true;

        foreach (var rawLine in lines)
        {
            if (isHeader) { isHeader = false; continue; }

            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Length <= colExchange) continue;

            // Filter: equity/cash only (empty expiry) on NSE segment
            if (!string.IsNullOrEmpty(fields[colExpiry].Trim())) continue;
            if (!fields[colExchange].Trim().Equals("NSE", StringComparison.OrdinalIgnoreCase)) continue;
            if (!fields[colSegment].Trim().Equals("NSE", StringComparison.OrdinalIgnoreCase)) continue;
            if (!fields[colInstrumentType].Trim().Equals("EQ", StringComparison.OrdinalIgnoreCase)) continue;

            var tradingSymbol = fields[colTradingSymbol].Trim();
            if (string.IsNullOrEmpty(tradingSymbol)) continue;

            if (!int.TryParse(fields[colInstrumentToken].Trim(), out var instrumentToken)) continue;

            results.Add(new StockRecord
            {
                Symbol = tradingSymbol,
                InstrumentToken = instrumentToken.ToString(),   // instrument_token stored as Name (per existing schema)
                Exchange = fields[colExchange].Trim(),
                CompanyName = fields[colCompanyName].Trim(),
                IsActive = true
            });
        }

        return results;
    }

    /// <summary>
    /// Ensures each parsed stock exists in DB and creates a StockSyncStatus row if missing.
    /// Returns the list of tracked StockRecord objects (with populated Id values).
    /// </summary>
    private async Task<List<StockRecord>> UpsertStocksAndSyncStatusesAsync(
        List<StockRecord> parsedStocks,
        CancellationToken ct)
    {
        var parsedBySymbol = parsedStocks.ToDictionary(s => s.Symbol);

        // Load existing stocks and index them by symbol for O(1) lookup
        var existingBySymbol = (await _stocksRepo.GetAllAsync())
            .ToDictionary(s => s.Symbol);

        // Determine which parsed stocks are new (not already in DB)
        var newStocks = parsedBySymbol
            .Where(kvp => !existingBySymbol.ContainsKey(kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();

        if (newStocks.Count > 0)
        {
            await _stocksRepo.InsertManyAsync(newStocks);
            await _stockSyncRepo.InsertManyAsync(newStocks.Select(s => new StockSyncStatusRecord
            {
                Stock_Id = s.Id,
                Symbol = s.Symbol,
                LastCandleTimestamp = DateTime.Now.AddDays(-1 * 3 * 365)
            }).ToList());
            _logger.LogInformation("Inserted {Count} new stocks.", newStocks.Count);
        }

        // Merge existing + newly-inserted (both now have valid Ids)
        var allTracked = new List<StockRecord>(existingBySymbol.Values);
        allTracked.AddRange(newStocks);
        return allTracked;
    }

    /// <summary>
    /// Parses a single CSV line, respecting double-quoted values.
    /// </summary>
    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        int i = 0, len = line.Length;

        while (i <= len)
        {
            if (i == len) { fields.Add(string.Empty); break; }

            if (line[i] == '"')
            {
                i++;
                var start = i;
                while (i < len && line[i] != '"') i++;
                fields.Add(line.Substring(start, i - start));
                i++;
                if (i < len && line[i] == ',') i++;
            }
            else
            {
                var start = i;
                while (i < len && line[i] != ',') i++;
                fields.Add(line.Substring(start, i - start));
                if (i < len && line[i] == ',') i++;
            }
        }

        return fields.ToArray();
    }
}