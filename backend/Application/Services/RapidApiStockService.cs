using System.Net.Http;
using System.Text.Json;
using Application.Constants;
using Application.Interfaces;
using Application.Models;
using Application.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services;
public class RapidApiStockService(IHttpClientFactory httpClientFactory, [FromKeyedServices("redis")] ICacheService cacheService, ILogger<RapidApiStockService> logger) : IStockService
{

    private readonly ICacheService _cacheService = cacheService;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("RapidApiClient");
    private readonly ILogger<RapidApiStockService> _logger = logger;

    /// <summary>
    /// Fetches stock details for a given stock ticker. It first checks if the details are present in the cache. If present, it returns the cached data. If not, it makes an API call to fetch the data, stores it in the cache for future requests, and then returns the response.
    /// </summary>
    /// <param name="ticker"></param>
    /// <returns type="IStockDetailsResponse">Stock details for the specified ticker. </returns>
    public async Task<IStockDetailsResponse> GetStockDetailsAsync(string ticker)
    {
        try
        {
            string stockDetailsCacheKey = $"{ServiceConstants.StockDetailsCacheKeyPrefix}{ticker}";
            string? cachedStockDetails = await _cacheService.GetCachedDataAsync(stockDetailsCacheKey);
            
            if (!string.IsNullOrEmpty(cachedStockDetails))
            {
                return JsonSerializer.Deserialize<StockDetailsResponse>(cachedStockDetails);
            }

            HttpRequestMessage request = InitializeHttpRequestAsync();
            request.RequestUri = new Uri($"{ServiceConstants.StockDetailsEndpoint}{ticker}", UriKind.Relative);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var stockResponse = FormatResponse.FormatStockDetailsResponse(json, ticker);

            await _cacheService.SetCachedDataAsync(stockDetailsCacheKey, stockResponse);
            return stockResponse;
        }
        catch (System.Exception e)
        {
            _logger.LogError($"Error occurred while fetching stock data for ticker: {JsonSerializer.Serialize(e)}");
            throw;
        }
    }

    /// <summary>
    /// Fetches historical stock data for a given ticker symbol and range.
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="range"></param>
    /// <returns type="IStockHistoryResponse">Historical stock data for the specified ticker and range.</returns>
    public async Task<IStockHistoryResponse> GetStockHistoryAsync(string ticker, string range)
    {
        try
        {
            string cachedStockHistoryCacheKey = $"{ServiceConstants.StockHistoryCacheKeyPrefix}{ticker}_{range}";
            string? cachedStockHistory = await _cacheService.GetCachedDataAsync(cachedStockHistoryCacheKey);

            if (!string.IsNullOrEmpty(cachedStockHistory))            {
                return JsonSerializer.Deserialize<StockHistoryResponse>(cachedStockHistory);
            }
            HttpRequestMessage request = InitializeHttpRequestAsync();
            request.RequestUri = new Uri($"{ServiceConstants.StockHistoryEndpoint}{ticker}&period={range}&filter=price", UriKind.Relative);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var stockHistoryResponse = FormatResponse.FormatStockHistoryResponse(json, ticker, range);
            
            await _cacheService.SetCachedDataAsync(cachedStockHistoryCacheKey, stockHistoryResponse);
            return stockHistoryResponse;
        }
        catch (System.Exception e)
        {
            _logger.LogError($"Error occurred while fetching stock history data for ticker: {JsonSerializer.Serialize(e)}");
            throw;
        }   
    }
    
    /// <summary>
    /// Fetches trending stocks.
    /// </summary>
    /// <returns type="ITrendingStocksResponse">Trending stocks.</returns>
    public async Task<ITrendingStocksResponse> GetTrendingStocksAsync()
    {
        try
        {
            string cachedTrendingStocksCacheKey = $"{ServiceConstants.TrendingStocksCacheKeyPrefix}{DateTime.UtcNow:yyyyMMdd}";
            string? cachedTrendingStocks = await _cacheService.GetCachedDataAsync(cachedTrendingStocksCacheKey);

            if (!string.IsNullOrEmpty(cachedTrendingStocks))            {
                return JsonSerializer.Deserialize<TrendingStocksResponse>(cachedTrendingStocks);
            }

            HttpRequestMessage request = InitializeHttpRequestAsync();
            request.RequestUri = new Uri("/trending", UriKind.Relative);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var trendingStocksResponse = FormatResponse.FormatTrendingStocksResponse(json);

            await _cacheService.SetCachedDataAsync(cachedTrendingStocksCacheKey, trendingStocksResponse);
            return trendingStocksResponse;
        } catch (System.Exception e)
        {
            _logger.LogError($"Error occurred while fetching trending stocks data: {JsonSerializer.Serialize(e)}");
            throw;
        }
    }

    private HttpRequestMessage InitializeHttpRequestAsync()
    {        
        var httpRequest = new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
        };
        return httpRequest;
    }
}