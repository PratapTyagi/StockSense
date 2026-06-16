using Extensions;
using Microsoft.EntityFrameworkCore;
using StockDataService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Register MySQL DbContext
string connectionString = builder.Configuration.GetConnectionString("MysqlDb")
    ?? throw new InvalidOperationException("Connection string 'MysqlDb' not found.");
builder.Services.AddDbContext<StockDataContext>(options =>
    options.UseMySQL(connectionString)
);

// Register custom scoped services and HTTP clients
builder.Services.AddScopedServices();
builder.Services.AddHttpClients();

var app = builder.Build();

// Apply pending migrations on startup (creates DB and tables automatically)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StockDataContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Applying database migrations...");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();