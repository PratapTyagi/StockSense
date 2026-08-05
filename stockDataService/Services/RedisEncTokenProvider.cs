using Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;

namespace StockDataService.Services;

/// <summary>
/// Redis-backed implementation of <see cref="IEncTokenProvider"/>.
///
/// Design:
/// - Persists the enctoken in Redis under a single well-known key.
/// - Encrypts the value with ASP.NET Core Data Protection so it is never at rest in plaintext.
///   Even if Redis is compromised, the ciphertext is unusable without the Data Protection key ring.
/// - Sets an absolute expiration that matches Zerodha's daily session cut-off (~06:00 IST).
///   Redis takes care of eviction automatically; the app never serves a stale token.
/// - An in-memory cache of the plaintext value is kept for the lifetime of the process so hot paths
///   avoid Redis round-trips and repeated decryption. It is invalidated when SetAsync/InvalidateAsync
///   are called on the same instance.
/// </summary>
public sealed class RedisEncTokenProvider : IEncTokenProvider
{
    private const string RedisKey = "zerodha:enctoken";
    private const string ProtectorPurpose = "Zerodha.EncToken.v1";

    private readonly IDistributedCache _cache;
    private readonly IDataProtector _protector;
    private readonly ILogger<RedisEncTokenProvider> _logger;
    private readonly IConfiguration _configuration;

    // Simple process-local memoization to avoid decrypting on every request.
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static string? _cachedToken;
    private static DateTimeOffset _cachedUntilUtc = DateTimeOffset.MinValue;

    public RedisEncTokenProvider(
        IDistributedCache cache,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<RedisEncTokenProvider> logger,
        IConfiguration configuration)
    {
        _cache = cache;
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string?> GetAsync(CancellationToken ct = default)
    {
        // Fast path: in-memory value that is still valid.
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _cachedUntilUtc)
        {
            return _cachedToken;
        }

        await _lock.WaitAsync(ct);
        try
        {
            // Re-check after acquiring the lock.
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _cachedUntilUtc)
            {
                return _cachedToken;
            }

            string? encrypted = await _cache.GetStringAsync(RedisKey, ct);
            if (string.IsNullOrEmpty(encrypted))
            {
                _cachedToken = null;
                return null;
            }

            try
            {
                string plaintext = _protector.Unprotect(encrypted);
                _cachedToken = plaintext;
                _cachedUntilUtc = GetNextExpiryUtc();
                return plaintext;
            }
            catch (Exception ex)
            {
                // Ciphertext is unreadable (e.g. data-protection key rotated).
                // Treat as missing so the caller can trigger a fresh login.
                _logger.LogError(ex, "Failed to decrypt cached enctoken. Removing corrupted entry.");
                await _cache.RemoveAsync(RedisKey, ct);
                _cachedToken = null;
                return null;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or whitespace.", nameof(token));
        }

        string encrypted = _protector.Protect(token);
        DateTimeOffset expiry = GetNextExpiryUtc();

        await _cache.SetStringAsync(
            RedisKey,
            encrypted,
            new DistributedCacheEntryOptions { AbsoluteExpiration = expiry },
            ct);

        await _lock.WaitAsync(ct);
        try
        {
            _cachedToken = token;
            _cachedUntilUtc = expiry;
        }
        finally
        {
            _lock.Release();
        }

        _logger.LogInformation("Zerodha enctoken updated. Expires at {ExpiryUtc} UTC.", expiry);
    }

    public async Task InvalidateAsync(CancellationToken ct = default)
    {
        await _cache.RemoveAsync(RedisKey, ct);

        await _lock.WaitAsync(ct);
        try
        {
            _cachedToken = null;
            _cachedUntilUtc = DateTimeOffset.MinValue;
        }
        finally
        {
            _lock.Release();
        }

        _logger.LogWarning("Zerodha enctoken invalidated.");
    }

    /// <summary>
    /// Zerodha sessions terminate at ~06:00 IST daily. Compute the next such instant in UTC.
    /// IST is UTC+05:30.
    /// </summary>
    private DateTimeOffset GetNextExpiryUtc()
    {
        string? expiryMinutesRaw = _configuration["Redis:EncTokenExpiryInMinutes"];
        if (!double.TryParse(expiryMinutesRaw, out double expiryMinutes))
        {
            // Fallback to IST offset (UTC+05:30) if configuration is missing or invalid.
            expiryMinutes = 330;
        }

        TimeSpan istOffset = TimeSpan.FromMinutes(expiryMinutes);
        DateTimeOffset nowIst = DateTimeOffset.UtcNow.ToOffset(istOffset);

        DateTimeOffset todayCutoffIst = new(
            nowIst.Year, nowIst.Month, nowIst.Day,
            6, 0, 0, istOffset);

        DateTimeOffset nextCutoffIst = nowIst < todayCutoffIst
            ? todayCutoffIst
            : todayCutoffIst.AddDays(1);

        return nextCutoffIst.ToUniversalTime();
    }
}