namespace Interfaces;

/// <summary>
/// Provides safe storage and retrieval for the Zerodha enctoken.
/// The token is stored in Redis with a TTL matching its session lifetime (~24 hours)
/// and is encrypted at rest using ASP.NET Core Data Protection.
/// </summary>
public interface IEncTokenProvider
{
    /// <summary>
    /// Retrieves the currently cached enctoken.
    /// Returns null if not set or expired.
    /// </summary>
    Task<string?> GetAsync(CancellationToken ct = default);

    /// <summary>
    /// Stores a new enctoken. The value is encrypted before being persisted to Redis
    /// and will automatically expire at the next session cut-off (~06:00 IST).
    /// </summary>
    Task SetAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Removes the currently stored enctoken (e.g. on logout or forced re-login).
    /// </summary>
    Task InvalidateAsync(CancellationToken ct = default);
}