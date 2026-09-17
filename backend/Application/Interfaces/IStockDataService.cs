using Application.Interfaces;

namespace Application.Interfaces;
public interface IStockDataService
{
    Task PushEncToken(string token);
}