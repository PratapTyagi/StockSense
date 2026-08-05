using Helpers;
using Interfaces;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using StockDataService.Repositories;
using StockDataService.Services;

namespace Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScopedServices(this IServiceCollection services)
    {
        services.AddScoped<IStockHistorySyncService, ZerodhaStockHistorySyncService>();
        services.AddScoped<IStockCandlesRepository, StockCandlesRepository>();
        services.AddScoped<IStockSyncRepository, StockSyncRepository>();
        services.AddScoped<IStocksRepository, StocksRepository>();
        services.AddScoped<IStockSyncService, ZerodhaStockSyncService>();

        // Encrypted, Redis-backed enctoken store. Registered as singleton so the process-local
        // memoization inside the provider is shared across requests.
        services.AddSingleton<IEncTokenProvider, RedisEncTokenProvider>();

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

    /// <summary>
    /// Registers Redis-backed distributed cache and ASP.NET Core Data Protection.
    /// - Redis is used as the shared, TTL-aware store for the enctoken.
    /// - Data Protection encrypts the token before it is written to Redis and persists its
    ///   key ring in Redis as well so keys survive app restarts and can be shared across
    ///   multiple instances of the service.
    /// </summary>
    public static IServiceCollection AddSecureTokenStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException(
                "Connection string 'Redis' is required for secure enctoken storage.");

        string instanceName = configuration["Redis:InstanceName"] ?? "stockDataService:";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = instanceName;
        });

        // Share one multiplexer for both the cache and the Data Protection key ring.
        var multiplexer = ConnectionMultiplexer.Connect(redisConnection);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);

        services
            .AddDataProtection()
            .SetApplicationName("StockSenseAI.stockDataService")
            .PersistKeysToStackExchangeRedis(multiplexer, $"{instanceName}dp-keys");

        return services;
    }
}