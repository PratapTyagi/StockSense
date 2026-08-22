using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Application.Interfaces;
using Application.Models.OpportunityScanner;
using Application.Services;

namespace Infrastructure.Controllers;
/// <summary>
/// Controller for handling stock-related operations such as fetching stock data, performing analytics, etc.
/// </summary>
/// <param name="logger"></param>
[Route("api/[controller]")]
[ApiController]
public class StockController(ILogger<StockController> logger, IStockService stockService, IStockSenseService stockSenseService, IOpportunityScannerService scanner) : ControllerBase
{
    private readonly ILogger<StockController> _logger = logger;
    private readonly IStockService _stockService = stockService;
    private readonly IStockSenseService _stockSenseService = stockSenseService;
    private readonly IOpportunityScannerService _scanner = scanner;

    /// <summary>
    /// Fetches stock details for a given stock name.
    /// </summary>
    /// <param name="name">The stock name to fetch details for.</param>
    /// <returns>Stock details for the specified name.</returns>
    [HttpGet]
    public async Task<IActionResult> GetStockDetails([FromQuery] string name)
    {
        _logger.LogInformation("Received request for stock details with name: {name}", name);
        try
        {
            var stockData = await _stockService.GetStockDetailsAsync(name);
            if (stockData == null)
            {
                return NotFound($"No stock data found for name: {name}");
            }
            return Ok(stockData);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching stock data.");
        }
    }

    /// <summary>
    /// Compares stock data for multiple tickers.
    /// </summary>
    /// <param name="tickers">Comma-separated list of stock tickers to compare.</param>
    /// <returns>Comparison results for the specified tickers.</returns>
    [HttpGet("compare")]
    public async Task<IActionResult> Compare([FromQuery] string tickers)
    {
        _logger.LogInformation("Received request to compare stocks with tickers: {tickers}", string.Join(", ", tickers));
        try
        {
            var result = await _stockSenseService.CompareStockAsync(tickers.Split(',').Select(t => t.Trim()).ToArray());
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while comparing stocks.");
        }
    }

    /// <summary>
    /// Fetches historical stock data for a given stock symbol and range.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string name,
        [FromQuery] string range = "1M")
    {
        try
        {
            var result = await _stockService.GetStockHistoryAsync(name, range);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching stock history.");
        }
    }

    /// <summary>
    /// Fetches trending stocks.
    /// </summary>
    /// <returns>A list of trending stocks.</returns>
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingStocks()
    {
        try
        {
            var result = await _stockService.GetTrendingStocksAsync();
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching trending stocks.");
        }
    }
}