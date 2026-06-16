using Application.Models.StockSenseService;

namespace Application.Interfaces;
public interface IStockSenseService
{
    public Task<IEnumerable<CompareStockResponse>> CompareStockAsync(string[] tickers);
}