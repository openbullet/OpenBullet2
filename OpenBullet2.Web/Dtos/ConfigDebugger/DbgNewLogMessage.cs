using RuriLib.Logging;

namespace OpenBullet2.Web.Dtos.ConfigDebugger;

/// <summary>
/// A new message was logged.
/// </summary>
public class DbgNewLogMessage
{
    /// <summary>
    /// The new log message.
    /// </summary>
    public BotLoggerEntry? NewMessage { get; set; }
}
