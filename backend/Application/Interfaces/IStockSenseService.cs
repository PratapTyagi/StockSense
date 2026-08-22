using Application.Models.StockSenseService;

namespace Application.Interfaces;

public interface IStockSenseService
{
    /// <summary>
    /// Sends a prompt to the AI model and returns the raw text response.
    /// </summary>
    Task<string> GetAIResponseAsync(string prompt);

    /// <summary>
    /// Compares multiple stocks using AI analysis.
    /// </summary>
    Task<IEnumerable<CompareStockResponse>> CompareStockAsync(string[] tickers);
}
