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
/// Triggers when a job finishes.
/// </summary>
[PolyType("jobFinishedTrigger")]
[MapsFrom(typeof(JobFinishedTrigger))]
[MapsTo(typeof(JobFinishedTrigger))]
public class JobFinishedTriggerDto : MultiRunJobTriggerDto
{

}

/// <summary>
/// Generic trigger for a multi run job that compares a given
/// stat to a given number.
/// </summary>
public class NumComparisonTrigger : MultiRunJobTriggerDto
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
/// Generic trigger for a multi run job that compares a given
/// stat to a given amount of time.
/// </summary>
public class TimeComparisonTrigger : MultiRunJobTriggerDto
{
    /// <summary>
    /// The comparison method.
    /// </summary>
    public NumComparison Comparison { get; set; }

    /// <summary>
    /// The amount of time to compare to.
    /// </summary>
    public TimeSpan TimeSpan { get; set; }
}

/// <summary>
/// Triggers when the number of tested data lines reaches a given
/// threshold.
/// </summary>
[PolyType("testedCountTrigger")]
[MapsFrom(typeof(TestedCountTrigger))]
[MapsTo(typeof(TestedCountTrigger))]
public class TestedCountTriggerDto : NumComparisonTrigger
{

}

/// <summary>
/// Triggers when the number of hits reaches a given threshold.
/// </summary>
[PolyType("hitCountTrigger")]
[MapsFrom(typeof(HitCountTrigger))]
[MapsTo(typeof(HitCountTrigger))]
public class HitCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of customs reaches a given threshold.
/// </summary>
[PolyType("customCountTrigger")]
[MapsFrom(typeof(CustomCountTrigger))]
[MapsTo(typeof(CustomCountTrigger))]
public class CustomCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of toChecks reaches a given threshold.
/// </summary>
[PolyType("toCheckCountTrigger")]
[MapsFrom(typeof(ToCheckCountTrigger))]
[MapsTo(typeof(ToCheckCountTrigger))]
public class ToCheckCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of fails reaches a given threshold.
/// </summary>
[PolyType("failCountTrigger")]
[MapsFrom(typeof(FailCountTrigger))]
[MapsTo(typeof(FailCountTrigger))]
public class FailCountTriggerDto : NumComparisonTrigger
{

}

/// <summary>
/// Triggers when the number of retries reaches a given threshold.
/// </summary>
[PolyType("retryCountTrigger")]
[MapsFrom(typeof(RetryCountTrigger))]
[MapsTo(typeof(RetryCountTrigger))]
public class RetryCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of bans reaches a given threshold.
/// </summary>
[PolyType("banCountTrigger")]
[MapsFrom(typeof(BanCountTrigger))]
[MapsTo(typeof(BanCountTrigger))]
public class BanCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of errors reaches a given threshold.
/// </summary>
[PolyType("errorCountTrigger")]
[MapsFrom(typeof(ErrorCountTrigger))]
[MapsTo(typeof(ErrorCountTrigger))]
public class ErrorCountTriggerDto : NumComparisonTrigger
{

}

/// <summary>
/// Triggers when the number of alive proxies reaches a given threshold.
/// </summary>
[PolyType("aliveProxiesCountTrigger")]
[MapsFrom(typeof(AliveProxiesCountTrigger))]
[MapsTo(typeof(AliveProxiesCountTrigger))]
public class AliveProxiesCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of banned proxies reaches a given threshold.
/// </summary>
[PolyType("bannedProxiesCountTrigger")]
[MapsFrom(typeof(BannedProxiesCountTrigger))]
[MapsTo(typeof(BannedProxiesCountTrigger))]
public class BannedProxiesCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the CPM reaches a given threshold.
/// </summary>
[PolyType("cpmCountTrigger")]
[MapsFrom(typeof(CPMTrigger))]
[MapsTo(typeof(CPMTrigger))]
public class CPMTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the captcha credit reaches a given threshold.
/// </summary>
[PolyType("captchaCreditTrigger")]
[MapsFrom(typeof(CaptchaCreditTrigger))]
[MapsTo(typeof(CaptchaCreditTrigger))]
public class CaptchaCreditTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the progress reaches a given threshold.
/// </summary>
[PolyType("progressTrigger")]
[MapsFrom(typeof(ProgressTrigger))]
[MapsTo(typeof(ProgressTrigger))]
public class ProgressTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the elapsed time reaches a given threshold.
/// </summary>
[PolyType("timeElapsedTrigger")]
[MapsFrom(typeof(TimeElapsedTrigger), autoMap: false)]
[MapsTo(typeof(TimeElapsedTrigger), autoMap: false)]
public class TimeElapsedTriggerDto : TimeComparisonTrigger
{

}

/// <summary>
/// Triggers when the remaining time reaches a given threshold.
/// </summary>
[PolyType("timeRemainingTrigger")]
[MapsFrom(typeof(TimeRemainingTrigger), autoMap: false)]
[MapsTo(typeof(TimeRemainingTrigger), autoMap: false)]
public class TimeRemainingTriggerDto : TimeComparisonTrigger
{
    
}
