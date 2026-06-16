using Helpers;
using Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

/// <summary>
/// Controller for fetching stock-related information such as fetching stock historical data.
/// </summary>
/// <param name="logger"></param>
[Route("api/[controller]")]
[ApiController]
public class StockDataController(ILogger<StockDataController> logger, IZerodhaHelper zerodhaHelper) : ControllerBase
{
    private ILogger<StockDataController> _logger = logger;
    private IZerodhaHelper _zerodhaHelper = zerodhaHelper;

    /// <summary>
    /// Fetches stock historical data for a given stock symbol.
    /// </summary>
    /// <param name="stockSymbol">The stock symbol to fetch historical data for.</param>
    /// <returns>Stock historical data for the specified symbol.</returns>
    [HttpGet]
    public async Task<IActionResult> GetStockData([FromQuery] string stockSymbol)
    {
        _logger.LogInformation("Received request for stock historical data with symbol: {stockSymbol}", stockSymbol);
        try
        {
            await _zerodhaHelper.GetStockHistoricalData(stockSymbol);
            return Ok();
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}