namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// A new hit has been found.
/// </summary>
public class MrjNewHitMessage
{
    /// <summary>
    /// The hit.
    /// </summary>
    public required MrjHitDto Hit { get; set; }
}
