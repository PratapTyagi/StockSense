using Application.Models.OpportunityScanner;

namespace Application.Interfaces;

/// <summary>
/// Strategy interface for generating AI explanations of stock opportunities.
/// Implementations can target different LLM providers (Groq, Ollama, Cloud, etc.)
/// </summary>
public interface IOpportunityExplanationService
{
    Task<ExplainOpportunityResponse> GenerateExplanationAsync(OpportunityResult opportunity);
}