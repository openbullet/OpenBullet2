using Microsoft.AspNetCore.Mvc;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Base API controller.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ApiController : ControllerBase
{

}
