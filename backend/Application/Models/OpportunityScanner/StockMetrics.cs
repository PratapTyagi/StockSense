namespace Application.Models.OpportunityScanner;

/// <summary>
/// Raw metrics computed from historical candles for a single stock.
/// This is the intermediate representation between "candles" and "scores".
/// </summary>
public class StockMetrics
{
    public long StockId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;

    public decimal LatestClose { get; set; }
    public DateTime LatestTimestamp { get; set; }

    // --- Momentum ---
    public double? OneMonthReturnPct { get; set; }
    public double? ThreeMonthReturnPct { get; set; }
    public double? SixMonthReturnPct { get; set; }

    // --- Trend ---
    public decimal? Sma20 { get; set; }
    public decimal? Sma50 { get; set; }
    public decimal? Sma200 { get; set; }

    /// <summary>Distance from SMA50 as percentage: (Price/SMA50 - 1) × 100</summary>
    public double? DistanceFromSma50Pct { get; set; }

    /// <summary>Distance from SMA200 as percentage: (Price/SMA200 - 1) × 100</summary>
    public double? DistanceFromSma200Pct { get; set; }

    /// <summary>SMA50 vs SMA200 distance: (SMA50/SMA200 - 1) × 100</summary>
    public double? Sma50VsSma200Pct { get; set; }

    // --- Volume ---
    public long CurrentVolume { get; set; }
    public double? AverageVolume20 { get; set; }
    public double? VolumeRatio { get; set; }

    /// <summary>Average daily traded value (price × volume) over 20 days. Used for liquidity filtering.</summary>
    public double? AverageDailyTradedValue { get; set; }

    // --- Risk ---
    /// <summary>Annualized volatility (%) computed from daily returns over the last 30 trading days.</summary>
    public double? AnnualizedVolatilityPct { get; set; }
}