using Microsoft.EntityFrameworkCore;
using StockDataService.Data;

namespace Extensions;

public static class DatabaseConnectionExtensions
{
    public static IServiceCollection AddDatabaseConnection(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<StockDataContext>(options =>
            options.UseSqlServer(connectionString)
        );

        return services;
    }
}