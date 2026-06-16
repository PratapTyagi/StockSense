using Helpers;
using Interfaces;

namespace Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScopedServices(this IServiceCollection services)
    {
        // Registers the helper with a scoped lifetime
        services.AddScoped<IZerodhaHelper, ZerodhaHelper>();
        services.AddSingleton<IKiteInstrumentLoader, KiteInstrumentLoader>();
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