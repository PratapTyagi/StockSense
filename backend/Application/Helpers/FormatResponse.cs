using System.Text.Json;
using Application.Interfaces;
using Application.Models;

namespace Application.Helpers;
public static class FormatResponse
{
    
    public static IStockDetailsResponse FormatStockDetailsResponse(string jsonResponse, string ticker)
    {  
        var apiResponse = JsonSerializer.Deserialize<RapidAPIStockDetailsResponse>(jsonResponse);

        if(apiResponse == null)
        {
            throw new Exception("Failed to deserialize API response for stock details.");
        }

        var response = new StockDetailsResponse
        {
            companyName = apiResponse.companyName,
            symbol = ticker,
            nsePrice = apiResponse.currentPrice.NSE != null ? decimal.Parse(apiResponse.currentPrice.NSE) : -1,
            bsePrice = apiResponse.currentPrice.BSE != null ? decimal.Parse(apiResponse.currentPrice.BSE) : -1,
            lastUpdated = DateTime.UtcNow,
            changePercent = apiResponse.stockDetailsReusableData.percentChange != null ? decimal.Parse(apiResponse.stockDetailsReusableData.percentChange) : 0,
            change = decimal.Parse(apiResponse.stockDetailsReusableData.price) - decimal.Parse(apiResponse.stockDetailsReusableData.close),
            news = apiResponse.recentNews?.Select(newsItem => new News
            {
                headline = newsItem.headline,
                summary = newsItem.summary,
                source = "RapidAPI",
                url = newsItem.url,
                publishedAt = DateTime.Parse(newsItem.date)
            }).ToList() ?? new List<News>()
        };

        return response;
    }

    public static IStockHistoryResponse FormatStockHistoryResponse(string jsonResponse, string ticker, string range)
    {
        var apiResponse = JsonSerializer.Deserialize<RapidAPIStockHistoryResponse>(jsonResponse);
        StockHistoryResponse stockHistoryResponse = new StockHistoryResponse
        {
            symbol = ticker,
            range = range,
            data = apiResponse.datasets.FirstOrDefault()?.values.Select(value => new PricePoint
            {
                Date = value[0] != null ? DateTime.Parse(value[0].ToString()) : DateTime.MinValue,
                Close = decimal.Parse(value[1]?.ToString() ?? "0")
            }).ToList() ?? new List<PricePoint>()
        };
        return stockHistoryResponse;
    }

    public static ITrendingStocksResponse FormatTrendingStocksResponse(string jsonResponse)
    {
        var apiResponse = JsonSerializer.Deserialize<RapidAPITrendingStockResponse>(jsonResponse);
        var trendingStocksResponse = new TrendingStocksResponse
        {
            topGainers = apiResponse.trending_stocks.top_gainers.Select(stock => new TrendingStock
            {
                symbol = stock.ticker_id,
                price = decimal.Parse(stock.price),
                change = decimal.Parse(stock.net_change),
                changePercent = decimal.Parse(stock.percent_change)
            }).ToList(),
            topLosers = apiResponse.trending_stocks.top_losers.Select(stock => new TrendingStock
            {
                symbol = stock.ticker_id,
                price = decimal.Parse(stock.price),
                change = decimal.Parse(stock.net_change),
                changePercent = decimal.Parse(stock.percent_change)
            }).ToList()
        };
        return trendingStocksResponse;
    }

}