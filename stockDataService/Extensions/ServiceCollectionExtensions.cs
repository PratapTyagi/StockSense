using Helpers;
using Interfaces;
using StockDataService.Repositories;
using StockDataService.Services;

namespace Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScopedServices(this IServiceCollection services)
    {
        services.AddScoped<IStockHistorySyncService, StockHistorySyncService>();
        services.AddScoped<IStockCandlesRepository, StockCandlesRepository>();
        services.AddScoped<IStockSyncRepository, StockSyncRepository>();
        services.AddScoped<IStocksRepository, StocksRepository>();
        services.AddScoped<IStockSyncService, StockSyncService>();

        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        // Client 1: For Instruments/Tokens
        services.AddHttpClient(Constants.ZerodhaInstrumentsClient, client =>
        {
            client.BaseAddress = new Uri(Constants.KiteAPI);
        });

        // Client 2: For Historical Data
        services.AddHttpClient(Constants.ZerodhaHistoricalClient, client =>
        {
            client.BaseAddress = new Uri(Constants.ZerodhaKite);
        });

        return services;
    }
}