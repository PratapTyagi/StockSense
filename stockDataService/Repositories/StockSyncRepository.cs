using Microsoft.EntityFrameworkCore;
using Interfaces;
using StockDataService.Data;
using StockDataService.Entities;

namespace StockDataService.Repositories;

public class StockSyncRepository : IStockSyncRepository
{
    private readonly StockDataContext _context;

    public StockSyncRepository(StockDataContext stockDataContext)
    {
        _context = stockDataContext;
    }

    public async Task<Dictionary<string, StockSyncStatusRecord>> GetAllStockSyncStatusAsync()
    {
        return await _context.StockSyncStatus
            .ToDictionaryAsync(r => r.Symbol);
    }

    public async Task<StockSyncStatusRecord?> GetStockSyncStatusBySymbolAsync(string symbol)
    {
        return await _context.StockSyncStatus.FirstOrDefaultAsync(r => r.Symbol == symbol);
    }

    public async Task InsertManyAsync(List<StockSyncStatusRecord> stockSyncStatuses)
    {
        await _context.StockSyncStatus.AddRangeAsync(stockSyncStatuses);
        await _context.SaveChangesAsync();
    }

    public async Task UpsertStockSyncStatusesAsync(List<StockSyncStatusRecord> stockSyncStatuses)
    {
        foreach (var status in stockSyncStatuses)
        {
            if (status.Id == 0)
            {
                // New record — attach as Added
                _context.StockSyncStatus.Add(status);
            }
            else if (_context.Entry(status).State == EntityState.Detached)
            {
                // Existing record but not tracked — mark as Modified
                _context.StockSyncStatus.Update(status);
            }
            // Else: already tracked, mutations are picked up automatically
        }
        await _context.SaveChangesAsync();
    }
}