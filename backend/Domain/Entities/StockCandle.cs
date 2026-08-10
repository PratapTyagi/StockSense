namespace Domain.Entities;

/// <summary>
/// Daily OHLCV candle for a <see cref="Stock"/>.
/// Mirrors the <c>dbo.StockCandles</c> table populated by the stockDataService.
/// </summary>
public partial class StockCandle
{
    public long Id { get; set; }
    public long StockId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Stock? Stock { get; set; }
}