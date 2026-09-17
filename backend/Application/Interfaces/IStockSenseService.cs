using Application.Models.StockSenseService;

namespace Application.Interfaces;

public interface IStockSenseService
{
    /// <summary>
    /// Compares multiple stocks using AI analysis.
    /// </summary>
    Task<IEnumerable<CompareStockResponse>> CompareStockAsync(string[] tickers);
}
