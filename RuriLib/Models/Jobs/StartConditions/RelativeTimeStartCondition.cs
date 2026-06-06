using System;

namespace RuriLib.Models.Jobs.StartConditions;

/// <summary>
/// Starts a job after a relative delay from its start timestamp.
/// </summary>
public class RelativeTimeStartCondition : StartCondition
{
    /// <summary>
    /// Gets or sets the delay that must elapse before the job can start.
    /// </summary>
    public TimeSpan StartAfter { get; set; } = TimeSpan.Zero;

    /// <inheritdoc />
    public override bool Verify(Job job)
        => StartAfter < DateTime.Now - job.StartTime;
}
