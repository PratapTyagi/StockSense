using System.Net.Http;
using Application.Interfaces;
using Domain.Contexts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;
public class WatchListService(StockSenseAiContext db, ILogger<WatchListService> logger) : IWatchListService
{
    StockSenseAiContext _db = db;
    ILogger<WatchListService> _logger = logger;

    public async Task<List<string>> GetWatchlistAsync(string userId)
    {
        try
        {
            List<string> watchListItems = await _db.WatchListItems.Select(w => w.Symbol).ToListAsync();
            return watchListItems;
        }
        catch (System.Exception)
        {
            _logger.LogError("Error retrieving watchlist for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task AddToWatchlistAsync(string userId, string symbol)
    {
        try
        {
            var existingItem = await _db.WatchListItems
                .FirstOrDefaultAsync(x => x.Symbol == symbol);
            if (existingItem != null) return;

            var watchListItem = new WatchListItem { Symbol = symbol };
            _db.WatchListItems.Add(watchListItem);
            await _db.SaveChangesAsync();
        }
        catch (System.Exception)
        {
            _logger.LogError("Error adding item {Symbol} to watchlist for user {UserId}", symbol, userId);
        }
    }

    public async Task RemoveFromWatchlistAsync(string userId, string symbol)
    {
        try
        {
            var item = await _db.WatchListItems
                .FirstOrDefaultAsync(x => x.Symbol == symbol);

            if (item == null) return;

            _db.WatchListItems.Remove(item);
            await _db.SaveChangesAsync();
        }
        catch (System.Exception)
        {
            _logger.LogError("Error removing item {Symbol} from watchlist for user {UserId}", symbol, userId);
        }
    }
}