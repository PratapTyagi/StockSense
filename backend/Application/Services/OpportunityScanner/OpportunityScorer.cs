using Application.Models.OpportunityScanner;

namespace Application.Services.OpportunityScanner;

/// <summary>
/// Converts raw <see cref="StockMetrics"/> into 0-100 category scores and the
/// weighted aggregate OpportunityScore.
///
/// Weights (per spec):
///   Momentum 40% • Trend 30% • Volume 15% • Risk 15%
///
/// Bucket thresholds are intentionally conservative and easy to tune; they are
/// heuristics, not financial truths. All scoring is pure and stateless.
/// </summary>
public static class OpportunityScorer
{
    private const double MomentumWeight = 0.40;
    private const double TrendWeight = 0.30;
    private const double VolumeWeight = 0.15;
    private const double RiskWeight = 0.15;

    public static OpportunityResult Score(StockMetrics m)
    {
        var breakdown = new ScoreBreakdown
        {
            MomentumScore = ScoreMomentum(m),
            TrendScore = ScoreTrend(m),
            VolumeScore = ScoreVolume(m),
            RiskScore = ScoreRisk(m),
        };

        var opportunity =
              breakdown.MomentumScore * MomentumWeight
            + breakdown.TrendScore * TrendWeight
            + breakdown.VolumeScore * VolumeWeight
            + breakdown.RiskScore * RiskWeight;

        return new OpportunityResult
        {
            StockId = m.StockId,
            Symbol = m.Symbol,
            CompanyName = m.CompanyName,
            Exchange = m.Exchange,
            LatestClose = m.LatestClose,
            LatestTimestamp = m.LatestTimestamp,
            OpportunityScore = Math.Round(opportunity, 2),
            Scores = new ScoreBreakdown
            {
                MomentumScore = Math.Round(breakdown.MomentumScore, 2),
                TrendScore = Math.Round(breakdown.TrendScore, 2),
                VolumeScore = Math.Round(breakdown.VolumeScore, 2),
                RiskScore = Math.Round(breakdown.RiskScore, 2),
            },
            Metrics = m,
        };
    }

    // --- Momentum ------------------------------------------------------------

    private static double ScoreMomentum(StockMetrics m)
    {
        // 1M 30% • 3M 40% • 6M 30%. Missing values contribute 0 with weight preserved
        // so a stock without full history won't get an artificially high score.
        var s1 = ScoreReturnBucket(m.OneMonthReturnPct);
        var s3 = ScoreReturnBucket(m.ThreeMonthReturnPct);
        var s6 = ScoreReturnBucket(m.SixMonthReturnPct);
        return s1 * 0.30 + s3 * 0.40 + s6 * 0.30;
    }

    private static double ScoreReturnBucket(double? ret)
    {
        if (ret is null) return 0;
        var r = ret.Value;
        if (r < 0) return 0;
        if (r < 5) return 50;
        if (r < 10) return 70;
        if (r < 20) return 85;
        return 100;
    }

    // --- Trend ---------------------------------------------------------------

    private static double ScoreTrend(StockMetrics m)
    {
        double score = 0;
        var price = m.LatestClose;

        if (m.Sma20 is { } s20 && price > s20) score += 20;
        if (m.Sma50 is { } s50 && price > s50) score += 25;
        if (m.Sma200 is { } s200 && price > s200) score += 30;
        if (m.Sma50 is { } sma50 && m.Sma200 is { } sma200 && sma50 > sma200) score += 25;

        return score; // caps naturally at 100
    }

    // --- Volume --------------------------------------------------------------

    private static double ScoreVolume(StockMetrics m)
    {
        // MVP: score purely on the ratio. We deliberately do NOT couple this to
        // price direction yet — that refinement is called out in the spec as a
        // future enhancement.
        if (m.VolumeRatio is null) return 50; // neutral when unknown
        var r = m.VolumeRatio.Value;
        if (r < 0.5) return 30;
        if (r < 0.8) return 50;
        if (r < 1.2) return 65;
        if (r < 1.5) return 80;
        if (r < 2.0) return 90;
        return 100;
    }

    // --- Risk ----------------------------------------------------------------

    private static double ScoreRisk(StockMetrics m)
    {
        // Lower volatility → higher score. Missing → neutral.
        if (m.AnnualizedVolatilityPct is null) return 50;
        var v = m.AnnualizedVolatilityPct.Value;
        if (v < 15) return 100;
        if (v < 20) return 90;
        if (v < 30) return 75;
        if (v < 40) return 55;
        if (v < 60) return 30;
        return 10;
    }
}