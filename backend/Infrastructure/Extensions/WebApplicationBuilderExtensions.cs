using Application.Constants;
using Domain.Contexts;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

internal static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddWebApplicationBuilderConfiguration(this WebApplicationBuilder builder)
    {
        #region Cors configuration
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });
        #endregion

        #region Application dependencies configuration
        builder.Services.AddApplicationDependencies();

        // Add services to the container.
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"))
            .AddConsole()
            .AddDebug()
            .SetMinimumLevel(LogLevel.Information);

        // Configuring Redis cache
        builder.Services.AddStackExchangeRedisCache(redisOptions =>
        {
            redisOptions.Configuration = builder.Configuration.GetConnectionString("Redis");
        });
        // Configuring SQL Server connection
        // deployment the tables live at <ConnectedDatabase>.Stocks etc.
        string connectionString = builder.Configuration.GetConnectionString("StockDataDb") ?? throw new InvalidOperationException("Connection string 'StockDataDb' not found.");
        builder.Services.AddDbContext<StockSenseAiContext>(options =>
            options.UseSqlServer(connectionString)
        );

        builder.Services.AddControllers();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient("StockDataServiceClient", client =>
        {
            client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("STOCK_DATA_SERVICE_URL"));
            client.DefaultRequestHeaders.Add("X-Api-Key", Environment.GetEnvironmentVariable("STOCK_DATA_SERVICE_API_KEY"));
        });
        builder.Services.AddHttpClient("RapidApiClient", client =>
        {
            client.BaseAddress = new Uri(ServiceConstants.StockApiBaseUrl);
            client.DefaultRequestHeaders.Add("x-rapidapi-key", "6df753a340msh70ecef6f43a89eap109b89jsn69dcb697c221");
            client.DefaultRequestHeaders.Add("x-rapidapi-host", "indian-stock-exchange-api2.p.rapidapi.com");
        });
        #endregion

        #region Swagger configuration
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        return builder;
        #endregion
    }
}