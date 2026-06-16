namespace Interfaces;

public interface ICandle
{
    DateTime Time { get; set; }
    decimal Open { get; set; }
    decimal High { get; set; }
    decimal Low { get; set; }
    decimal Close { get; set; }
    long Volume { get; set; }
}