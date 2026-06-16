
using System.Net.Http.Headers;
using Interfaces;
using Newtonsoft.Json;

namespace Helpers;

public class ZerodhaHelper(IHttpClientFactory _httpClientFactory, ILogger<ZerodhaHelper> _logger, IKiteInstrumentLoader _kiteInstrumentLoader) : IZerodhaHelper
{
    /// <summary>
    /// Fetches stock historical data for a given stock symbol using the Kite API. It first retrieves the instrument token for the specified stock symbol using the IKiteInstrumentLoader service, then makes an authenticated request to the Kite API's historical data endpoint to fetch the data. The response is logged for debugging purposes. If any errors occur during the request, they are caught and logged as well.
    /// </summary>
    /// <param name="stockSymbol"></param>
    /// <returns></returns>
    public async Task GetStockHistoricalData(string stockSymbol)
    {
        var _httpClient = _httpClientFactory.CreateClient(Constants.ZerodhaHistoricalClient);
        try
        {
            int token = _kiteInstrumentLoader.GetTokenCorrespondingToStockSymbol(stockSymbol);
            _logger.LogInformation("Token for {stockSymbol}: {token}", stockSymbol, token);
            // Fetch token from GetTokenCorrespondingToStockSymbol method somehow on load
            _httpClient.DefaultRequestHeaders.Add("Authorization", "enctoken jx1To07x4YF6wl9yfXiRNzN+B9XnCaPbEcZ0ZZPO5z9XTtHLFQHJJsBp9k/u9kSggt3/vDOQvODdxzhYf2sDDv0DWFJ0edbb/ZzGpetWYwXoNmXd2/3QZQ==");
            HttpResponseMessage response = await _httpClient.GetAsync($"{Constants.HistoricalEndpoint}/{token}/day?user_id=TL0092&oi=1&from=2024-05-24&to={DateTime.Today.ToString("yyyy-MM-dd")}");
            // Read the content as a string
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            Response<ZerodhaStockHistoryDataDTO> responseData = JsonConvert.DeserializeObject<Response<ZerodhaStockHistoryDataDTO>>(responseBody);

            _logger.LogInformation("Historical data response: {responseBody}", JsonConvert.SerializeObject(responseData));
        }
        catch (System.Exception e)
        {
            _logger.LogError("Request error: {Message}", e.Message);
            throw;
        }
    }
}