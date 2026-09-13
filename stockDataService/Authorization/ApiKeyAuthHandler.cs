using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace StockDataService.Authorization;

/// <summary>
/// Options for API key authentication.
/// Configured via appsettings / environment variables:
///   { "ApiKey": "your-secret-key" }
///   or  ApiKey=your-secret-key
/// </summary>
public class ApiKeyAuthOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// The single valid API key. Any request with a matching X-Api-Key header is authenticated.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}

/// <summary>
/// Custom authentication handler that validates requests using an API key
/// passed in the X-Api-Key header.
///
/// How it works:
/// 1. Reads the "X-Api-Key" header from the incoming request.
/// 2. Compares it against the configured key.
/// 3. If it matches, the request is authenticated.
/// 4. If not, returns 401 Unauthorized.
///
/// No roles — a valid key grants full access to all endpoints.
/// </summary>
public class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    public ApiKeyAuthHandler(
        IOptionsMonitor<ApiKeyAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Check if the header is present
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing X-Api-Key header."));
        }

        string? apiKey = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Empty X-Api-Key header."));
        }

        // 2. Validate against the configured key
        if (string.IsNullOrWhiteSpace(Options.Key) || !CryptographicEquals(Options.Key, apiKey))
        {
            Logger.LogWarning(
                "Invalid API key attempt from {RemoteIp}.",
                Context.Connection.RemoteIpAddress);
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        // 3. Build the authenticated identity
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "api-client"),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    /// <summary>
    /// Constant-time string comparison to prevent timing side-channel attacks.
    /// </summary>
    private static bool CryptographicEquals(string a, string b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }
}