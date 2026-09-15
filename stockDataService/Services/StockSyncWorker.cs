using Interfaces;

public class StockSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEncTokenProvider _encTokenProvider;
    private readonly ILogger<StockSyncWorker> _logger;

    public StockSyncWorker(
        IServiceScopeFactory scopeFactory,
        IEncTokenProvider encTokenProvider,
        ILogger<StockSyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _encTokenProvider = encTokenProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var stockSyncService =
                scope.ServiceProvider.GetRequiredService<IStockSyncService>();

            var historySyncService =
                scope.ServiceProvider.GetRequiredService<IStockHistorySyncService>();
            
            // 1. Sync stocks
            try
            {
                await stockSyncService.SyncStocksAsync(cancellationToken);

                _logger.LogInformation(
                    "Stock synchronization completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Stock synchronization failed.");
            }

            // 2. Sync history
            try
            {
                await historySyncService.SyncAllStocksHistoricalDataAsync();

                _logger.LogInformation(
                    "Stock history synchronization completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Stock history synchronization failed.");
            }

            
            await _encTokenProvider.InvalidateAsync(cancellationToken);
            await Task.Delay(
                TimeSpan.FromDays(7),
                cancellationToken);
        }
    }
}