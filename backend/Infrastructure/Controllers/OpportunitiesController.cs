using Application.Interfaces;
using Application.Models.OpportunityScanner;
using Microsoft.AspNetCore.Mvc;

namespace backend.Infrastructure.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OpportunitiesController(ILogger<OpportunitiesController> logger, IOpportunityScannerService scanner, IExplainOpportunitiesService explainOpportunitiesService) : ControllerBase
{
    private readonly ILogger<OpportunitiesController> _logger = logger;
    private readonly IOpportunityScannerService _opportunityScannerService = scanner;
    private readonly IExplainOpportunitiesService _explainOpportunitiesService = explainOpportunitiesService;

    /// <summary>
    /// Returns the top-N ranked stocks by OpportunityScore.
    /// </summary>
    /// <param name="top">Number of results (default 20, max 200).</param>
    /// <param name="minScore">Optional minimum OpportunityScore (0-100).</param>
    /// <param name="exchange">Optional exchange filter (e.g. "NSE").</param>
    [HttpGet("")]
    public async Task<IActionResult> Opportunities(
        [FromQuery] int top = 20,
        [FromQuery] double? minScore = null,
        [FromQuery] string? exchange = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new OpportunityScannerRequest
            {
                Top = Math.Clamp(top, 1, 200),
                MinScore = minScore,
                Exchange = exchange,
            };

            var results = await _opportunityScannerService.GetOpportunitiesAsync(request, cancellationToken);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Opportunity scan failed");
            return StatusCode(500, "An error occurred while running the opportunity scan.");
        }
    }

    /// <summary>
    /// Explains why a stock received its opportunity score using AI interpretation.
    /// Returns a structured response with summary, strengths, risks, and overall interpretation.
    /// </summary>
    /// <param name="stockName">Stock symbol (e.g. "RELIANCE", "3IINFOLTD")</param>
    [HttpGet("{stockName}/explanation")]
    public async Task<IActionResult> ExplainOpportunities(string stockName, CancellationToken cancellationToken = default)
    {
        try
        {
            var explanation = await _explainOpportunitiesService.ExplainOpportunitiesAsync(stockName, cancellationToken);
            return Ok(explanation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Explain opportunity failed for {StockName}", stockName);
            return StatusCode(500, "An error occurred while explaining the opportunity.");
        }
    }
}