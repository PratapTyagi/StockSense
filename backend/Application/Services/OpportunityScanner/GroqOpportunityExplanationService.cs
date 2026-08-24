using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Interfaces;
using Application.Models.OpportunityScanner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Services.OpportunityScanner;

/// <summary>
/// Groq-based implementation of IOpportunityExplanationService.
/// Calls the Groq API (OpenAI-compatible) with openai/gpt-oss-20b model.
/// </summary>
public class GroqOpportunityExplanationService : IOpportunityExplanationService
{
    private readonly string _groqApiUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GroqOpportunityExplanationService> _logger;
    private readonly ICacheService _cacheService;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _cacheKeyPrefix = "opportunity:";
    private readonly string _cacheKeySuffix = ":explanation";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromDays(1);

    public GroqOpportunityExplanationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        [FromKeyedServices("redis")] ICacheService cacheService,
        ILogger<GroqOpportunityExplanationService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _groqApiUrl = configuration["Groq:Url"] ?? throw new InvalidOperationException("Groq API Url not configured. Set 'Groq:Url' in appsettings.");
        _apiKey = configuration["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq API key not configured. Set 'Groq:ApiKey' in appsettings.");
        _model = configuration["Groq:Model"] ?? "openai/gpt-oss-20b";
        _cacheService = cacheService;
    }

    public async Task<ExplainOpportunityResponse> GenerateExplanationAsync(OpportunityResult opportunity)
    {
        var cacheKey = getCacheKey(opportunity.Symbol);
        var cached = await _cacheService.GetCachedDataAsync<ExplainOpportunityResponse>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Returning cached explanation for {Symbol}", opportunity.Symbol);
            return cached;
        }

        var prompt = BuildPrompt(opportunity);

        _logger.LogInformation("GroqExplanation: requesting explanation for {Symbol}", opportunity.Symbol);

        var aiResponse = await CallGroqAsync(prompt);
        var result = ParseResponse(aiResponse, opportunity.Symbol);

        // Cache the parsed object so retrieval deserializes cleanly
        await _cacheService.SetCachedDataAsync(cacheKey, result, CacheExpiration);

        return result;
    }

    private async Task<string> CallGroqAsync(string prompt)
    {
        var requestBody = new
        {
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            model = _model
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(HttpMethod.Post, _groqApiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Groq API error ({StatusCode}): {Response}", response.StatusCode, responseJson);
            throw new HttpRequestException($"Groq API returned {response.StatusCode}: {responseJson}");
        }

        var doc = JsonDocument.Parse(responseJson);

        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return text ?? string.Empty;
    }

    private static string BuildPrompt(OpportunityResult result)
    {
        var m = result.Metrics;
        var sb = new StringBuilder();

        // System instructions
        sb.AppendLine("You are explaining stock-market scanner data to an investor.");
        sb.AppendLine();
        sb.AppendLine("IMPORTANT SCORE SEMANTICS:");
        sb.AppendLine();
        sb.AppendLine("All scores range from 0 to 100.");
        sb.AppendLine("Higher Momentum Score = stronger recent momentum.");
        sb.AppendLine("Higher Trend Score = stronger technical trend.");
        sb.AppendLine("Higher Volume Score = stronger volume confirmation.");
        sb.AppendLine("Higher Risk Score = LOWER risk / healthier risk profile.");
        sb.AppendLine();
        sb.AppendLine("IMPORTANT:");
        sb.AppendLine("Risk Score is NOT a risk level. It is an inverse risk measure.");
        sb.AppendLine("Risk Score 90 = relatively favorable/low-risk profile.");
        sb.AppendLine("Risk Score 50 = moderate risk profile.");
        sb.AppendLine("Risk Score 20 = relatively high-risk profile.");
        sb.AppendLine("Never describe a high Risk Score as \"high risk\".");
        sb.AppendLine();
        sb.AppendLine("Opportunity Score is NOT a prediction of future returns or upside.");
        sb.AppendLine("Do not say a stock has \"upside potential\" because its Opportunity Score is high.");
        sb.AppendLine("The Opportunity Score represents the quality of the stock's current");
        sb.AppendLine("momentum, trend, volume and risk signals according to the scanner.");
        sb.AppendLine();
        sb.AppendLine("Do not make buy/sell recommendations.");
        sb.AppendLine("Do not predict future prices.");
        sb.AppendLine("Do not invent information that isn't provided.");
        sb.AppendLine();
        sb.AppendLine("Use neutral language such as:");
        sb.AppendLine("- \"stronger technical profile\"");
        sb.AppendLine("- \"positive momentum\"");
        sb.AppendLine("- \"weaker momentum\"");
        sb.AppendLine("- \"higher volatility\"");
        sb.AppendLine("- \"lower risk profile\"");
        sb.AppendLine("- \"potentially overextended\"");
        sb.AppendLine("- \"moderate overall score\"");
        sb.AppendLine("- \"weaker longer-term trend\" (not \"downtrend\")");
        sb.AppendLine();
        sb.AppendLine("STYLE RULES:");
        sb.AppendLine("- Each strength or watch point should describe ONE signal. Do not combine unrelated signals.");
        sb.AppendLine("- If returns are positive but the Momentum Score is low, explain why (e.g. returns may be modest relative to the scoring scale).");
        sb.AppendLine("- Do not reference \"market context\" or any information not explicitly provided in the data.");
        sb.AppendLine("- Say \"weaker longer-term trend\" instead of \"downtrend\".");
        sb.AppendLine("- Say \"indicates\" instead of \"suggests\" or \"confirms\".");
        sb.AppendLine();

        // Output format
        sb.AppendLine("Respond ONLY with valid JSON in this exact format (no markdown, no code fences):");
        sb.AppendLine("{");
        sb.AppendLine("  \"summary\": \"1-2 sentence neutral summary of the stock's scanner profile\",");
        sb.AppendLine("  \"strengths\": [\"strength 1\", \"strength 2\"],");
        sb.AppendLine("  \"watchPoints\": [\"watch point 1\", \"watch point 2\"],");
        sb.AppendLine("  \"overall\": \"1-2 sentence overall interpretation\"");
        sb.AppendLine("}");
        sb.AppendLine();

        // Data section
        sb.AppendLine("--- DATA ---");
        sb.AppendLine();
        sb.AppendLine($"Stock: {result.Symbol}");
        sb.AppendLine();
        sb.AppendLine("Scores:");
        sb.AppendLine($"  Opportunity Score: {result.OpportunityScore}/100");
        sb.AppendLine($"  Momentum Score: {result.Scores.MomentumScore}/100");
        sb.AppendLine($"  Trend Score: {result.Scores.TrendScore}/100");
        sb.AppendLine($"  Volume Score: {result.Scores.VolumeScore}/100");
        sb.AppendLine($"  Risk Score: {result.Scores.RiskScore}/100 (higher = lower risk)");
        if (result.OverextensionPenalty > 0)
            sb.AppendLine($"  Overextension Penalty: -{result.OverextensionPenalty}");
        sb.AppendLine();

        // Raw metrics
        sb.AppendLine("Underlying Metrics:");
        sb.AppendLine($"  Latest Close: {m.LatestClose}");
        if (m.OneMonthReturnPct.HasValue)
            sb.AppendLine($"  1-Month Return: {m.OneMonthReturnPct:F2}%");
        if (m.ThreeMonthReturnPct.HasValue)
            sb.AppendLine($"  3-Month Return: {m.ThreeMonthReturnPct:F2}%");
        if (m.SixMonthReturnPct.HasValue)
            sb.AppendLine($"  6-Month Return: {m.SixMonthReturnPct:F2}%");
        if (m.DistanceFromSma50Pct.HasValue)
            sb.AppendLine($"  Price vs SMA50: {m.DistanceFromSma50Pct:F2}%");
        if (m.DistanceFromSma200Pct.HasValue)
            sb.AppendLine($"  Price vs SMA200: {m.DistanceFromSma200Pct:F2}%");
        if (m.Sma50VsSma200Pct.HasValue)
            sb.AppendLine($"  SMA50 vs SMA200: {m.Sma50VsSma200Pct:F2}%");
        if (m.VolumeRatio.HasValue)
            sb.AppendLine($"  Volume Ratio (recent/avg): {m.VolumeRatio:F2}x");
        if (m.AnnualizedVolatilityPct.HasValue)
            sb.AppendLine($"  Annualized Volatility: {m.AnnualizedVolatilityPct:F2}%");
        sb.AppendLine();

        // Signals
        sb.AppendLine("Signals:");
        if (result.Signals.Count > 0)
        {
            foreach (var signal in result.Signals)
                sb.AppendLine($"  - {signal}");
        }
        else
        {
            sb.AppendLine("  (none)");
        }

        return sb.ToString();
    }

    private ExplainOpportunityResponse ParseResponse(string aiResponse, string symbol)
    {
        try
        {
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
            _logger.LogWarning(ex, "GroqExplanation: failed to parse JSON response for {Symbol}, returning raw text", symbol);
        }

        return new ExplainOpportunityResponse
        {
            Summary = aiResponse,
            Strengths = new List<string>(),
            WatchPoints = new List<string>(),
            Overall = ""
        };
    }

    /// <summary>
    /// Get cache key name
    /// </summary>
    /// <param name="stockName"></param>
    /// <returns></returns>
    private string getCacheKey(string stockName) => $"{_cacheKeyPrefix}{stockName.ToLowerInvariant()}{_cacheKeySuffix}";
}