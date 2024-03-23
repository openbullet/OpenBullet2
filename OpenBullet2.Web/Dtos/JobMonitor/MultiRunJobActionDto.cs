using OpenBullet2.Web.Attributes;
using RuriLib.Models.Jobs.Monitor.Actions;

namespace OpenBullet2.Web.Dtos.JobMonitor;

/// <summary>
/// An action for a multi run job.
/// </summary>
public class MultiRunJobActionDto : ActionDto
{
}

/// <summary>
/// Sets the number of bots.
/// </summary>
[PolyType("setBotsAction")]
[MapsFrom(typeof(SetBotsAction))]
[MapsTo(typeof(SetBotsAction))]
public class SetBotsActionDto : MultiRunJobActionDto
{
    /// <summary>
    /// The new amount of bots.
    /// </summary>
    public int Amount { get; set; }
}

/// <summary>
/// Reloads proxies.
/// </summary>
[PolyType("reloadProxiesAction")]
[MapsFrom(typeof(ReloadProxiesAction))]
[MapsTo(typeof(ReloadProxiesAction))]
public class ReloadProxiesActionDto : MultiRunJobActionDto
{
}
