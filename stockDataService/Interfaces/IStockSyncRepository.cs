using StockDataService.Entities;

namespace Interfaces;

public interface IStockSyncRepository
{
    Task<Dictionary<string, StockSyncStatusRecord>> GetAllStockSyncStatusAsync();
    Task<StockSyncStatusRecord?> GetStockSyncStatusBySymbolAsync(string symbol);
    Task InsertManyAsync(List<StockSyncStatusRecord> stockSyncStatuses);
    Task UpsertStockSyncStatusAsync(StockSyncStatusRecord stockSyncStatus);
}