using Entities;
using Enums;
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
public class ZerodhaStockSyncService : IStockSyncService
{
    private readonly IStocksRepository _stocksRepo;
    private readonly IStockSyncRepository _stockSyncRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ZerodhaStockSyncService> _logger;

    public ZerodhaStockSyncService(
        IStocksRepository stocksRepo,
        IStockSyncRepository stockSyncRepo,
        IHttpClientFactory httpClientFactory,
        ILogger<ZerodhaStockSyncService> logger)
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
            string csv = await FetchInstrumentsAsync(cancellationToken);

            IReadOnlyList<InstrumentRecord> instruments = ParseInstruments(csv);

            List<StockRecord> stocks = ParseStocks(instruments);
            _logger.LogInformation("Parsed {Count} NSE equity instruments from Kite.", stocks.Count);

            await UpsertStocksAndSyncStatusesAsync(stocks, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Kite instruments on startup.");
            throw;
        }
    }

    /// <summary>
    /// Fetch all the instruments present in zerodha instruments api
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<string> FetchInstrumentsAsync(CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient(Constants.ZerodhaInstrumentsClient);

        using var response = await client.GetAsync(Constants.InstrumentsEndpoint, ct);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }

    /// <summary>
    /// Parse instruments from csv
    /// </summary>
    /// <param name="csv"></param>
    /// <returns></returns>
    private IReadOnlyList<InstrumentRecord> ParseInstruments(string csv)
    {
        var results = new List<InstrumentRecord>();

        foreach (var line in csv.Split('\n').Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseCsvLine(line.Trim());

            if (fields.Length <= InstrumentColumns.Exchange)
                continue;

            results.Add(new InstrumentRecord
            {
                InstrumentToken = fields[InstrumentColumns.InstrumentToken].Trim(),
                TradingSymbol = fields[InstrumentColumns.TradingSymbol].Trim(),
                CompanyName = fields[InstrumentColumns.CompanyName].Trim(),
                Expiry = fields[InstrumentColumns.Expiry].Trim(),
                InstrumentType = fields[InstrumentColumns.InstrumentType].Trim(),
                Segment = fields[InstrumentColumns.Segment].Trim(),
                Exchange = fields[InstrumentColumns.Exchange].Trim()
            });
        }

        return results;
    }

    /// <summary>
    /// Parse stocks from instruments
    /// </summary>
    /// <param name="instruments"></param>
    /// <returns></returns>
    private List<StockRecord> ParseStocks(IEnumerable<InstrumentRecord> instruments)
    {
        return instruments
            .Where(IsListedStock)
            .Select(ToStockRecord)
            .ToList();
    }

    /// <summary>
    /// To stock record
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    private static StockRecord ToStockRecord(InstrumentRecord instrument)
    {
        return new StockRecord
        {
            Symbol = instrument.TradingSymbol,
            InstrumentToken = instrument.InstrumentToken,
            CompanyName = instrument.CompanyName,
            Exchange = instrument.Exchange,
            IsActive = true
        };
    }

    /// <summary>
    /// Is Listed stock
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    private static bool IsListedStock(InstrumentRecord instrument)
    {
        InstrumentCategory category = ClassifyInstrumentCategory(instrument);
        return category is InstrumentCategory.MainboardStock or InstrumentCategory.SmeStock;
    }


    /// <summary>
    /// Classify instrument category
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns>InstrumentCategory</returns>
    private static InstrumentCategory ClassifyInstrumentCategory(InstrumentRecord instrument)
    {
        if (!IsNseCashEquity(instrument))
            return InstrumentCategory.Unknown;

        if (string.IsNullOrWhiteSpace(instrument.CompanyName))
            return InstrumentCategory.Unknown;

        var symbol = instrument.TradingSymbol;
        var company = instrument.CompanyName;

        if (IsGovernmentSecurity(symbol, company))
            return InstrumentCategory.GovernmentSecurity;

        if (IsBond(symbol, company))
            return InstrumentCategory.Bond;

        if (IsEtf(symbol, company))
            return InstrumentCategory.Etf;

        if (IsReit(symbol))
            return InstrumentCategory.Reit;

        if (IsInvit(symbol))
            return InstrumentCategory.Invit;

        if (IsSme(symbol))
            return InstrumentCategory.SmeStock;

        return InstrumentCategory.MainboardStock;
    }
    private static bool IsNseCashEquity(InstrumentRecord instrument)
    {
        return string.IsNullOrWhiteSpace(instrument.Expiry)
            && instrument.Exchange.Equals("NSE", StringComparison.OrdinalIgnoreCase)
            && instrument.Segment.Equals("NSE", StringComparison.OrdinalIgnoreCase)
            && instrument.InstrumentType.Equals("EQ", StringComparison.OrdinalIgnoreCase);
    }
    private static bool IsGovernmentSecurity(string symbol, string companyName)
    {
        return symbol.EndsWith("-SG", StringComparison.OrdinalIgnoreCase)
            || symbol.EndsWith("-GS", StringComparison.OrdinalIgnoreCase)
            || symbol.EndsWith("-TB", StringComparison.OrdinalIgnoreCase)
            || companyName.StartsWith("SDL ", StringComparison.OrdinalIgnoreCase)
            || companyName.StartsWith("GOI ", StringComparison.OrdinalIgnoreCase);
    }
    private static bool IsBond(string symbol, string companyName)
    {
        return symbol.EndsWith("-N0", StringComparison.OrdinalIgnoreCase)
            || symbol.EndsWith("-N1", StringComparison.OrdinalIgnoreCase)
            || companyName.Contains("BOND", StringComparison.OrdinalIgnoreCase)
            || companyName.Contains("NCD", StringComparison.OrdinalIgnoreCase);
    }
    private static bool IsEtf(string symbol, string companyName)
    {
        return companyName.Contains("ETF", StringComparison.OrdinalIgnoreCase)
            || symbol.EndsWith("BEES", StringComparison.OrdinalIgnoreCase);
    }
    private static bool IsReit(string symbol)
        => symbol.EndsWith("-RR", StringComparison.OrdinalIgnoreCase);
    private static bool IsInvit(string symbol)
        => symbol.EndsWith("-IV", StringComparison.OrdinalIgnoreCase);
    private static bool IsSme(string symbol)
    => symbol.EndsWith("-SM", StringComparison.OrdinalIgnoreCase);


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

    /// <summary>
    /// Instruments column
    /// </summary>
    private static class InstrumentColumns
    {
        public const int InstrumentToken = 0;
        public const int TradingSymbol = 2;
        public const int CompanyName = 3;
        public const int Expiry = 5;
        public const int InstrumentType = 9;
        public const int Segment = 10;
        public const int Exchange = 11;
    }
}