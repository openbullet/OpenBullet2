using RuriLib.Models.Conditions.Comparisons;
using System;

namespace RuriLib.Models.Jobs.Monitor.Triggers;

/// <summary>
/// Represents a condition evaluated by the job monitor.
/// </summary>
public abstract class Trigger
{
    /// <summary>
    /// Checks whether the trigger matches the given job.
    /// </summary>
    /// <param name="job">The job to inspect.</param>
    /// <returns><see langword="true"/> if the trigger matches.</returns>
    public virtual bool CheckStatus(Job job)
        => throw new NotImplementedException();
}

/// <summary>
/// Matches a specific job status.
/// </summary>
public class JobStatusTrigger : Trigger
{
    /// <summary>Gets or sets the expected job status.</summary>
    public JobStatus Status { get; set; } = JobStatus.Idle;

    /// <inheritdoc />
    public override bool CheckStatus(Job job)
        => job.Status == Status;
}

/// <summary>
/// Matches jobs that finished successfully.
/// </summary>
public class JobFinishedTrigger : Trigger
{
    /// <inheritdoc />
    public override bool CheckStatus(Job job)
        => job.Status == JobStatus.Idle && job.LastRunOutcome == JobLastRunOutcome.Completed;
}

/// <summary>
/// Matches jobs based on progress percentage.
/// </summary>
public class ProgressTrigger : Trigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public float Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(Job job)
        => Functions.Conditions.Conditions.Check(job.Progress * 100, Comparison, Amount);
}

/// <summary>
/// Matches jobs based on elapsed time.
/// </summary>
public class TimeElapsedTrigger : Trigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the seconds component.</summary>
    public int Seconds { get; set; }
    /// <summary>Gets or sets the minutes component.</summary>
    public int Minutes { get; set; }
    /// <summary>Gets or sets the hours component.</summary>
    public int Hours { get; set; }
    /// <summary>Gets or sets the days component.</summary>
    public int Days { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(Job job)
        => Functions.Conditions.Conditions.Check(job.Elapsed, Comparison, new TimeSpan(Days, Hours, Minutes, Seconds));
}

/// <summary>
/// Matches jobs based on remaining time.
/// </summary>
public class TimeRemainingTrigger : Trigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the seconds component.</summary>
    public int Seconds { get; set; }
    /// <summary>Gets or sets the minutes component.</summary>
    public int Minutes { get; set; }
    /// <summary>Gets or sets the hours component.</summary>
    public int Hours { get; set; }
    /// <summary>Gets or sets the days component.</summary>
    public int Days { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(Job job)
        => Functions.Conditions.Conditions.Check(job.Remaining, Comparison, new TimeSpan(Days, Hours, Minutes, Seconds));
}
