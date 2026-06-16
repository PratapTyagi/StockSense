using Application.Models;

namespace Application.Interfaces;
public interface ITrendingStocksResponse
{
    List<TrendingStock> topGainers { get; }
    List<TrendingStock> topLosers { get; }
}

public interface ITrendingStock
{
    string symbol { get; }
    decimal price { get; }
    decimal change { get; }
    decimal changePercent { get; }
}
