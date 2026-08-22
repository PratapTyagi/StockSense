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
/// Explains the opportunity for a single stock by retrieving pre-computed data
/// from Redis cache (stored by GetOpportunitiesAsync) or computing it on-the-fly
/// if not cached.
/// </summary>
public class ExplainOpportunitiesService : IExplainOpportunitiesService
{
    private const int CandleLookbackDays = 220;
    private const string CacheKeyPrefix = "opportunity:";

    private readonly StockSenseAiContext _db;
    private readonly ILogger<ExplainOpportunitiesService> _logger;
    private readonly ICacheService _cacheService;

    public ExplainOpportunitiesService(
        StockSenseAiContext db,
        ILogger<ExplainOpportunitiesService> logger,
        [FromKeyedServices("redis")] ICacheService cacheService)
    {
        _db = db;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<IReadOnlyList<OpportunityResult>> ExplainOpportunitiesAsync(
        string stockName,
        CancellationToken cancellationToken = default)
    {
        // 1. Try to fetch pre-computed opportunity data from Redis cache
        var cacheKey = $"{CacheKeyPrefix}{stockName.ToLowerInvariant()}";
        var cachedJson = await _cacheService.GetCachedDataAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedJson))
        {
            _logger.LogInformation("ExplainOpportunities: cache hit for {StockName}", stockName);
            var cachedResult = JsonSerializer.Deserialize<OpportunityResult>(cachedJson);
            if (cachedResult != null)
            {
                return new List<OpportunityResult> { cachedResult };
            }
        }

        // 2. Cache miss — compute metrics for the single stock
        _logger.LogInformation("ExplainOpportunities: cache miss for {StockName}, computing on-the-fly", stockName);

        var stock = await _db.Stocks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IsActive && s.Symbol.ToLower() == stockName.ToLower(), cancellationToken);

        if (stock == null)
        {
            _logger.LogWarning("ExplainOpportunities: stock {StockName} not found or inactive", stockName);
            return Array.Empty<OpportunityResult>();
        }

        var cutoff = DateTime.UtcNow.AddDays(-(CandleLookbackDays * 2));
        var candles = await _db.StockCandles
            .AsNoTracking()
            .Where(c => c.StockId == stock.Id && c.Timestamp >= cutoff)
            .OrderBy(c => c.Timestamp)
            .ToListAsync(cancellationToken);

        if (candles.Count < MetricCalculator.MinCandlesRequired)
        {
            _logger.LogWarning("ExplainOpportunities: insufficient candle data for {StockName} ({Count} candles)",
                stockName, candles.Count);
            return Array.Empty<OpportunityResult>();
        }

        var last = candles[^1];
        if (last.Close <= 0)
        {
            _logger.LogWarning("ExplainOpportunities: invalid latest close for {StockName}", stockName);
            return Array.Empty<OpportunityResult>();
        }

        var metrics = MetricCalculator.Compute(stock, candles);
        var scored = OpportunityScorer.Score(metrics);
        scored.Rank = 1;

        return new List<OpportunityResult> { scored };
    }
}