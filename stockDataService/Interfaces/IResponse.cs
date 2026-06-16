using Newtonsoft.Json;

namespace Interfaces;

public record Response<T>
{
    [JsonProperty("status")]
    public string? Status { get; set; }
    [JsonProperty("data")]
    public T? Data { get; set; }
}