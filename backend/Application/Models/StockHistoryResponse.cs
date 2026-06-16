using Application.Interfaces;
using Application.Models;

namespace Application.Models;
public class StockHistoryResponse : IStockHistoryResponse
{
    public required string symbol { get; set; }
    public required string range { get; set; }
    public required List<PricePoint> data { get; set; }
}

public class PricePoint
{
    public required DateTime Date { get; set; }
    public required decimal Close { get; set; }
}