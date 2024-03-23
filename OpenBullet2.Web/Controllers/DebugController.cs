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
    /// <summary>
    /// Trigger the garbage collector.
    /// </summary>
    [HttpPost("gc")]
    [MapToApiVersion("1.0")]
    public ActionResult GarbageCollect(GarbageCollectRequestDto dto)
    {
#pragma warning disable S1215
        GC.Collect(dto.Generations is -1 ? GC.MaxGeneration : dto.Generations,
#pragma warning restore S1215
            dto.Mode, dto.Blocking, dto.Compacting);
        return Ok();
    }

}
