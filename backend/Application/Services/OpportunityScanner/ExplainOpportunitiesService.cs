using System.Text;
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
/// Explains the opportunity for a single stock by:
/// 1. Retrieving pre-computed OpportunityResult from Redis cache (or computing on-the-fly)
/// 2. Building a structured prompt from the scored data
/// 3. Sending it to the AI service for natural-language interpretation
/// 4. Parsing the AI response into a structured ExplainOpportunityResponse
/// </summary>
public class ExplainOpportunitiesService : IExplainOpportunitiesService
{
    private const int CandleLookbackDays = 220;
    private const string CacheKeyPrefix = "opportunity:";

    private readonly StockSenseAiContext _db;
    private readonly ILogger<ExplainOpportunitiesService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IStockSenseService _stockSenseService;

    public ExplainOpportunitiesService(
        StockSenseAiContext db,
        ILogger<ExplainOpportunitiesService> logger,
        [FromKeyedServices("redis")] ICacheService cacheService,
        IStockSenseService stockSenseService)
    {
        _db = db;
        _logger = logger;
        _cacheService = cacheService;
        _stockSenseService = stockSenseService;
    }

    public async Task<ExplainOpportunityResponse> ExplainOpportunitiesAsync(
        string stockName,
        CancellationToken cancellationToken = default)
    {
        // 1. Get the OpportunityResult (from cache or compute on-the-fly)
        var opportunityResult = await GetOpportunityResultAsync(stockName, cancellationToken);

        if (opportunityResult == null)
        {
            return new ExplainOpportunityResponse
            {
                Summary = $"Unable to explain opportunity for '{stockName}'. Stock not found or insufficient data.",
                Strengths = new List<string>(),
                Risks = new List<string>(),
                Overall = "No data available for analysis."
            };
        }

        // 2. Build the AI prompt from the opportunity data
        var prompt = BuildExplanationPrompt(opportunityResult);

        // 3. Send to AI service
        _logger.LogInformation("ExplainOpportunities: requesting AI explanation for {StockName}", stockName);
        var aiResponse = await _stockSenseService.GetAIResponseAsync(prompt);

        // 4. Parse the AI response into structured format
        return ParseAIResponse(aiResponse, stockName);
    }

    /// <summary>
    /// Retrieves the OpportunityResult from Redis cache, or computes it on-the-fly if not cached.
    /// </summary>
    private async Task<OpportunityResult?> GetOpportunityResultAsync(string stockName, CancellationToken cancellationToken)
    {
        // Try Redis cache first
        var cacheKey = $"{CacheKeyPrefix}{stockName.ToLowerInvariant()}";
        var cachedJson = await _cacheService.GetCachedDataAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedJson))
        {
            _logger.LogInformation("ExplainOpportunities: cache hit for {StockName}", stockName);
            var cachedResult = JsonSerializer.Deserialize<OpportunityResult>(cachedJson);
            if (cachedResult != null)
                return cachedResult;
        }

        // Cache miss — compute on-the-fly
        _logger.LogInformation("ExplainOpportunities: cache miss for {StockName}, computing on-the-fly", stockName);

        var stock = await _db.Stocks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IsActive && s.Symbol.ToLower() == stockName.ToLower(), cancellationToken);

        if (stock == null)
        {
            _logger.LogWarning("ExplainOpportunities: stock {StockName} not found or inactive", stockName);
            return null;
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
            return null;
        }

        var last = candles[^1];
        if (last.Close <= 0)
        {
            _logger.LogWarning("ExplainOpportunities: invalid latest close for {StockName}", stockName);
            return null;
        }

        var metrics = MetricCalculator.Compute(stock, candles);
        var scored = OpportunityScorer.Score(metrics);
        scored.Rank = 1;

        return scored;
    }

    /// <summary>
    /// Builds a structured prompt for the AI service that requests JSON output.
    /// </summary>
    private static string BuildExplanationPrompt(OpportunityResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are explaining stock-market data to an investor.");
        sb.AppendLine();
        sb.AppendLine("Do not provide buy/sell recommendations.");
        sb.AppendLine();
        sb.AppendLine("Explain why this stock received its opportunity score.");
        sb.AppendLine();
        sb.AppendLine("Clearly separate:");
        sb.AppendLine("1. What looks positive");
        sb.AppendLine("2. What should be watched");
        sb.AppendLine("3. Overall interpretation");
        sb.AppendLine();
        sb.AppendLine("Use ONLY the supplied data.");
        sb.AppendLine("Do not invent financial information.");
        sb.AppendLine();
        sb.AppendLine("Respond ONLY with valid JSON in this exact format (no markdown, no code fences):");
        sb.AppendLine("{");
        sb.AppendLine("  \"summary\": \"A brief 1-2 sentence summary of the stock's opportunity\",");
        sb.AppendLine("  \"strengths\": [\"strength 1\", \"strength 2\", ...],");
        sb.AppendLine("  \"risks\": [\"risk 1\", \"risk 2\", ...],");
        sb.AppendLine("  \"overall\": \"Overall interpretation paragraph\"");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"Stock:");
        sb.AppendLine($"{result.Symbol}");
        sb.AppendLine();
        sb.AppendLine($"Opportunity Score:");
        sb.AppendLine($"{result.OpportunityScore}");
        sb.AppendLine();
        sb.AppendLine($"Momentum Score:");
        sb.AppendLine($"{result.Scores.MomentumScore}");
        sb.AppendLine();
        sb.AppendLine($"Trend Score:");
        sb.AppendLine($"{result.Scores.TrendScore}");
        sb.AppendLine();
        sb.AppendLine($"Volume Score:");
        sb.AppendLine($"{result.Scores.VolumeScore}");
        sb.AppendLine();
        sb.AppendLine($"Risk Score:");
        sb.AppendLine($"{result.Scores.RiskScore}");
        sb.AppendLine();
        sb.AppendLine($"Signals:");
        foreach (var signal in result.Signals)
        {
            sb.AppendLine(signal);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses the AI response JSON into an ExplainOpportunityResponse.
    /// Falls back to a raw-text response if JSON parsing fails.
    /// </summary>
    private ExplainOpportunityResponse ParseAIResponse(string aiResponse, string stockName)
    {
        try
        {
            // Strip markdown code fences if present (```json ... ```)
            var cleaned = aiResponse.Trim();
            if (cleaned.StartsWith("```"))
            {
                var firstNewline = cleaned.IndexOf('\n');
                if (firstNewline > 0)
                    cleaned = cleaned[(firstNewline + 1)..];
                if (cleaned.EndsWith("```"))
                    cleaned = cleaned[..^3].Trim();
            }

            var parsed = JsonSerializer.Deserialize<ExplainOpportunityResponse>(cleaned, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed != null)
                return parsed;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "ExplainOpportunities: failed to parse AI JSON response for {StockName}, returning raw text", stockName);
        }

        // Fallback: return the raw AI response as the summary
        return new ExplainOpportunityResponse
        {
            Summary = aiResponse,
            Strengths = new List<string>(),
            Risks = new List<string>(),
            Overall = ""
        };
    }
}