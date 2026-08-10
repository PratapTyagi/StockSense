namespace Application.Models.OpportunityScanner;

/// <summary>
/// Query parameters accepted by the Opportunity Scanner endpoint.
/// Kept intentionally small — MVP surface only.
/// </summary>
public class OpportunityScannerRequest
{
    /// <summary>Number of top-ranked opportunities to return.</summary>
    public int Top { get; set; } = 20;

    /// <summary>Only include stocks whose OpportunityScore is at least this value (0-100).</summary>
    public double? MinScore { get; set; }

    /// <summary>Optional exchange filter (e.g. "NSE").</summary>
    public string? Exchange { get; set; }
}