using Application.Models.OpportunityScanner;

namespace Application.Interfaces;

/// <summary>
/// Ranks the active stock universe by an aggregate "OpportunityScore"
/// derived from Momentum (40%), Trend (30%), Volume (15%), and Risk (15%).
/// All inputs come from the local Stocks/StockCandles tables.
/// </summary>
public interface IOpportunityScannerService
{
    Task<IReadOnlyList<OpportunityResult>> ScanAsync(
        OpportunityScannerRequest request,
        CancellationToken cancellationToken = default);
}