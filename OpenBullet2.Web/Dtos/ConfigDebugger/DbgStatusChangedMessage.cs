using RuriLib.Models.Debugger;

namespace OpenBullet2.Web.Dtos.ConfigDebugger;

/// <summary>
/// The status changed.
/// </summary>
public class DbgStatusChangedMessage
{
    /// <summary>
    /// The new status.
    /// </summary>
    public ConfigDebuggerStatus NewStatus { get; set; }
}
