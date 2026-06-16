using Application.Interfaces;
using Application.Models.WatchList;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;
/// <summary>
/// Controller for managing user watchlists, allowing users to add, remove, and view their watch list items.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class WatchlistController(IWatchListService watchListService) : ControllerBase
{

    /// <summary>
    /// Retrieves the watchlist for a specific user.
    /// </summary>
    /// <returns>The watchlist for the specified user.</returns>
    [HttpGet()]
    public async Task<IActionResult> GetWatchlist()
    {
        var watchlist = await watchListService.GetWatchlistAsync("userId");
        return Ok(watchlist);
    }

    /// <summary>
    /// Adds an item to the user's watchlist.
    /// </summary>
    /// <param name="item">The item to be added to the watchlist.</param>
    /// <returns>A response indicating the result of the add operation.</returns>
    [HttpPost()]
    public async Task<IActionResult> AddToWatchlist([FromBody] AddWatchlistDto item)
    {
        string symbol = item.Symbol.ToUpper();
        await watchListService.AddToWatchlistAsync("userId", symbol);
        return Ok();
    }

    /// <summary>
    /// Removes an item from the user's watchlist.
    /// </summary>
    /// <param name="symbol">The symbol of the item to be removed from the watchlist.</param>
    /// <returns>A response indicating the result of the remove operation.</returns>
    [HttpDelete()]
    public async Task<IActionResult> RemoveFromWatchlist([FromQuery] string symbol)
    {
        symbol = symbol.ToUpper();
        await watchListService.RemoveFromWatchlistAsync("userId", symbol);
        return Ok();
    }
}