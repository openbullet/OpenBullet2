namespace RuriLib.Models.Jobs.Threading
{
    public enum TaskManagerStatus
    {
        Idle,
        Starting,
        Running,
        Pausing,
        Paused,
        Stopping,
        Resuming
    }
}
