using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Web.Auth;
using OpenBullet2.Web.Dtos.Debugging;
using OpenBullet2.Web.Exceptions;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Debug utilities.
/// </summary>
[TypeFilter<AdminFilter>]
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
            dto.Mode, dto.Blocking, dto.Compacting);
#pragma warning restore S1215
        return Ok();
    }
    
    /// <summary>
    /// Get the current server log as a text file.
    /// </summary>
    [HttpGet("server-logs")]
    [MapToApiVersion("1.0")]
    public ActionResult GetServerLogs()
    {
        // This is only supported for the default Serilog configuration
        var logsFolder = Path.Combine(Globals.UserDataFolder, "Logs");
        var latestLog = Directory.GetFiles(logsFolder, "log-*.txt").MaxBy(f => f);

        if (latestLog is null)
        {
            throw new ResourceNotFoundException(ErrorCode.FileNotFound,
                "No logs found on the server or the Serilog configuration is not the default one");
        }
        
        var stream = new FileStream(latestLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return File(stream, "text/plain", Path.GetFileName(latestLog));
    }
}
