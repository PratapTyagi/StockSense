using System.Text.Json;
using Application.Constants;
using Application.Helpers;
using Application.Interfaces;
using Application.Models.StockSenseService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mysqlx;

namespace Application.Services;

public class StockSenseService([FromKeyedServices("redis")] ICacheService cacheService, IStockService stockService, ILogger<StockSenseService> logger) : IStockSenseService
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly IStockService _stockService = stockService;
    private readonly ILogger<StockSenseService> _logger = logger;

    /// <summary>
    /// Compares stock data for multiple tickers. This method is intended to fetch and compare stock details for the provided tickers. The implementation can include fetching stock details for each ticker, comparing them based on certain criteria (e.g., price, market cap, etc.), and returning a comparison result. The actual comparison logic and return type can be defined based on specific requirements.
    /// </summary>
    /// <param name="tickers"></param>
    /// <returns></returns>
    public async Task<IEnumerable<CompareStockResponse>> CompareStockAsync(string[] tickers)
    {
        try
        {
            string?[] cachedValues = await _cacheService.GetCachedDataAsync(tickers);
            Dictionary<string, string> tickerToCacheMap = new Dictionary<string, string>();
            for (int i = 0; i < tickers.Length; i++)
            {
                if (cachedValues[i] != null)
                    tickerToCacheMap[tickers[i]] = cachedValues[i];
            }

            if (tickerToCacheMap.Count == tickers.Length)
            {
                return tickerToCacheMap.Select(kvp => new CompareStockResponse
                {
                    Ticker = kvp.Key,
                    Details = JsonSerializer.Deserialize<StockDetailsResponse>(kvp.Value)
                });
            }

            var tasks = new List<Task<IStockDetailsResponse>>();
            foreach (var ticker in tickers)
            {
                if (!tickerToCacheMap.ContainsKey(ticker))
                {
                    tasks.Add(_stockService.GetStockDetailsAsync(ticker));
                }
            }

            var stockDetailsResponses = await Task.WhenAll(tasks);
            for (int i = 0; i < stockDetailsResponses.Length; i++)
            {
                var stockDetails = stockDetailsResponses[i];
                var ticker = stockDetails.symbol;
                await _cacheService.SetCachedDataAsync($"{ServiceConstants.StockDetailsCacheKeyPrefix}{ticker}", stockDetails);
                tickerToCacheMap[ticker] = JsonSerializer.Serialize(stockDetails);
            }

            return tickerToCacheMap.Select(kvp => new CompareStockResponse
            {
                Ticker = kvp.Key,
                Details = JsonSerializer.Deserialize<StockDetailsResponse>(kvp.Value)
            });
        }
        catch (System.Exception)
        {
            _logger.LogError("An error occurred while comparing stocks for tickers: {tickers}", string.Join(", ", tickers));
            throw;
        }
    }
}