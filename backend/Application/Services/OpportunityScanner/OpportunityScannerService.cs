using System.Text.Json;
using Application.Interfaces;
using Application.Models.OpportunityScanner;
using Domain.Contexts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Services.OpportunityScanner;

/// <summary>
/// End-to-end Opportunity Scanner pipeline:
///
///     Stocks → Symbol eligibility → Load candles → Data eligibility
///            → Compute metrics → Score → Rank → Top N
///
/// Data is sourced exclusively from the local Stocks and StockCandles tables.
/// Results are cached in Redis for 1 day so that ExplainOpportunitiesAsync
/// can retrieve pre-computed data for individual stocks.
/// </summary>
public class OpportunityScannerService : IOpportunityScannerService
{
    private const int CandleLookbackDays = 220;
    private const string CacheKeyPrefix = "opportunity:";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromDays(1);

    /// <summary>
    /// Minimum average daily traded value (₹) for a stock to be considered liquid enough.
    /// Configurable — set conservatively low so we don't arbitrarily exclude stocks
    /// without empirical evidence. Can be tuned once real data is observed.
    /// </summary>
    private const double MinAverageDailyTradedValue = 500000; // ₹5 lakh

    /// <summary>
    /// Special trading series suffixes that should be excluded from opportunity ranking.
    /// These represent restricted/illiquid trading categories on Indian exchanges.
    /// </summary>
    private static readonly string[] ExcludedSuffixes = { "-BE", "-SM", "-ST" };

    private readonly StockSenseAiContext _db;
    private readonly ILogger<OpportunityScannerService> _logger;
    private readonly ICacheService _cacheService;

    public OpportunityScannerService(
        StockSenseAiContext db,
        ILogger<OpportunityScannerService> logger,
        [FromKeyedServices("redis")] ICacheService cacheService)
    {
        _db = db;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<IReadOnlyList<OpportunityResult>> GetOpportunitiesAsync(
        OpportunityScannerRequest request,
        CancellationToken cancellationToken = default)
    {
        var top = request.Top <= 0 ? 20 : request.Top;

        // 1. Universe: active stocks (optionally filtered by exchange).
        var stocksQuery = _db.Stocks
            .AsNoTracking()
            .Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Exchange))
        {
            var exchange = request.Exchange.Trim();
            stocksQuery = stocksQuery.Where(s => s.Exchange == exchange);
        }

        var allStocks = await stocksQuery.ToListAsync(cancellationToken);
        if (allStocks.Count == 0)
        {
            _logger.LogInformation("Opportunity scan: no active stocks in universe.");
            return Array.Empty<OpportunityResult>();
        }

        // 2. Symbol-level eligibility filter (before loading candles to save I/O).
        var eligibleStocks = allStocks
            .Where(s => IsSymbolEligible(s.Symbol))
            .ToList();

        _logger.LogInformation(
            "Opportunity scan starting: universe={UniverseSize}, after symbol filter={Eligible}, top={Top}",
            allStocks.Count, eligibleStocks.Count, top);

        if (eligibleStocks.Count == 0)
            return Array.Empty<OpportunityResult>();

        // 3. Bulk-load candle history for eligible stocks only.
        //    Filter by stockIds to avoid loading the entire table.
        var stockIds = eligibleStocks.Select(s => s.Id).ToArray();
        var cutoff = DateTime.UtcNow.AddDays(-(CandleLookbackDays * 2)); // calendar buffer

        var candlesByStock = await _db.StockCandles
            .AsNoTracking()
            .Where(c => stockIds.Contains(c.StockId) && c.Timestamp >= cutoff)
            .OrderBy(c => c.StockId).ThenBy(c => c.Timestamp)
            .ToListAsync(cancellationToken);

        var grouped = candlesByStock
            .GroupBy(c => c.StockId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<StockCandle>)g.ToList());

        // 4. Metric + score pipeline.
        var results = new List<OpportunityResult>(eligibleStocks.Count);
        int skippedNoData = 0, skippedInsufficient = 0, skippedInvalidClose = 0, skippedIlliquid = 0;

        foreach (var stock in eligibleStocks)
        {
            if (!grouped.TryGetValue(stock.Id, out var candles) || candles.Count == 0)
            {
                skippedNoData++;
                continue;
            }

            var eligibility = CheckDataEligibility(candles);
            if (eligibility != DataEligibility.Eligible)
            {
                switch (eligibility)
                {
                    case DataEligibility.InsufficientHistory: skippedInsufficient++; break;
                    case DataEligibility.InvalidLatestClose: skippedInvalidClose++; break;
                }
                continue;
            }

            var metrics = MetricCalculator.Compute(stock, candles);

            // Liquidity filter (post-metric, since we need AverageDailyTradedValue)
            if (metrics.AverageDailyTradedValue is not null && metrics.AverageDailyTradedValue.Value < MinAverageDailyTradedValue)
            {
                skippedIlliquid++;
                continue;
            }

            var scored = OpportunityScorer.Score(metrics);

            // Low liquidity warning (above threshold but still relatively low)
            if (metrics.AverageDailyTradedValue is not null && metrics.AverageDailyTradedValue.Value < MinAverageDailyTradedValue * 5)
            {
                scored.Signals.Add("Low liquidity");
            }

            if (request.MinScore is { } minScore && scored.OpportunityScore < minScore) continue;
            results.Add(scored);

            // Cache each opportunity result in Redis with 1-day expiration
            var cacheKey = $"{CacheKeyPrefix}{stock.Symbol.ToLowerInvariant()}";
            await _cacheService.SetCachedDataAsync(cacheKey, scored, CacheExpiration);
        }

        _logger.LogInformation(
            "Opportunity scan done: scored={Scored}, skipped_no_data={NoData}, skipped_insufficient={Insufficient}, skipped_invalid_close={Invalid}, skipped_illiquid={Illiquid}",
            results.Count, skippedNoData, skippedInsufficient, skippedInvalidClose, skippedIlliquid);

        // 5. Rank descending and take top-N.
        var ranked = results
            .OrderByDescending(r => r.OpportunityScore)
            .ThenByDescending(r => r.Scores.MomentumScore)
            .ThenBy(r => r.Symbol) // deterministic tie-breaking
            .Take(top)
            .ToList();

        // Assign rank numbers
        for (int i = 0; i < ranked.Count; i++)
            ranked[i].Rank = i + 1;

        return ranked;
    }

    // =========================================================================
    // ELIGIBILITY
    // =========================================================================

    /// <summary>
    /// Symbol-level filter: excludes special trading series.
    /// </summary>
    internal static bool IsSymbolEligible(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol)) return false;

        foreach (var suffix in ExcludedSuffixes)
        {
            if (symbol.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private enum DataEligibility { Eligible, InsufficientHistory, InvalidLatestClose, InvalidVolume }

    /// <summary>
    /// Data-level eligibility: enough history, valid close, valid volume.
    /// </summary>
    private static DataEligibility CheckDataEligibility(IReadOnlyList<StockCandle> candles)
    {
        if (candles.Count < MetricCalculator.MinCandlesRequired)
            return DataEligibility.InsufficientHistory;

        var last = candles[^1];
        if (last.Close <= 0)
            return DataEligibility.InvalidLatestClose;

        if (last.Volume < 0)
            return DataEligibility.InvalidVolume;

        return DataEligibility.Eligible;
    }
}