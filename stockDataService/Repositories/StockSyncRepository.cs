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
            .Include(r => r.Stock)
            .ToDictionaryAsync(r => r.Symbol);
    }

    public async Task<StockSyncStatusRecord?> GetStockSyncStatusBySymbolAsync(string symbol)
    {
        return await _context.StockSyncStatus
            .Include(r => r.Stock)
            .FirstOrDefaultAsync(r => r.Symbol == symbol);
    }

    public async Task InsertManyAsync(List<StockSyncStatusRecord> stockSyncStatuses)
    {
        await _context.StockSyncStatus.AddRangeAsync(stockSyncStatuses);
        await _context.SaveChangesAsync();
    }

    public async Task UpsertStockSyncStatusAsync(StockSyncStatusRecord stockSyncStatus)
    {
        var existing = await _context.StockSyncStatus
        .FirstOrDefaultAsync(r => r.Stock_Id == stockSyncStatus.Stock_Id);

        if (existing is null)
        {
            await _context.StockSyncStatus.AddAsync(stockSyncStatus);
            await _context.SaveChangesAsync();
            return;
        }

        // Merge only the mutable fields — never overwrite Id / Stock_Id
        existing.Symbol = stockSyncStatus.Symbol;
        existing.LastCandleTimestamp = stockSyncStatus.LastCandleTimestamp;
        if (stockSyncStatus.LastFundamentalSync.HasValue)
            existing.LastFundamentalSync = stockSyncStatus.LastFundamentalSync;

        await _context.SaveChangesAsync();
    }
}