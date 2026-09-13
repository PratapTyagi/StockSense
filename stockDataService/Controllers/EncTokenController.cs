using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StockDataService.Controllers;

/// <summary>
/// Endpoints for managing the daily Zerodha enctoken.
///
/// The token is stored in Redis with a TTL matching Zerodha's session cut-off
/// and is encrypted at rest via ASP.NET Core Data Protection.
///
/// SECURITY: Requires a valid X-Api-Key header.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EncTokenController : ControllerBase
{
    private readonly IEncTokenProvider _tokenProvider;
    private readonly ILogger<EncTokenController> _logger;

    public EncTokenController(
        IEncTokenProvider tokenProvider,
        ILogger<EncTokenController> logger)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    public record SetTokenRequest(string EncToken);

    /// <summary>
    /// Store or replace the daily Zerodha enctoken.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Set([FromBody] SetTokenRequest request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.EncToken))
        {
            return BadRequest(new { error = "encToken is required." });
        }

        await _tokenProvider.SetAsync(request.EncToken, ct);
        _logger.LogInformation("Enctoken updated via API by {Remote}.", HttpContext.Connection.RemoteIpAddress);

        return Ok(new { status = "ok" });
    }

    /// <summary>
    /// Reports whether a valid enctoken is currently cached. Never returns the token itself.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        string? token = await _tokenProvider.GetAsync(ct);
        return Ok(new { configured = !string.IsNullOrEmpty(token) });
    }

    /// <summary>
    /// Explicitly invalidate the current enctoken (forces the next sync to fail loudly
    /// until a fresh one is provided).
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Invalidate(CancellationToken ct)
    {
        await _tokenProvider.InvalidateAsync(ct);
        return NoContent();
    }
}