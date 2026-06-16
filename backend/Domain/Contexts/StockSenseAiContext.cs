using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.Contexts;
public class StockSenseAiContext: DbContext
{
    public StockSenseAiContext(DbContextOptions<StockSenseAiContext> options) : base(options)
    {
    }

    // Define DbSets for your entities here
    public DbSet<WatchListItem> WatchListItems { get; set; } = null!;
}