using Application.Models.OpportunityScanner;

namespace Application.Interfaces;

/// <summary>
/// Ranks the active stock universe by an aggregate "OpportunityScore"
/// derived from Momentum (35%), Trend (30%), Volume (10%), and Risk (25%).
/// All inputs come from the local Stocks/StockCandles tables.
/// </summary>
public interface IOpportunityScannerService
{
    Task<IReadOnlyList<OpportunityResult>> ScanAsync(
        OpportunityScannerRequest request,
        CancellationToken cancellationToken = default);
}