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
public class StockDataController(ILogger<StockDataController> logger, IZerodhaHelper zerodhaHelper, IKiteStockSyncService kiteStockSyncService) : ControllerBase
{
    private ILogger<StockDataController> _logger = logger;
    private IZerodhaHelper _zerodhaHelper = zerodhaHelper;
    private IKiteStockSyncService _kiteStockSyncService = kiteStockSyncService;

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
            var data = await _zerodhaHelper.GetStockHistoricalData(stockSymbol, DateTime.Today.AddDays(-5 * 365)); // Fetch data for the last 5 years
            return Ok(data);
        }
        catch (System.Exception ex)
        {
            _logger.LogError("Error fetching stock data: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Syncs data for all stocks by fetching the Kite instruments CSV, filtering for NSE equities, ensuring each stock exists in the Stocks table, and creating a matching StockSyncStatus row if missing.
    /// This endpoint is intended to be called at application startup or when a manual sync is required. It will log the number of NSE equity instruments parsed and any errors encountered during the sync process
    /// </summary>
    /// <returns></returns>
    [HttpGet("/sync-all-stocks")]
    public async Task<IActionResult> SyncDataForAllStocks()
    {
        _logger.LogInformation("Syncing data for all stocks.");
        try
        {
            await _kiteStockSyncService.StockSyncAsync(HttpContext.RequestAborted);
            return Ok("Data sync done for all the stocks.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError("Error syncing stocks data: {Message}", ex.Message);
            return StatusCode(Constants.InternalServerError, "An error occurred while syncing stock data.");
        }
    }

    // [HttpGet("/sync-historical-data")]
    // public async Task<IActionResult> SyncHistoricalDataForAllStocks()
    // {
    //     _logger.LogInformation("Syncing historical data for all stocks.");
    //     try
    //     {
    //         await _zerodhaHelper.SyncHistoricalDataForAllStocks(HttpContext.RequestAborted);
    //         return Ok("Historical data sync done for all the stocks.");
    //     }
    //     catch (System.Exception ex)
    //     {
    //         _logger.LogError("Error syncing historical data: {Message}", ex.Message);
    //         return StatusCode(Constants.InternalServerError, "An error occurred while syncing historical stock data.");
    //     }
    // }
}