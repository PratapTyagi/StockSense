using System.Runtime.Serialization;

namespace Application.Models.StockDataService;
/// <summary>
/// Data Transfer Object (DTO) for adding an item to the user's watchlist.
/// </summary>
[DataContract]
public class EncTokenDto
{
    /// <summary>
    /// The token that's used for syncing stock history data.
    /// </summary>
    [DataMember(Name = "token")]
    public string Token { get; set; } = string.Empty;
}