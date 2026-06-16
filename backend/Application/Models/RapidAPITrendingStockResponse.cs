namespace Application.Interfaces;
public class RapidAPITrendingStockResponse
{
    public required RapidAPITrendingStocksData trending_stocks { get; set; }
}

public class RapidAPITrendingStocksData
{
    public required List<RapidAPITrendingStockItem> top_gainers { get; set; }
    public required List<RapidAPITrendingStockItem> top_losers { get; set; }
}

public class RapidAPITrendingStockItem
{
    public required string ticker_id { get; set; }
    public required string company_name { get; set; }
    public required string price { get; set; }
    public required string percent_change { get; set; }
    public required string net_change { get; set; }
    public required string ric { get; set; }
}