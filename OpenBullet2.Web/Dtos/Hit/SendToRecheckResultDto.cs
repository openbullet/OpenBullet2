namespace OpenBullet2.Web.Dtos.Hit;

/// <summary>
/// The result of sending a hit to recheck.
/// </summary>
public class SendToRecheckResultDto
{
    /// <summary>
    /// The job ID of the recheck job.
    /// </summary>
    public required int JobId { get; set; }
}
