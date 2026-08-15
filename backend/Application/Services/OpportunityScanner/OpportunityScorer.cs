using Application.Models.OpportunityScanner;

namespace Application.Services.OpportunityScanner;

/// <summary>
/// Converts raw <see cref="StockMetrics"/> into 0-100 category scores and the
/// weighted aggregate OpportunityScore. Also generates explanatory signals.
///
/// Weights:
///   Momentum 35% • Trend 30% • Volume 10% • Risk 25%
///
/// Scoring uses continuous/normalized functions to reduce clustering.
/// All scoring is pure and stateless.
/// </summary>
public static class OpportunityScorer
{
    // --- Configurable weights ---
    public const double MomentumWeight = 0.35;
    public const double TrendWeight = 0.30;
    public const double VolumeWeight = 0.10;
    public const double RiskWeight = 0.25;

    // --- Configurable thresholds ---

    /// <summary>Distance from SMA200 (%) at which overextension penalty begins (20%).</summary>
    public const double OverextensionThresholdPct = 20.0;

    /// <summary>Momentum return cap (%) — returns above this don't increase score further.</summary>
    public const double MomentumCapPct = 50.0;

    /// <summary>Trend distance cap (%) — being more than this % above SMA doesn't increase score further.</summary>
    public const double TrendDistanceCapPct = 30.0;

    /// <summary>Volume ratio cap — ratios above this don't increase score further.</summary>
    public const double VolumeRatioCap = 3.0;

    /// <summary>Volatility (%) at which risk score becomes 0.</summary>
    public const double VolatilityMaxPct = 80.0;

    /// <summary>Volatility (%) at which risk score is 100 (perfect).</summary>
    public const double VolatilityMinPct = 10.0;

    public static OpportunityResult Score(StockMetrics m)
    {
        var signals = new List<string>();

        var momentumScore = ScoreMomentum(m, signals);
        var trendScore = ScoreTrend(m, signals);
        var volumeScore = ScoreVolume(m, signals);
        var riskScore = ScoreRisk(m, signals);

        // Base score from weighted components
        var baseScore =
              momentumScore * MomentumWeight
            + trendScore * TrendWeight
            + volumeScore * VolumeWeight
            + riskScore * RiskWeight;

        // Overextension penalty (bounded, applied after base score)
        var penalty = CalculateOverextensionPenalty(m.DistanceFromSma200Pct);

        // Signal for overextension
        if (penalty > 0)
            signals.Add("Potentially overextended");

        // Final score = base - penalty, clamped to [0, 100]
        var finalScore = Math.Clamp(baseScore - penalty, 0.0, 100.0);

        return new OpportunityResult
        {
            StockId = m.StockId,
            Symbol = m.Symbol,
            CompanyName = m.CompanyName,
            Exchange = m.Exchange,
            LatestClose = m.LatestClose,
            LatestTimestamp = m.LatestTimestamp,
            OpportunityScore = Math.Round(finalScore, 2),
            OverextensionPenalty = Math.Round(penalty, 2),
            Scores = new ScoreBreakdown
            {
                MomentumScore = Math.Round(momentumScore, 2),
                TrendScore = Math.Round(trendScore, 2),
                VolumeScore = Math.Round(volumeScore, 2),
                RiskScore = Math.Round(riskScore, 2),
            },
            Signals = signals,
            Metrics = m,
        };
    }

    // =========================================================================
    // MOMENTUM (35%) — Continuous normalized scoring
    // =========================================================================

    private static double ScoreMomentum(StockMetrics m, List<string> signals)
    {
        // Weight: 1M 30%, 3M 40%, 6M 30%
        var s1 = ScoreReturn(m.OneMonthReturnPct);
        var s3 = ScoreReturn(m.ThreeMonthReturnPct);
        var s6 = ScoreReturn(m.SixMonthReturnPct);

        var score = s1 * 0.30 + s3 * 0.40 + s6 * 0.30;

        // Generate signals
        if (m.OneMonthReturnPct is > 10) signals.Add("Strong 1M momentum");
        if (m.ThreeMonthReturnPct is > 15) signals.Add("Strong 3M momentum");
        if (m.SixMonthReturnPct is > 25) signals.Add("Strong 6M momentum");

        return score;
    }

    /// <summary>
    /// Continuous return scoring:
    /// - Negative returns: linearly scale from 0 (at -30% or worse) to 30 (at 0%)
    /// - 0% to cap: linearly scale from 30 to 100
    /// - Above cap: stays at 100
    /// This eliminates coarse buckets and produces unique scores.
    /// </summary>
    internal static double ScoreReturn(double? ret)
    {
        if (ret is null) return 0;
        var r = ret.Value;

        if (r <= -30.0) return 0;
        if (r < 0) return Lerp(0, 30, (r + 30.0) / 30.0);
        if (r >= MomentumCapPct) return 100;
        return Lerp(30, 100, r / MomentumCapPct);
    }

    // =========================================================================
    // TREND (30%) — Distance-based continuous scoring
    // =========================================================================

