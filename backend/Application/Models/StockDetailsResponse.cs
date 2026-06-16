using Application.Interfaces;
using Application.Models;

public class StockDetailsResponse : IStockDetailsResponse
{
    public required string companyName { get; set; }
    public required string symbol { get; set; }
    public required decimal nsePrice { get; set; }
    public required decimal bsePrice { get; set; }
    public required decimal changePercent { get; set; }
    public required decimal change { get; set; }
    public required DateTime lastUpdated { get; set; }
    public List<News> news { get; set; } = null!;
}

public class News
{
    public required string headline { get; set; }
    public required string summary { get; set; }
    public required string source { get; set; }
    public required string url { get; set; }
    public required DateTime publishedAt { get; set; }
}