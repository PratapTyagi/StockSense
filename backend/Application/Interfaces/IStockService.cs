using Application.Interfaces;

namespace Application.Interfaces;
public interface IStockService
{
    Task<IStockDetailsResponse> GetStockDetailsAsync(string ticker);
    Task<IStockHistoryResponse> GetStockHistoryAsync(string ticker, string range);
    Task<ITrendingStocksResponse> GetTrendingStocksAsync();
}