using RuriLib.Logging;

namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Debugger log of a hit.
/// </summary>
public class MRJHitLogDto
{
    /// <summary>
    /// List of log entries.
    /// </summary>
    public List<BotLoggerEntry> Log { get; set; } = new();
}
