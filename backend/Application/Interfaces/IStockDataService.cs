using Application.Interfaces;

namespace Application.Interfaces;
public interface IStockDataService
{
    Task<IStockDetailsResponse> GetStockDetailsAsync(string ticker);
    Task<ITrendingStocksResponse> GetTrendingStocksAsync();
}