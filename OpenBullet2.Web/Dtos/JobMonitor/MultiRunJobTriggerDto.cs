using OpenBullet2.Web.Attributes;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Jobs.Monitor.Triggers;

namespace OpenBullet2.Web.Dtos.JobMonitor;

/// <summary>
/// A trigger for a multi run job.
/// </summary>
public class MultiRunJobTriggerDto : TriggerDto
{
}

/// <summary>
/// Generic trigger for a multi run job that compares a given
/// stat to a given number.
/// </summary>
public class MrjNumComparisonTrigger : MultiRunJobTriggerDto
{
    /// <summary>
    /// The comparison method.
    /// </summary>
    public NumComparison Comparison { get; set; }

    /// <summary>
    /// The amount to compare to.
    /// </summary>
    public int Amount { get; set; }
}

/// <summary>
/// Triggers when the number of tested data lines reaches a given
/// threshold.
/// </summary>
[PolyType("testedCountTrigger")]
[MapsFrom(typeof(TestedCountTrigger))]
[MapsTo(typeof(TestedCountTrigger))]
public class TestedCountTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the number of hits reaches a given threshold.
/// </summary>
[PolyType("hitCountTrigger")]
[MapsFrom(typeof(HitCountTrigger))]
[MapsTo(typeof(HitCountTrigger))]
public class HitCountTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the number of customs reaches a given threshold.
/// </summary>
[PolyType("customCountTrigger")]
[MapsFrom(typeof(CustomCountTrigger))]
[MapsTo(typeof(CustomCountTrigger))]
public class CustomCountTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the number of toChecks reaches a given threshold.
/// </summary>
[PolyType("toCheckCountTrigger")]
[MapsFrom(typeof(ToCheckCountTrigger))]
[MapsTo(typeof(ToCheckCountTrigger))]
public class ToCheckCountTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the number of fails reaches a given threshold.
/// </summary>
[PolyType("failCountTrigger")]
[MapsFrom(typeof(FailCountTrigger))]
[MapsTo(typeof(FailCountTrigger))]
public class FailCountTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the number of retries reaches a given threshold.
/// </summary>
[PolyType("retryCountTrigger")]
[MapsFrom(typeof(RetryCountTrigger))]
[MapsTo(typeof(RetryCountTrigger))]
public class RetryCountTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the number of bans reaches a given threshold.
/// </summary>
[PolyType("banCountTrigger")]
[MapsFrom(typeof(BanCountTrigger))]
[MapsTo(typeof(BanCountTrigger))]
public class BanCountTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the number of errors reaches a given threshold.
/// </summary>
[PolyType("errorCountTrigger")]
[MapsFrom(typeof(ErrorCountTrigger))]
[MapsTo(typeof(ErrorCountTrigger))]
public class ErrorCountTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the number of alive proxies reaches a given threshold.
/// </summary>
[PolyType("aliveProxiesCountTrigger")]
[MapsFrom(typeof(AliveProxiesCountTrigger))]
[MapsTo(typeof(AliveProxiesCountTrigger))]
public class AliveProxiesCountTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the number of banned proxies reaches a given threshold.
/// </summary>
[PolyType("bannedProxiesCountTrigger")]
[MapsFrom(typeof(BannedProxiesCountTrigger))]
[MapsTo(typeof(BannedProxiesCountTrigger))]
public class BannedProxiesCountTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the CPM reaches a given threshold.
/// </summary>
[PolyType("cpmCountTrigger")]
[MapsFrom(typeof(CPMTrigger))]
[MapsTo(typeof(CPMTrigger))]
public class CpmTriggerDto : MrjNumComparisonTrigger
{
}

/// <summary>
/// Triggers when the captcha credit reaches a given threshold.
/// </summary>
[PolyType("captchaCreditTrigger")]
[MapsFrom(typeof(CaptchaCreditTrigger))]
[MapsTo(typeof(CaptchaCreditTrigger))]
public class CaptchaCreditTriggerDto : MultiRunJobTriggerDto
{
    /// <summary>
    /// The comparison method.
    /// </summary>
    public NumComparison Comparison { get; set; }

    /// <summary>
    /// The amount to compare to.
    /// </summary>
    public float Amount { get; set; }
}
