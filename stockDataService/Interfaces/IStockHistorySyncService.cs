using StockDataService.Entities;

namespace Interfaces
{
    public interface IStockHistorySyncService
    {
        Task<List<StockCandleRecord>> GetStockHistoricalData(string stockSymbol, DateTime fromDate);
        Task SyncAllStocksHistoricalDataAsync();
    }
}