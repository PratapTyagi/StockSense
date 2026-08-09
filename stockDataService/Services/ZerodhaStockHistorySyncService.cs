using Newtonsoft.Json;
using Interfaces;
using StockDataService.Entities;
using System.Numerics;
using Microsoft.AspNetCore.Http.HttpResults;

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
    /// Zerodha's kite historical API is aggressively rate-limited. Empirically, spacing
    /// consecutive requests by ~3 seconds keeps us under the threshold and avoids HTTP 429.
    /// </summary>
    private static readonly TimeSpan ZerodhaCallInterval = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Timestamp of the last outbound call to Zerodha's HistoricalEndpoint. Used to enforce
    /// that consecutive requests are spaced at least <see cref="ZerodhaCallInterval"/> apart.
    /// Instance-level (not static) because this service is registered per-scope and one sync
    /// run is serialized end-to-end.
    /// </summary>
    private DateTime _lastZerodhaCallUtc = DateTime.MinValue;

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
                List<StockCandleRecord>? candles = await FetchCandlesFromZerodhaAsync(stock, fromDate, toDate);
                if (candles == null)
                {
                    continue;
                }

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
                    updatedSyncStatus.LastCandleTimestamp = DateTime.Today;
                }

                await stockSyncRepo.UpsertStockSyncStatusAsync(updatedSyncStatus);

                if (candles.Count == 0)
                {
                    continue;
                }

                await stockCandlesRepo.InsertManyAsync(candles);

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

        List<StockCandleRecord>? remainingCandles = await FetchCandlesFromZerodhaAsync(stockSyncStatus.Stock, stockHistoryInDatabase.FirstOrDefault()?.Timestamp ?? fromDate, DateTime.Today);
        if (remainingCandles == null)
        {
            return new List<StockCandleRecord>();
        }
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
    /// Enforces that consecutive calls to Zerodha's HistoricalEndpoint are spaced at least
    /// <see cref="ZerodhaCallInterval"/> (3 seconds) apart to avoid HTTP 429 responses.
    /// </summary>
    private async Task<List<StockCandleRecord>?> FetchCandlesFromZerodhaAsync(
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

        // Rate-limit guard: ensure at least ZerodhaCallInterval has elapsed since the previous
        // request to HistoricalEndpoint. This is done immediately before dispatching so the
        // very first call in a run is not delayed unnecessarily.
        await WaitForZerodhaRateLimitAsync();
        _lastZerodhaCallUtc = DateTime.UtcNow;

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

        try
        {
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
        catch (System.Exception)
        {
            logger.LogError("Error fetching {stock} historical data.", stock.Symbol);
            return null;
        }
    }

    /// <summary>
    /// Blocks until at least <see cref="ZerodhaCallInterval"/> has elapsed since the
    /// previous Zerodha HistoricalEndpoint request. No-op on the very first call.
    /// </summary>
    private async Task WaitForZerodhaRateLimitAsync()
    {
        if (_lastZerodhaCallUtc == DateTime.MinValue)
        {
            return;
        }

        TimeSpan elapsed = DateTime.UtcNow - _lastZerodhaCallUtc;
        if (elapsed < ZerodhaCallInterval)
        {
            TimeSpan wait = ZerodhaCallInterval - elapsed;
            await Task.Delay(wait);
        }
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