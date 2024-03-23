namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Message sent when a new result is generated.
/// </summary>
public class MrjNewResultMessage
{
    /// <summary>
    /// The data line that generated this result.
    /// </summary>
    public string DataLine { get; set; } = string.Empty;

    /// <summary>
    /// The proxy, if any.
    /// </summary>
    public MrjProxy? Proxy { get; set; }

    /// <summary>
    /// The final status of the bot.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
