using OpenBullet2.Web.Attributes;

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
[PolyType("setBots")]
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
[PolyType("reloadProxies")]
public class ReloadProxiesActionDto : MultiRunJobActionDto
{

}
