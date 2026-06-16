namespace Interfaces
{
    public interface IZerodhaHelper
    {
        Task GetStockHistoricalData(string stockSymbol);
    }
}