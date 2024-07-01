using RuriLib.Logging;

namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Debugger log of a hit.
/// </summary>
public class MrjHitLogDto
{
    /// <summary>
    /// List of log entries. If null, the bot logger is not enabled.
    /// </summary>
    public List<BotLoggerEntry>? Log { get; set; } = null;
}
