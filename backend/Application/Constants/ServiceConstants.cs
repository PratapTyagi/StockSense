namespace Application.Constants;
public static class ServiceConstants
{
    #region Domain names
    public const string StockApiBaseUrl = "https://indian-stock-exchange-api2.p.rapidapi.com";
    #endregion

    #region API endpoints
    public const string StockDetailsEndpoint = "/stock?name=";
    public const string StockHistoryEndpoint = "/historical_data?stock_name=";
    #endregion

    #region Cache keys
    public const string StockDetailsCacheKeyPrefix = "stock_details_";
    public const string StockHistoryCacheKeyPrefix = "stock_history_";
    public const string TrendingStocksCacheKeyPrefix = "trending_stocks_";
    #endregion
}