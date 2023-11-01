namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// The number of bots has changed.
/// </summary>
public class BotsChangedMessage
{
    /// <summary>
    /// The new amount of bots.
    /// </summary>
    public int NewValue { get; set; }
}
