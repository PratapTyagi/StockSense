using Application.Models.OpportunityScanner;

namespace Application.Interfaces;

/// <summary>
/// Explains why a stock received its opportunity score by building a prompt
/// from the scored data and sending it to the AI service for interpretation.
/// </summary>
public interface IExplainOpportunitiesService
{
    Task<ExplainOpportunityResponse> ExplainOpportunitiesAsync(
        string stockName,
        CancellationToken cancellationToken = default);
}