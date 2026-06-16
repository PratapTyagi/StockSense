namespace Application.Interfaces;
public interface IWatchListService
{
    Task<List<string>> GetWatchlistAsync(string userId);
    Task AddToWatchlistAsync(string userId, string item);
    Task RemoveFromWatchlistAsync(string userId, string item);
}