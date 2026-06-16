namespace Application.Models.StockSenseService;
public class CompareStockResponse
{
    public required string Ticker { get; set; }
    public required StockDetailsResponse Details { get; set; }
}