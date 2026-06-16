using System.Collections.Generic;
using Application.Models;

namespace Application.Interfaces;
public interface IStockHistoryResponse
{
    public string symbol { get; set; }
    public string range { get; set; }
    public List<PricePoint> data { get; set; }
}

