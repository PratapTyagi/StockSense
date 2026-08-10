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

    // --- Volume ---
    public long CurrentVolume { get; set; }
    public double? AverageVolume20 { get; set; }
    public double? VolumeRatio { get; set; }

    // --- Risk ---
    /// <summary>Annualized volatility (%) computed from daily log/simple returns over the last 30 trading days.</summary>
    public double? AnnualizedVolatilityPct { get; set; }
}