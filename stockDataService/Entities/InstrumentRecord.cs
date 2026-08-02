namespace Entities;

public class InstrumentRecord
{
    public required string InstrumentToken { get; init; }

    public required string TradingSymbol { get; init; }

    public required string CompanyName { get; init; }

    public required string Exchange { get; init; }

    public required string Segment { get; init; }

    public required string InstrumentType { get; init; }

    public required string Expiry { get; init; }
}