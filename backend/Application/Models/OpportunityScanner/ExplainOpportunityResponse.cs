using System.Text.Json.Serialization;

namespace Application.Models.OpportunityScanner;

/// <summary>
/// Structured AI explanation of why a stock received its opportunity score.
/// </summary>
public class ExplainOpportunityResponse
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("risks")]
    public List<string> Risks { get; set; } = new();

    [JsonPropertyName("overall")]
    public string Overall { get; set; } = string.Empty;
}