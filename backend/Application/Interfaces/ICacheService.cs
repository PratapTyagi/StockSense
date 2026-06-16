namespace Application.Interfaces;
public interface ICacheService
{
    /// <summary>
    /// Fetches cached data for a given cache key. If the data is not present in the cache, it returns null.
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    Task<string?> GetCachedDataAsync(string cacheKey);

    /// <summary>
    /// Fetches cached data for multiple cache keys. It returns an array of cached data corresponding to the provided cache keys. If a particular cache key does not have cached data, the corresponding entry in the returned array will be null.
    /// </summary>
    /// <param name="cacheKeys"></param>
    /// <returns></returns>
    Task<string?[]> GetCachedDataAsync(string[] cacheKeys);

    /// <summary>
    /// Stores data in the cache with a specified cache key. It also allows configuring cache expiration settings. If preventCacheExpiration is set to true, the cache entry will not expire. Otherwise, it will expire after the specified absoluteExpirationRelativeToNow duration (default is 1 hour).
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="data"></param>
    /// <param name="absoluteExpirationRelativeToNow"></param>
    /// <returns></returns>
    Task SetCachedDataAsync(string cacheKey, object data, TimeSpan? absoluteExpirationRelativeToNow = null);
}