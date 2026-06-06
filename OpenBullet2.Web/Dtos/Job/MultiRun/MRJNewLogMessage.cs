using RuriLib.Logging;

namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// A new multi-run job log entry was emitted.
/// </summary>
public class MrjNewLogMessage
{
    /// <summary>
    /// The new log entry.
    /// </summary>
    public required BotLoggerEntry NewMessage { get; set; }
}
