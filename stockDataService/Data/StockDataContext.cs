using Microsoft.EntityFrameworkCore;
using StockDataService.Entities;

namespace StockDataService.Data;

public class StockDataContext : DbContext
{
    public StockDataContext(DbContextOptions<StockDataContext> options) : base(options)
    { }

    public DbSet<StockRecord> Stocks { get; set; } = null!;
    public DbSet<StockCandleRecord> StockHistory { get; set; } = null!;
    public DbSet<StockSyncStatusRecord> StockSyncStatus { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StockCandleRecord>(entity =>
        {
            entity.ToTable("StockCandles", "dbo");
        });

        modelBuilder.Entity<StockRecord>(entity =>
        {
            entity.ToTable("Stocks", "dbo");
        });

        modelBuilder.Entity<StockSyncStatusRecord>(entity =>
        {
            entity.ToTable("StockSyncStatus", "dbo");
        });
    }
}
