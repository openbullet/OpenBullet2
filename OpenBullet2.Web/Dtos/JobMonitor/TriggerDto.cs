using OpenBullet2.Web.Attributes;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.Monitor.Triggers;

namespace OpenBullet2.Web.Dtos.JobMonitor;

/// <summary>
/// Generic trigger DTO.
/// </summary>
public class TriggerDto : PolyDto
{
}

/// <summary>
/// Job status trigger DTO.
/// </summary>
[PolyType("jobStatusTrigger")]
[MapsFrom(typeof(JobStatusTrigger))]
[MapsTo(typeof(JobStatusTrigger))]
public class JobStatusTriggerDto : TriggerDto
{
    /// <summary>
    /// The required status.
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Idle;
}

/// <summary>
/// Triggers when a job finishes.
/// </summary>
[PolyType("jobFinishedTrigger")]
[MapsFrom(typeof(JobFinishedTrigger))]
[MapsTo(typeof(JobFinishedTrigger))]
public class JobFinishedTriggerDto : TriggerDto
{
}

/// <summary>
/// Generic trigger for a job that compares a given
/// stat to a given number.
/// </summary>
public class NumComparisonTrigger : TriggerDto
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
/// Triggers when the progress reaches a given threshold.
/// </summary>
[PolyType("progressTrigger")]
[MapsFrom(typeof(ProgressTrigger))]
[MapsTo(typeof(ProgressTrigger))]
public class ProgressTriggerDto : TriggerDto
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

/// <summary>
/// Generic trigger for a job that compares a given
/// stat to a given amount of time.
/// </summary>
public class TimeComparisonTrigger : TriggerDto
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
/// Triggers when the elapsed time reaches a given threshold.
/// </summary>
[PolyType("timeElapsedTrigger")]
[MapsFrom(typeof(TimeElapsedTrigger), false)]
[MapsTo(typeof(TimeElapsedTrigger), false)]
public class TimeElapsedTriggerDto : TimeComparisonTrigger
{
}

/// <summary>
/// Triggers when the remaining time reaches a given threshold.
/// </summary>
[PolyType("timeRemainingTrigger")]
[MapsFrom(typeof(TimeRemainingTrigger), false)]
[MapsTo(typeof(TimeRemainingTrigger), false)]
public class TimeRemainingTriggerDto : TimeComparisonTrigger
{
}
