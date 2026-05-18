namespace RuriLib.Models.Jobs;

/// <summary>
/// Represents the terminal outcome of the most recent job run.
/// </summary>
public enum JobLastRunOutcome
{
    /// <summary>No run outcome is available.</summary>
    None,
    /// <summary>The run completed all of its work.</summary>
    Completed,
    /// <summary>The run was stopped before finishing all work.</summary>
    Stopped,
    /// <summary>The run was aborted.</summary>
    Aborted,
    /// <summary>The run ended because of an unrecoverable error.</summary>
    Failed
}
