namespace Interfaces;

public interface IStockSyncService
{
    Task SyncStocksAsync(CancellationToken cancellationToken);
}