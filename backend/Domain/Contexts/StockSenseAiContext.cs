using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.Contexts;

public class StockSenseAiContext : DbContext
{
    public StockSenseAiContext(DbContextOptions<StockSenseAiContext> options) : base(options)
    {
    }

    // Define DbSets for your entities here
    public DbSet<WatchListItem> WatchListItems { get; set; } = null!;
    public DbSet<Stock> Stocks { get; set; } = null!;
    public DbSet<StockCandle> StockCandles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.ToTable("Stocks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Symbol).HasMaxLength(50).IsRequired();
            entity.Property(e => e.InstrumentToken).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Exchange).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CompanyName).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Symbol).IsUnique();
        });

        modelBuilder.Entity<StockCandle>(entity =>
        {
            entity.ToTable("StockCandles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Open).HasColumnType("decimal(18,4)");
            entity.Property(e => e.High).HasColumnType("decimal(18,4)");
            entity.Property(e => e.Low).HasColumnType("decimal(18,4)");
            entity.Property(e => e.Close).HasColumnType("decimal(18,4)");
            entity.Property(e => e.CreatedAt)
                  .ValueGeneratedOnAdd()
                  .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.StockId, e.Timestamp }).IsUnique();
            entity.HasOne(e => e.Stock)
                  .WithMany(s => s.Candles)
                  .HasForeignKey(e => e.StockId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}