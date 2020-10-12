namespace RuriLib.Models.Jobs
{
    // Has all the underlying TaskManager statuses plus some extra ones like Waiting for additional job-specific features
    public enum JobStatus
    {
        Idle,
        Waiting,
        Starting,
        Running,
        Pausing,
        Paused,
        Stopping,
        Resuming
    }
}
