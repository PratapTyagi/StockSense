using Application.Interfaces;
using Application.Models.OpportunityScanner;
using Microsoft.AspNetCore.Mvc;

namespace backend.Infrastructure.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OpportunitiesController(
    ILogger<OpportunitiesController> logger,
    IOpportunityScannerService scanner,
    IOpportunityExplanationService explanationService) : ControllerBase
{
    private readonly ILogger<OpportunitiesController> _logger = logger;
    private readonly IOpportunityScannerService _scanner = scanner;
    private readonly IOpportunityExplanationService _explanationService = explanationService;

    /// <summary>
    /// Returns the top-N ranked stocks by OpportunityScore.
    /// </summary>
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

            var results = await _scanner.GetOpportunitiesAsync(request, cancellationToken);
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
    /// </summary>
    /// <param name="stockName">Stock symbol (e.g. "RELIANCE", "3IINFOLTD")</param>
    [HttpGet("{stockName}/explanation")]
    public async Task<IActionResult> ExplainOpportunity(string stockName, CancellationToken cancellationToken = default)
    {
        try
        {
            var opportunity = await _scanner.GetOpportunityForStockAsync(stockName, cancellationToken);

            if (opportunity == null)
                return NotFound($"No opportunity data found for stock '{stockName}'.");

            var explanation = await _explanationService.GenerateExplanationAsync(opportunity);
            return Ok(explanation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Explain opportunity failed for {StockName}", stockName);
            return StatusCode(500, "An error occurred while explaining the opportunity.");
        }
    }
}