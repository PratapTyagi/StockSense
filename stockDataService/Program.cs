using Extensions;
using Microsoft.EntityFrameworkCore;
using StockDataService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Register SQL Server DbContext
string connectionString = builder.Configuration.GetConnectionString("StockDataDb")
    ?? throw new InvalidOperationException("Connection string 'StockDataDb' not found.");

// Register custom scoped services and HTTP clients
builder.Services.AddDatabaseConnection(connectionString);
builder.Services.AddSecureTokenStore(builder.Configuration);
builder.Services.AddScopedServices();
builder.Services.AddHttpClients();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();