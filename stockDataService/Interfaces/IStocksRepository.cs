using StockDataService.Entities;

public interface IStocksRepository
{
    Task<StockRecord?> GetBySymbolAsync(string symbol);
    Task<List<StockRecord>> GetAllAsync();
    Task InsertAsync(StockRecord stock);
    Task InsertManyAsync(List<StockRecord> stocks);
}