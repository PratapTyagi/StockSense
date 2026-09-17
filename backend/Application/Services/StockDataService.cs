using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces;
using System.Text.Json;
using System.Net.Http.Json;

namespace Backend.Services;

public class StockDataService(ILogger<StockDataService> logger, IHttpClientFactory httpClientFactory): IStockDataService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("StockDataServiceClient");
    private readonly ILogger<StockDataService> _logger = logger;

    /*
    * Adds enctoken to stockdata service
    */
    public async Task PushEncToken(string token)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/enctoken")
            {
                Content = JsonContent.Create(new { encToken = token })
            };
            var res = await _httpClient.SendAsync(httpRequest);
            _logger.LogInformation("Enc token saved successfully. {res}", res);
        }
        catch (System.Exception)
        {
            _logger.LogError("Error adding the token");
            throw;
        }
    }
}