using OpenBullet2.Web.Attributes;
using RuriLib.Models.Jobs.StartConditions;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Information about the start time of a job.
/// </summary>
public class TimeStartConditionDto : PolyDto
{
}

/// <summary>
/// Information about the start time of a job, relative to when the
/// job is first launched.
/// </summary>
[PolyType("relativeTimeStartCondition")]
[MapsFrom(typeof(RelativeTimeStartCondition))]
[MapsTo(typeof(RelativeTimeStartCondition))]
public class RelativeTimeStartConditionDto : TimeStartConditionDto
{
    /// <summary>
    /// After how long the job should start.
    /// </summary>
    public TimeSpan StartAfter { get; set; }
}

/// <summary>
/// Information about the absolute start time of a job.
/// </summary>
[PolyType("absoluteTimeStartCondition")]
[MapsFrom(typeof(AbsoluteTimeStartCondition))]
[MapsTo(typeof(AbsoluteTimeStartCondition))]
public class AbsoluteTimeStartConditionDto : TimeStartConditionDto
{
    /// <summary>
    /// When the job should start.
    /// </summary>
    public DateTime StartAt { get; set; }
}
