using OpenBullet2.Core.Models.Jobs;

namespace OpenBullet2.Web.Dtos.JobMonitor;

/// <summary>
/// DTO for a triggered action in the job monitor.
/// </summary>
public class TriggeredActionDto
{
    /// <summary>
    /// The id of the triggered action.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The name of the triggered action.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the triggered action is currently able to be triggered
    /// or not.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the action can be triggered multiple times.
    /// </summary>
    public bool IsRepeatable { get; set; } = false;

    /// <summary>
    /// The number of times the action was executed so far.
    /// </summary>
    public int Executions { get; private set; } = 0;

    /// <summary>
    /// The job this triggered action refers to.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// The name of the job this triggered action refers to.
    /// </summary>
    public string JobName { get; set; } = "Invalid job";

    /// <summary>
    /// The type of job this triggered action refers to.
    /// </summary>
    public JobType JobType { get; set; } = JobType.MultiRun;

    /// <summary>
    /// All triggers that must be verified at the same time in order
    /// to start the execution of the action.
    /// </summary>
    public List<object> Triggers { get; set; } = new();

    /// <summary>
    /// All actions that will be executed sequentially when the
    /// triggering conditions are verified.
    /// </summary>
    public List<object> Actions { get; set; } = new();
}
