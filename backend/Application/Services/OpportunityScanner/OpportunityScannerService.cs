using Application.Interfaces;
using Application.Models.OpportunityScanner;
using Domain.Contexts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.OpportunityScanner;

/// <summary>
/// End-to-end Opportunity Scanner pipeline:
///
///     Stocks → Eligibility filter → Load candles → Compute metrics
///            → Score categories → Aggregate → Rank → Top N
///
/// Data is sourced exclusively from the local <c>Stocks</c> and <c>StockCandles</c>
/// tables (populated by the stockDataService), so this service does not depend on
/// any external market-data provider.
/// </summary>
public class OpportunityScannerService : IOpportunityScannerService
{
    // How many trading days of history we pull per stock. The scorer only needs
    // ~200 for SMA200, but pulling a small buffer keeps the query stable even if
    // a sync run misses a day or two.
    private const int CandleLookbackDays = 220;

    private readonly StockSenseAiContext _db;
    private readonly ILogger<OpportunityScannerService> _logger;

    public OpportunityScannerService(
        StockSenseAiContext db,
        ILogger<OpportunityScannerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OpportunityResult>> ScanAsync(
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

        var stocks = await stocksQuery.ToListAsync(cancellationToken);
        if (stocks.Count == 0)
        {
            _logger.LogInformation("Opportunity scan: no active stocks in universe.");
            return Array.Empty<OpportunityResult>();
        }

        _logger.LogInformation(
            "Opportunity scan starting: universe={UniverseSize} stocks, top={Top}",
            stocks.Count, top);

        // 2. Bulk-load the tail of history for every stock in a single query.
        //    We rely on the (StockId, Timestamp) index for efficient retrieval.
        var stockIds = stocks.Select(s => s.Id).ToArray();
        var cutoff = DateTime.UtcNow.AddDays(-(CandleLookbackDays * 2)); // ~2x calendar buffer for weekends/holidays

        var candlesByStock = await _db.StockCandles
            .AsNoTracking()
            .Where(c => c.Timestamp >= cutoff)
            .OrderBy(c => c.StockId).ThenBy(c => c.Timestamp)
            .ToListAsync(cancellationToken);

        var grouped = candlesByStock
            .GroupBy(c => c.StockId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<StockCandle>)g.ToList());

        // 3. Metric + score pipeline. This part is CPU-bound and pure, so we
        //    keep it sequential — it's plenty fast for a few thousand stocks.
        var results = new List<OpportunityResult>(stocks.Count);
        int skippedNoData = 0, skippedInsufficient = 0, skippedInvalidClose = 0;

        foreach (var stock in stocks)
        {
            if (!grouped.TryGetValue(stock.Id, out var candles) || candles.Count == 0)
            {
                skippedNoData++;
                continue;
            }

            if (!IsEligible(candles, out var reason))
            {
                if (reason == IneligibilityReason.InsufficientHistory) skippedInsufficient++;
                else if (reason == IneligibilityReason.InvalidLatestClose) skippedInvalidClose++;
                continue;
            }

            var metrics = MetricCalculator.Compute(stock, candles);
            var scored = OpportunityScorer.Score(metrics);

            if (request.MinScore is { } minScore && scored.OpportunityScore < minScore) continue;
            results.Add(scored);
        }

        _logger.LogInformation(
            "Opportunity scan done: scored={Scored}, skipped_no_data={NoData}, skipped_insufficient={Insufficient}, skipped_invalid_close={Invalid}",
            results.Count, skippedNoData, skippedInsufficient, skippedInvalidClose);

        // 4. Rank descending and take top-N.
        return results
            .OrderByDescending(r => r.OpportunityScore)
            .ThenByDescending(r => r.Scores.MomentumScore)
            .Take(top)
            .ToList();
    }

    // --- Eligibility ---------------------------------------------------------

    private enum IneligibilityReason { None, InsufficientHistory, InvalidLatestClose, InvalidVolume }

    /// <summary>
    /// Basic sanity filter — mirrors the spec's "Don't rank garbage data" rule.
    /// </summary>
    private static bool IsEligible(IReadOnlyList<StockCandle> candles, out IneligibilityReason reason)
    {
        if (candles.Count < MetricCalculator.MinCandlesRequired)
        {
            reason = IneligibilityReason.InsufficientHistory;
            return false;
        }

        var last = candles[^1];
        if (last.Close <= 0)
        {
            reason = IneligibilityReason.InvalidLatestClose;
            return false;
        }
        if (last.Volume < 0)
        {
            reason = IneligibilityReason.InvalidVolume;
            return false;
        }

        reason = IneligibilityReason.None;
        return true;
    }
}