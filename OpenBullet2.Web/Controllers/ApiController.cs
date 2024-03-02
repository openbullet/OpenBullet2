using Microsoft.AspNetCore.Mvc;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Base API controller.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ApiController : ControllerBase
{
    /// <summary>
    /// Sets a warning message in the HTTP response
    /// via the X-Application-Warning header.
    /// </summary>
    // TODO: Trigger a toast without timer in frontend via the
    // http interceptor if this header is set.
    protected void SetWarning(string message) => 
        Response.Headers.Append("X-Application-Warning", message);
}
