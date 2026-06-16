using Microsoft.EntityFrameworkCore;
using StockDataService.Entities;

namespace StockDataService.Data;

public class StockDataContext : DbContext
{
    public StockDataContext(DbContextOptions<StockDataContext> options) : base(options)
    {
    }

    public DbSet<StockHistoryRecord> StockHistory { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StockHistoryRecord>(entity =>
        {
            // Index for efficient querying by symbol + timestamp
            entity.HasIndex(e => new { e.Symbol, e.Timestamp })
                  .HasDatabaseName("IX_StockHistory_Symbol_Timestamp");
        });
    }
}