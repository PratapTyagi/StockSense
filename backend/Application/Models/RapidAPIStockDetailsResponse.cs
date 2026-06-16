using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace Application.Models;
public class RapidAPIStockDetailsResponse
{
    public required string companyName { get; set; }
    public required string percentChange { get; set; }
    public required CurrentPrice currentPrice { get; set; }
    public required StockDetailsReusableData stockDetailsReusableData { get; set; }
    public List<NewsResponse> recentNews { get; set; } = null!;
}

public class CurrentPrice
{
    public string BSE { get; set; } = null!;
    public string NSE { get; set; } = null!;
}

public class NewsResponse
{
    public required string headline { get; set; }
    public required string summary { get; set; }
    public required string url { get; set; }
    public required string date { get; set; }
}

public class StockDetailsReusableData
{
    public string? close { get; set; }
    public string? price { get; set; }
    public string? percentChange { get; set; }
    public string? marketCap { get; set; }
}