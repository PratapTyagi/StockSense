using StockDataService.Entities;

namespace Interfaces;

public interface IStockCandlesRepository
{
    Task<List<StockCandleRecord>> GetByStockIdAsync(long stockId);
    Task<List<StockCandleRecord>> GetByStockIdAndFromDateAsync(long stockId, DateTime fromDate);
    Task InsertManyAsync(IEnumerable<StockCandleRecord> records);
    Task<Dictionary<long, List<StockCandleRecord>>> GetByStockIdsAsync(IEnumerable<long> stockIds);
}