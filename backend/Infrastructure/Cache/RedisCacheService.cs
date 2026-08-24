using System.Text.Json;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Cache;

public class RedisCacheService(IDistributedCache distributedCache) : ICacheService
{
    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDistributedCache _distributedCache = distributedCache;

    /// <summary>
    /// Fetches cached data for a given cache key. If the data is not present in the cache, it returns null.
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    public async Task<string?> GetCachedDataAsync(string cacheKey)
    {
        return await _distributedCache.GetStringAsync(cacheKey);
    }

    /// <summary>
    /// Fetches cached data and deserializes it to the specified type.
    /// Returns null if the key doesn't exist or deserialization fails.
    /// </summary>
    public async Task<T?> GetCachedDataAsync<T>(string cacheKey) where T : class
    {
        var raw = await _distributedCache.GetStringAsync(cacheKey);
        if (string.IsNullOrEmpty(raw))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(raw, DeserializeOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Fetches cached data for multiple cache keys. It returns an array of cached data corresponding to the provided cache keys. If a particular cache key does not have cached data, the corresponding entry in the returned array will be null.
    /// </summary>
    /// <param name="cacheKeys"></param>
    /// <returns></returns>
    public async Task<string?[]> GetCachedDataAsync(string[] cacheKeys)
    {
        var tasks = cacheKeys.Select(key => _distributedCache.GetStringAsync(key)).ToArray();
        return await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Stores data in the cache with a specified cache key. It also allows configuring cache expiration settings. If preventCacheExpiration is set to true, the cache entry will not expire. Otherwise, it will expire after the specified absoluteExpirationRelativeToNow duration (default is 1 hour).
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="data"></param>
    /// <param name="absoluteExpirationRelativeToNow"></param>
    /// <returns></returns>
    public async Task SetCachedDataAsync(string cacheKey, object data, TimeSpan? absoluteExpirationRelativeToNow = null)
    {
        var options = absoluteExpirationRelativeToNow.HasValue
        ? new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow.Value
        }
        : new DistributedCacheEntryOptions();
        await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(data), options);
    }
}