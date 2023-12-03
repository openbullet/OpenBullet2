namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// A new hit has been found.
/// </summary>
public class MRJNewHitMessage
{
    /// <summary>
    /// The hit.
    /// </summary>
    public required MRJHitDto Hit { get; set; }
}
