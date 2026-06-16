using Newtonsoft.Json;

namespace Interfaces;

public record ZerodhaStockHistoryDataDTO
{
    [JsonProperty("candles")]
    public List<List<object>> CandlesRaw { get; set; } = new();

    // A clean computed list that converts the raw object array into structured objects
    [JsonIgnore]
    public List<Candle> Candles => CandlesRaw.ConvertAll(c => new Candle(
        Time: DateTime.Parse(c[0].ToString()!),
        Open: Convert.ToDecimal(c[1]),
        High: Convert.ToDecimal(c[2]),
        Low: Convert.ToDecimal(c[3]),
        Close: Convert.ToDecimal(c[4]),
        Volume: Convert.ToInt64(c[5]),
        OpenInterest: Convert.ToInt64(c[6])
    ));
}

public record Candle(
    DateTime Time,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume,
    long OpenInterest
);
