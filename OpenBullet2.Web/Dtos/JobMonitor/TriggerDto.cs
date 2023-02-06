using OpenBullet2.Web.Attributes;
using RuriLib.Models.Jobs;

namespace OpenBullet2.Web.Dtos.JobMonitor;

/// <summary>
/// Generic trigger DTO.
/// </summary>
public class TriggerDto : PolyDto
{

}

/// <summary>
/// Job status trigger DTO.
/// </summary>
[PolyType("jobStatus")]
public class JobStatusTriggerDto : TriggerDto
{
    /// <summary>
    /// The required status.
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Idle;
}
