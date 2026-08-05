using Newtonsoft.Json;
using Interfaces;
using StockDataService.Entities;
using System.Numerics;

namespace Helpers;

public class ZerodhaStockHistorySyncService(
    IHttpClientFactory httpClientFactory,
    ILogger<ZerodhaStockHistorySyncService> logger,
    IStockCandlesRepository stockCandlesRepo,
    IStockSyncRepository stockSyncRepo,
    IEncTokenProvider encTokenProvider)
    : IStockHistorySyncService
{
    /// <summary>
    /// Fetches all stock statuses and syncs historical data for each stock that is not up-to-date.
    /// </summary>
    /// <returns></returns>
    public async Task SyncAllStocksHistoricalDataAsync()
    {
        Dictionary<string, StockSyncStatusRecord> syncStatusBySymbol = await stockSyncRepo.GetAllStockSyncStatusAsync();
        logger.LogInformation("Starting historical data sync for {Count} stocks.", syncStatusBySymbol.Count);

        DateTime toDate = DateTime.Today;

        foreach (var (symbol, syncStatus) in syncStatusBySymbol)
        {
            StockRecord stock = syncStatus.Stock;
            DateTime fromDate = GetFromDate(syncStatus);
            if (fromDate > toDate)
            {
                logger.LogInformation("{Symbol} is already up-to-date.", symbol);
                continue;
            }

            try
            {
                var candles = await FetchCandlesFromZerodhaAsync(stock, fromDate, toDate);
                if (candles.Count == 0) continue;

                await stockCandlesRepo.InsertManyAsync(candles);

                var updatedSyncStatus = syncStatus;

                if (updatedSyncStatus == null)
                {
                    updatedSyncStatus = new StockSyncStatusRecord
                    {
                        Stock_Id = stock.Id,
                        Symbol = symbol,
                        LastCandleTimestamp = candles[^1].Timestamp
                    };
                }
                else
                {
                    syncStatus.LastCandleTimestamp = candles[^1].Timestamp;
                }

                await stockSyncRepo.UpsertStockSyncStatusAsync(syncStatus);

                logger.LogInformation(
                    "{Symbol} synced successfully. {Count} candles imported.",
                    symbol,
                    candles.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch latest historical data for {Symbol}", symbol);
            }
        }

        logger.LogInformation("Historical data sync completed.");
    }

    /// <summary>
    /// Public entry-point used by the controller. Fetches and persists candles for a single symbol.
    /// </summary>
    public async Task<List<StockCandleRecord>> GetStockHistoricalData(
        string stockSymbol,
        DateTime fromDate)
    {
        StockSyncStatusRecord stockSyncStatus = await stockSyncRepo.GetStockSyncStatusBySymbolAsync(stockSymbol)
            ?? throw new ArgumentException($"Invalid stock symbol: {stockSymbol}");
        List<StockCandleRecord> stockHistoryInDatabase = await stockCandlesRepo.GetByStockIdAndFromDateAsync(stockSyncStatus.Stock.Id, fromDate)
            ?? throw new ArgumentException($"Invalid stock symbol: {stockSymbol}");

        List<StockCandleRecord> remainingCandles = await FetchCandlesFromZerodhaAsync(stockSyncStatus.Stock, stockHistoryInDatabase.FirstOrDefault()?.Timestamp ?? fromDate, DateTime.Today);
        List<StockCandleRecord> totalStockHistory = stockHistoryInDatabase.Concat(remainingCandles).OrderByDescending(c => c.Timestamp).ToList();
        stockSyncStatus.LastCandleTimestamp = totalStockHistory.LastOrDefault()?.Timestamp ?? stockSyncStatus.LastCandleTimestamp;
        if (remainingCandles.Count > 0)
        {
            await stockCandlesRepo.InsertManyAsync(remainingCandles);
            await stockSyncRepo.UpsertStockSyncStatusAsync(stockSyncStatus);
        }

        return totalStockHistory;
    }

    /// <summary>
    /// Fetches candles from Zerodha for the given stock and date range. Does NOT persist.
    /// </summary>
    private async Task<List<StockCandleRecord>> FetchCandlesFromZerodhaAsync(
        StockRecord stock,
        DateTime fromDate,
        DateTime toDate)
    {
        var instrumentToken = stock.InstrumentToken;
        var client = httpClientFactory.CreateClient(Constants.ZerodhaHistoricalClient);

        string? encToken = await encTokenProvider.GetAsync();
        if (string.IsNullOrEmpty(encToken))
        {
            throw new InvalidOperationException(
                "You are not authorized to perform this operation.");
        }

        // Build request explicitly so the shared HttpClient (from IHttpClientFactory) is not mutated
        // by concurrent callers. DefaultRequestHeaders on a named client are NOT thread-safe.
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{Constants.HistoricalEndpoint}/{instrumentToken}/day?user_id=TL0092&oi=1&from={fromDate:yyyy-MM-dd}&to={toDate:yyyy-MM-dd}");
        request.Headers.TryAddWithoutValidation("Authorization", $"enctoken {encToken}");

        var response = await client.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            // Token likely expired mid-run. Invalidate so the next call forces a refresh.
            await encTokenProvider.InvalidateAsync();
            throw new UnauthorizedAccessException(
                "Zerodha rejected the enctoken. It has been invalidated; supply a fresh one and retry.");
        }

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var responseData = JsonConvert.DeserializeObject<Response<ZerodhaStockHistoryDataDTO>>(json);

        return responseData?.Data?.Candles?
            .Select(c => new StockCandleRecord
            {
                StockId = stock.Id,
                Timestamp = c.Time,
                Open = c.Open,
                High = c.High,
                Low = c.Low,
                Close = c.Close,
                Volume = c.Volume
            })
            .ToList()
            ?? new List<StockCandleRecord>();
    }

    private static DateTime GetFromDate(StockSyncStatusRecord? syncStatus)
    {
        if (syncStatus == null || syncStatus.LastCandleTimestamp == default)
        {
            return DateTime.Today.AddYears(-5);
        }
        return syncStatus.LastCandleTimestamp.Date.AddDays(1);
    }
}