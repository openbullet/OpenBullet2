namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// A command sent to a job.
/// </summary>
public class JobCommandDto
{
    /// <summary>
    /// The id of the job.
    /// </summary>
    public required int JobId { get; set; }

    /// <summary>
    /// If true, the server waits until the command returns
    /// before sending the response. For example, when starting
    /// a job, the response will be sent only when the job finished
    /// waiting and actually started.
    /// </summary>
    public bool Wait { get; set; } = false;
}
