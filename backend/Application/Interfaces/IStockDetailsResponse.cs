using Application.Models;

namespace Application.Interfaces;
public interface IStockDetailsResponse
{
    string companyName { get; set; }
    string symbol { get; set; }
    decimal nsePrice { get; set; }
    decimal bsePrice { get; set; }
    decimal changePercent { get; set; }
    decimal change { get; set; }
    DateTime lastUpdated { get; set; }
    List<News> news { get; set; }
}