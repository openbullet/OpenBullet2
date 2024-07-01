using RuriLib.Logging;
using RuriLib.Models.Debugger;

namespace OpenBullet2.Web.Dtos.ConfigDebugger;

/// <summary>
/// The current state of a debugger.
/// </summary>
public class DbgStateDto
{
    /// <summary>
    /// The current status of the debugger.
    /// </summary>
    public ConfigDebuggerStatus Status { get; set; }

    /// <summary>
    /// The current log history of the debugger.
    /// </summary>
    public IEnumerable<BotLoggerEntry> Log { get; set; } = Array.Empty<BotLoggerEntry>();

    /// <summary>
    /// The list of variables.
    /// </summary>
    public IEnumerable<VariableDto> Variables { get; set; } = Array.Empty<VariableDto>();
}
