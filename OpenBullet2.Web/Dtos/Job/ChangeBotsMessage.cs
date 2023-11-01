namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Request to change the number of bots.
/// </summary>
public class ChangeBotsMessage
{
    /// <summary>
    /// The desired number of bots.
    /// </summary>
    public int Desired { get; set; }
}
