using Application.Interfaces;
using Application.Models.StockDataService;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Controllers;
/// <summary>
/// Controller for handling stock-related operations such as fetching stock data, performing analytics, etc.
/// </summary>
/// <param name="logger"></param>
[Route("api/[controller]")]
[ApiController]
public class EncTokenController(ILogger<EncTokenController> logger, IStockDataService stockDataService): ControllerBase
{
    private readonly ILogger<EncTokenController> _logger = logger;
    private readonly IStockDataService _stockDataService = stockDataService;

    [HttpPost()]
    public async Task<IActionResult> PutEncToken([FromBody] EncTokenDto item)
    {
        _logger.LogInformation("Received an encToken");

        try
        {
            await _stockDataService.PushEncToken(item.Token);
            return Ok();
        }
        catch (System.Exception)
        {
            return StatusCode(500, "An error occurred while adding auth token.");
        }
    }
}