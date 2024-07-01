using Microsoft.AspNetCore.Mvc;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Check if the server is healthy.
/// </summary>
[ApiVersion("1.0")]
public class HealthController : ApiController
{
    /// <summary>
    /// Check if the server is healthy.
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    public IActionResult HealthCheck()
    {
        return Ok();
    }    
}