    private static double ScoreTrend(StockMetrics m, List<string> signals)
    {
        // Three components weighted:
        // - Price vs SMA50 distance: 35%
        // - Price vs SMA200 distance: 40%
        // - SMA50 vs SMA200 (golden/death cross proximity): 25%

        var priceVsSma50 = ScoreTrendDistance(m.DistanceFromSma50Pct);
        var priceVsSma200 = ScoreTrendDistance(m.DistanceFromSma200Pct);
        var sma50VsSma200 = ScoreTrendDistance(m.Sma50VsSma200Pct);

        var score = priceVsSma50 * 0.35 + priceVsSma200 * 0.40 + sma50VsSma200 * 0.25;

        // Signals
        if (m.DistanceFromSma50Pct is > 0) signals.Add("Price above 50-day SMA");
        if (m.DistanceFromSma200Pct is > 0) signals.Add("Price above 200-day SMA");
        if (m.Sma50VsSma200Pct is > 0) signals.Add("50-day SMA above 200-day SMA");

        return score;
    }

    /// <summary>
    /// Continuous trend distance scoring:
    /// - At -cap or below: 0
    /// - At 0 (exactly at SMA): 50
    /// - At +cap or above: 100
    /// Linear interpolation between these points.
    /// </summary>
    internal static double ScoreTrendDistance(double? distancePct)
    {
        if (distancePct is null) return 50; // neutral when unknown
        var d = distancePct.Value;

        // Clamp to [-cap, +cap]
        d = Math.Clamp(d, -TrendDistanceCapPct, TrendDistanceCapPct);

        // Map [-cap, +cap] → [0, 100]
        return Lerp(0, 100, (d + TrendDistanceCapPct) / (2.0 * TrendDistanceCapPct));
    }

    // =========================================================================
    // VOLUME (10%) — Bounded, with price-direction awareness
    // =========================================================================

    private static double ScoreVolume(StockMetrics m, List<string> signals)
    {
        if (m.VolumeRatio is null) return 50; // neutral when unknown

        var ratio = m.VolumeRatio.Value;

        // Base volume score: linear from 0 at ratio=0 to 70 at ratio=1.0, then
        // slower growth up to cap. This prevents extreme volume from dominating.
        double baseScore;
        if (ratio <= 0) baseScore = 0;
        else if (ratio <= 1.0) baseScore = Lerp(30, 65, ratio);
        else if (ratio <= VolumeRatioCap) baseScore = Lerp(65, 90, (ratio - 1.0) / (VolumeRatioCap - 1.0));
        else baseScore = 90; // capped — extreme volume doesn't keep increasing

        // Price-direction bonus/penalty: if we have 1M return data, use it
        // to determine if high volume is confirming a move or not.
        if (ratio > 1.2 && m.OneMonthReturnPct is not null)
        {
            if (m.OneMonthReturnPct.Value > 0)
            {
                // High volume + positive price = confirmation bonus (up to +10)
                baseScore = Math.Min(100, baseScore + 10);
            }
            else
            {
                // High volume + negative price = distribution, reduce score
                baseScore = Math.Max(0, baseScore - 15);
            }
        }

        if (ratio > 1.5) signals.Add("Above-average volume");

        return Math.Clamp(baseScore, 0, 100);
    }

    // =========================================================================
    // RISK (25%) — Continuous volatility-based scoring
    // =========================================================================

    private static double ScoreRisk(StockMetrics m, List<string> signals)
    {
        if (m.AnnualizedVolatilityPct is null) return 50; // neutral when unknown

        var vol = m.AnnualizedVolatilityPct.Value;

        // Linear: VolatilityMinPct → 100, VolatilityMaxPct → 0
        double score;
        if (vol <= VolatilityMinPct) score = 100;
        else if (vol >= VolatilityMaxPct) score = 0;
        else score = Lerp(100, 0, (vol - VolatilityMinPct) / (VolatilityMaxPct - VolatilityMinPct));

        if (vol > 40) signals.Add("High volatility");

        return Math.Clamp(score, 0, 100);
    }

    // =========================================================================
    // OVEREXTENSION PENALTY
    // =========================================================================

    /// <summary>
    /// Calculates a bounded penalty based on distance from SMA200.
    /// Penalty ranges from 0 to 15 (max). Does not destroy the score.
    ///
    /// Thresholds:
    ///   Distance &lt; 20%  → 0
    ///   20% - &lt;35%      → 2
    ///   35% - &lt;50%      → 5
    ///   50% - &lt;70%      → 8
    ///   70% - &lt;100%     → 12
    ///   ≥100%            → 15
    /// </summary>
    internal static double CalculateOverextensionPenalty(double? distanceFromSma200Pct)
    {
        if (distanceFromSma200Pct is null) return 0;
        var dist = distanceFromSma200Pct.Value;

        if (dist < 20) return 0;
        if (dist < 35) return 2;
        if (dist < 50) return 5;
        if (dist < 70) return 8;
        if (dist < 100) return 12;
        return 15; // max penalty, bounded
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    /// <summary>Linear interpolation from a to b by t ∈ [0,1].</summary>
    private static double Lerp(double a, double b, double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);
        return a + (b - a) * t;
    }
}