using RuriLib.Models.Jobs;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// The status of a job has changed.
/// </summary>
public class JobStatusChangedMessage
{
    /// <summary>
    /// The new status.
    /// </summary>
    public JobStatus NewStatus { get; set; }
}
