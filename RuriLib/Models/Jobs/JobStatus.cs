namespace RuriLib.Models.Jobs;

// Has all the underlying TaskManager statuses plus some extra ones like Waiting for additional job-specific features
/// <summary>
/// Represents the lifecycle states of a job.
/// </summary>
public enum JobStatus
{
    /// <summary>The job is idle.</summary>
    Idle,
    /// <summary>The job is waiting for its start condition.</summary>
    Waiting,
    /// <summary>The job is starting.</summary>
    Starting,
    /// <summary>The job is running.</summary>
    Running,
    /// <summary>The job is pausing.</summary>
    Pausing,
    /// <summary>The job is paused.</summary>
    Paused,
    /// <summary>The job is stopping.</summary>
    Stopping,
    /// <summary>The job is resuming.</summary>
    Resuming
}
