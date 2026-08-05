using Application.Interfaces;
using Application.Services;
using Infrastructure.Cache;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddScoped<IStockService, RapidApiStockService>();
        services.AddKeyedSingleton<ICacheService, RedisCacheService>("redis");
        services.AddScoped<IWatchListService, WatchListService>();
        services.AddScoped<IStockSenseService, StockSenseService>();
    }
}