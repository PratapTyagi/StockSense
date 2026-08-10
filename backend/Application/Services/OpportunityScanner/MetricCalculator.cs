using Application.Models.OpportunityScanner;
using Domain.Entities;

namespace Application.Services.OpportunityScanner;

/// <summary>
/// Pure, dependency-free calculator that turns an ordered list of daily candles
/// (oldest → newest) into a <see cref="StockMetrics"/> record.
///
/// The class is deliberately kept stateless so it can be trivially unit-tested
/// and reused across the batch pipeline without worrying about concurrency.
/// </summary>
public static class MetricCalculator
{
    // Approximate trading-day counts. We use trading days (not calendar days)
    // because the candle stream itself is aligned to trading days.
    private const int OneMonthDays = 21;
    private const int ThreeMonthDays = 63;
    private const int SixMonthDays = 126;
    private const int VolatilityWindow = 30;
    private const double TradingDaysPerYear = 252.0;

    /// <summary>
    /// Minimum number of candles required to compute the full metric set.
    /// The eligibility filter in the service should enforce this before calling.
    /// </summary>
    public const int MinCandlesRequired = 200;

    /// <summary>
    /// Compute all metrics for a single stock. <paramref name="candles"/> MUST
    /// be sorted ascending by <see cref="StockCandle.Timestamp"/>.
    /// </summary>
    public static StockMetrics Compute(Stock stock, IReadOnlyList<StockCandle> candles)
    {
        var last = candles[^1];
        var closes = new decimal[candles.Count];
        for (int i = 0; i < candles.Count; i++) closes[i] = candles[i].Close;

        var metrics = new StockMetrics
        {
            StockId = stock.Id,
            Symbol = stock.Symbol,
            CompanyName = stock.CompanyName,
            Exchange = stock.Exchange,
            LatestClose = last.Close,
            LatestTimestamp = last.Timestamp,
            CurrentVolume = last.Volume,

            OneMonthReturnPct = PercentReturn(closes, OneMonthDays),
            ThreeMonthReturnPct = PercentReturn(closes, ThreeMonthDays),
            SixMonthReturnPct = PercentReturn(closes, SixMonthDays),

            Sma20 = SimpleMovingAverage(closes, 20),
            Sma50 = SimpleMovingAverage(closes, 50),
            Sma200 = SimpleMovingAverage(closes, 200),

            AverageVolume20 = AverageVolume(candles, 20),
            AnnualizedVolatilityPct = AnnualizedVolatility(closes, VolatilityWindow),
        };

        if (metrics.AverageVolume20 is > 0)
        {
            metrics.VolumeRatio = metrics.CurrentVolume / metrics.AverageVolume20.Value;
        }

        return metrics;
    }

    /// <summary>
    /// Percentage return between the last close and the close <paramref name="lookbackDays"/> ago.
    /// Returns null when there is not enough history or the reference close is non-positive.
    /// </summary>
    private static double? PercentReturn(decimal[] closes, int lookbackDays)
    {
        if (closes.Length <= lookbackDays) return null;
        var current = closes[^1];
        var past = closes[^(lookbackDays + 1)];
        if (past <= 0) return null;
        return (double)((current / past) - 1m) * 100.0;
    }

    private static decimal? SimpleMovingAverage(decimal[] closes, int window)
    {
        if (closes.Length < window) return null;
        decimal sum = 0m;
        for (int i = closes.Length - window; i < closes.Length; i++) sum += closes[i];
        return sum / window;
    }

    private static double? AverageVolume(IReadOnlyList<StockCandle> candles, int window)
    {
        if (candles.Count < window) return null;
        long sum = 0;
        for (int i = candles.Count - window; i < candles.Count; i++) sum += candles[i].Volume;
        return sum / (double)window;
    }

    /// <summary>
    /// Annualized volatility (%) = stdev(daily returns over <paramref name="window"/> days) × √252 × 100.
    /// </summary>
    private static double? AnnualizedVolatility(decimal[] closes, int window)
    {
        // Need window+1 closes to produce `window` daily returns.
        if (closes.Length < window + 1) return null;

        var returns = new double[window];
        int start = closes.Length - window - 1;
        for (int i = 0; i < window; i++)
        {
            var prev = (double)closes[start + i];
            var curr = (double)closes[start + i + 1];
            if (prev <= 0) return null;
            returns[i] = (curr / prev) - 1.0;
        }

        double mean = 0.0;
        for (int i = 0; i < returns.Length; i++) mean += returns[i];
        mean /= returns.Length;

        double sqSum = 0.0;
        for (int i = 0; i < returns.Length; i++)
        {
            var d = returns[i] - mean;
            sqSum += d * d;
        }
        // Sample standard deviation (n-1). With window=30 the bias is negligible either way.
        var stdev = Math.Sqrt(sqSum / (returns.Length - 1));
        return stdev * Math.Sqrt(TradingDaysPerYear) * 100.0;
    }
}