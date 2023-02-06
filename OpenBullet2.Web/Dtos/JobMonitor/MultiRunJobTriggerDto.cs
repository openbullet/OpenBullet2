using OpenBullet2.Web.Attributes;
using RuriLib.Models.Conditions.Comparisons;

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
[PolyType("jobFinished")]
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
[PolyType("testedCount")]
public class TestedCountTriggerDto : NumComparisonTrigger
{

}

/// <summary>
/// Triggers when the number of hits reaches a given threshold.
/// </summary>
[PolyType("hitCount")]
public class HitCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of customs reaches a given threshold.
/// </summary>
[PolyType("customCount")]
public class CustomCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of toChecks reaches a given threshold.
/// </summary>
[PolyType("toCheckCount")]
public class ToCheckCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of fails reaches a given threshold.
/// </summary>
[PolyType("failCount")]
public class FailCountTriggerDto : NumComparisonTrigger
{

}

/// <summary>
/// Triggers when the number of retries reaches a given threshold.
/// </summary>
[PolyType("retryCount")]
public class RetryCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of bans reaches a given threshold.
/// </summary>
[PolyType("banCount")]
public class BanCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of errors reaches a given threshold.
/// </summary>
[PolyType("errorCount")]
public class ErrorCountTriggerDto : NumComparisonTrigger
{

}

/// <summary>
/// Triggers when the number of alive proxies reaches a given threshold.
/// </summary>
[PolyType("aliveProxiesCount")]
public class AliveProxiesCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the number of banned proxies reaches a given threshold.
/// </summary>
[PolyType("bannedProxiesCount")]
public class BannedProxiesCountTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the CPM reaches a given threshold.
/// </summary>
[PolyType("cpmCount")]
public class CPMTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the captcha credit reaches a given threshold.
/// </summary>
[PolyType("captchaCredit")]
public class CaptchaCreditTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the progress reaches a given threshold.
/// </summary>
[PolyType("progress")]
public class ProgressTriggerDto : NumComparisonTrigger
{
    
}

/// <summary>
/// Triggers when the elapsed time reaches a given threshold.
/// </summary>
[PolyType("timeElapsed")]
public class TimeElapsedTriggerDto : TimeComparisonTrigger
{

}

/// <summary>
/// Triggers when the remaining time reaches a given threshold.
/// </summary>
[PolyType("timeRemaining")]
public class TimeRemainingTriggerDto : TimeComparisonTrigger
{
    
}
