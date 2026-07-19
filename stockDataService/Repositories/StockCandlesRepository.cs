using Microsoft.EntityFrameworkCore;
using Interfaces;
using StockDataService.Data;
using StockDataService.Entities;

namespace StockDataService.Repositories;

public class StockCandlesRepository : IStockCandlesRepository
{
    private readonly StockDataContext _context;

    public StockCandlesRepository(StockDataContext stockDataContext)
    {
        _context = stockDataContext;
    }

    public async Task<List<StockCandleRecord>> GetByStockIdAsync(long stockId)
    {
        return await _context.StockHistory
            .Where(r => r.StockId == stockId)
            .ToListAsync();
    }

    public async Task<List<StockCandleRecord>> GetByStockIdAndFromDateAsync(long stockId, DateTime fromDate)
    {
        return await _context.StockHistory
            .Where(r => r.StockId == stockId && r.Timestamp >= fromDate)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
    }

    public async Task InsertManyAsync(IEnumerable<StockCandleRecord> records)
    {
        await _context.StockHistory.AddRangeAsync(records);
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<long, List<StockCandleRecord>>> GetByStockIdsAsync(IEnumerable<long> stockIds)
    {
        var idSet = stockIds.ToHashSet();
        var records = await _context.StockHistory
            .Where(r => idSet.Contains(r.StockId))
            .OrderBy(r => r.StockId)
            .ThenByDescending(r => r.Timestamp)
            .ToListAsync();

        return records
            .GroupBy(r => r.StockId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}