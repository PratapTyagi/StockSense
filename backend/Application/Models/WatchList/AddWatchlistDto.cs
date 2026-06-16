using System.Runtime.Serialization;

namespace Application.Models.WatchList;
/// <summary>
/// Data Transfer Object (DTO) for adding an item to the user's watchlist.
/// </summary>
[DataContract]
public class AddWatchlistDto
{
    /// <summary>
    /// The symbol of the item to be added to the watchlist.
    /// </summary>
    [DataMember(Name = "symbol")]
    public string Symbol { get; set; } = string.Empty;
}