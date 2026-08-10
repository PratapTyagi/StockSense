namespace Application.Models.OpportunityScanner;

/// <summary>
/// Per-category (0-100) score breakdown for a stock.
/// </summary>
public class ScoreBreakdown
{
    public double MomentumScore { get; set; }
    public double TrendScore { get; set; }
    public double VolumeScore { get; set; }
    public double RiskScore { get; set; }
}

/// <summary>
/// Final output row returned by the Opportunity Scanner.
/// One row per eligible stock, sorted by <see cref="OpportunityScore"/> descending.
/// </summary>
public class OpportunityResult
{
    public long StockId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;

    public decimal LatestClose { get; set; }
    public DateTime LatestTimestamp { get; set; }

    /// <summary>Aggregate 0-100 score. Higher = more attractive opportunity.</summary>
    public double OpportunityScore { get; set; }

    public ScoreBreakdown Scores { get; set; } = new();
    public StockMetrics Metrics { get; set; } = new();
}