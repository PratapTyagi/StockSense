using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

/// <summary>
/// Controller for stock-level operations such as syncing the master list of stocks.
/// Requires a valid X-Api-Key header.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StockController(ILogger<StockController> logger, IStockSyncService stockSyncService) : ControllerBase
{
    private readonly ILogger<StockController> _logger = logger;
    private readonly IStockSyncService _stockSyncService = stockSyncService;

    /// <summary>
    /// Syncs the master list of stocks by fetching the Kite instruments CSV,
    /// filtering for NSE equities, and ensuring each stock exists in the Stocks table
    /// along with a matching StockSyncStatus row.
    /// </summary>
    /// <returns>An action result indicating the outcome of the sync operation.</returns>
    [HttpGet("sync-all")]
    public async Task<IActionResult> SyncStocks()
    {
        _logger.LogInformation("Received request to sync stocks.");
        try
        {
            await _stockSyncService.SyncStocksAsync(HttpContext.RequestAborted);
            return Ok("Stocks synced successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError("Error syncing stocks: {Message}", ex.Message);
            return StatusCode(Constants.InternalServerError, "An error occurred while syncing stocks.");
        }
    }
}