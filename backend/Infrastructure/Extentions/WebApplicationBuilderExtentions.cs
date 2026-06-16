using Application.Constants;
using Domain.Contexts;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extentions;
internal static class WebApplicationBuilderExtentions
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
        builder.Services.AddApplicationDependencies(builder.Configuration);

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
        // Configuring MySQL connection
        string connectionString = builder.Configuration.GetConnectionString("MysqlDb") ?? throw new InvalidOperationException("Connection string 'MysqlDb' not found.");
        builder.Services.AddDbContext<StockSenseAiContext>(options =>
            options.UseMySQL(connectionString)
        );

        builder.Services.AddControllers();
        builder.Services.AddHttpClient();
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