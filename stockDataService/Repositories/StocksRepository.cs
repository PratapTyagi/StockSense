using Microsoft.EntityFrameworkCore;
using StockDataService.Data;
using StockDataService.Entities;

public class StocksRepository : IStocksRepository
{
    private readonly StockDataContext _dbContext;

    public StocksRepository(StockDataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockRecord?> GetBySymbolAsync(string symbol)
    {
        return await _dbContext.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
    }

    public async Task<List<StockRecord>> GetAllAsync()
    {
        return await _dbContext.Stocks.ToListAsync();
    }

    public async Task InsertAsync(StockRecord stock)
    {
        await _dbContext.Stocks.AddAsync(stock);
        await _dbContext.SaveChangesAsync();
    }

    public async Task InsertManyAsync(List<StockRecord> stocks)
    {
        await _dbContext.Stocks.AddRangeAsync(stocks);
        await _dbContext.SaveChangesAsync();
    }
}