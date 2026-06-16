using Application.Models;

namespace Application.Interfaces;
public interface IRapidAPIStockHistoryResponse
{
    List<Dataset> datasets { get; }
}

public interface IDataset
{
    string metric { get; }
    string label { get; }

    // List of rows, each row can contain different types depending on metric
    List<List<object>> values { get; }
}