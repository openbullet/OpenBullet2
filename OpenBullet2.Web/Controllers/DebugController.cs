using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Debugging;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Debug utilities.
/// </summary>
[Admin]
[ApiVersion("1.0")]
public class DebugController : ApiController
{
    private readonly ILogger<DebugController> _logger;

    /// <summary></summary>
    public DebugController(ILogger<DebugController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Trigger the garbage collector.
    /// </summary>
    [HttpPost("gc")]
    [MapToApiVersion("1.0")]
    public ActionResult GarbageCollect(GarbageCollectRequestDto dto)
    {
        GC.Collect(dto.Generations is -1 ? GC.MaxGeneration : dto.Generations, 
            dto.Mode, dto.Blocking, dto.Compacting);
        return Ok();
    }

}
