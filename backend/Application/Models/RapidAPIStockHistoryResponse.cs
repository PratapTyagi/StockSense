using Application.Interfaces;

namespace Application.Models;
public class RapidAPIStockHistoryResponse: IRapidAPIStockHistoryResponse
{
    public required List<Dataset> datasets { get; set; }
}

public class Dataset: IDataset
{
    public required string metric { get; set; }
    public required string label { get; set; }

    // Each inner array can have different types:
    // For Price/DMA it is [string date, string value]
    // For Volume it is [string date, int volume, object meta]
    public required List<List<object>> values { get; set; }
}