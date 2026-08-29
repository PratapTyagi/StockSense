using System.Collections.Generic;
using Application.Interfaces;

namespace Application.Models;
public class TrendingStocksResponse : ITrendingStocksResponse
{
    public required List<TrendingStock> topGainers { get; set; }
    public required List<TrendingStock> topLosers { get; set; }

    List<TrendingStock> ITrendingStocksResponse.topGainers => topGainers;
    List<TrendingStock> ITrendingStocksResponse.topLosers => topLosers;
}

public class TrendingStock : ITrendingStock
{
    public required string symbol { get; set; }
    public string companyName { get; set; } = string.Empty;
    public required decimal price { get; set; }
    public required decimal change { get; set; }
    public required decimal changePercent { get; set; }
}
