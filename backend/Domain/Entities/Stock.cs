namespace Domain.Entities;

/// <summary>
/// Represents an equity instrument that we track in the local database.
/// Mirrors the <c>dbo.Stocks</c> table populated by the stockDataService.
/// </summary>
public partial class Stock
{
    public long Id { get; set; }
    public string Symbol { get; set; } = null!;
    public string InstrumentToken { get; set; } = null!;
    public string Exchange { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public bool IsActive { get; set; }

    public virtual ICollection<StockCandle> Candles { get; set; } = new List<StockCandle>();
}