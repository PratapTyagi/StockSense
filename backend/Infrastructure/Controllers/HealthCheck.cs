using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;
/// <summary>
/// Controller for checking health of backend server.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Checks health of the app
    /// </summary>
    /// <returns>A message that says our app is working fine.</returns>
    [HttpGet()]
    public async Task<IActionResult> GetWatchlist()
    {
        return Ok("App is working fine");
    }
}