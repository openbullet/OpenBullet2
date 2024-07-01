using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.ConfigDebugger;
using RuriLib.Logging;

namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// The result of a config debug operation.
/// </summary>
public class DebugConfigResultDto
{
    /// <summary>
    /// The log history of the debugger.
    /// </summary>
    public IEnumerable<BotLoggerEntry> Log { get; set; } = Array.Empty<BotLoggerEntry>();
    
    /// <summary>
    /// The list of variables.
    /// </summary>
    public IEnumerable<VariableDto> Variables { get; set; } = Array.Empty<VariableDto>();
    
    /// <summary>
    /// The error message, if any.
    /// </summary>
    public ErrorMessage? Error { get; set; }
}
