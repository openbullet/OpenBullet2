namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// A new hit has been found.
/// </summary>
public class MRJNewHitMessage
{
    /// <summary>
    /// The data line that generated this hit.
    /// </summary>
    public string DataLine { get; set; } = string.Empty;

    /// <summary>
    /// The proxy used if any.
    /// </summary>
    public MRJProxy? Proxy { get; set; }

    /// <summary>
    /// The date at which the hit was obtained.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The type of hit.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The string representation of the captured variables.
    /// </summary>
    public string CaptureString { get; set; } = string.Empty;
}
