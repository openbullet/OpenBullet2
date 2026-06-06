using System;

namespace RuriLib.Models.Jobs.StartConditions;

/// <summary>
/// Starts a job at an absolute point in time.
/// </summary>
public class AbsoluteTimeStartCondition : StartCondition
{
    /// <summary>
    /// Gets or sets the date and time after which the job may start.
    /// </summary>
    public DateTime StartAt { get; set; } = DateTime.Now + TimeSpan.FromMinutes(1);

    /// <inheritdoc />
    public override bool Verify(Job job)
        => StartAt < DateTime.Now;
}
