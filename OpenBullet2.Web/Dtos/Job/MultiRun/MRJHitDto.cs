namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// A hit obtained by a multi run job.
/// </summary>
public class MrjHitDto
{
    /// <summary>
    /// The temporary id of the hit in memory.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// When the hit was obtained.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The type of hit (SUCCESS, NONE or other like CUSTOM).
    /// </summary>
    public string Type { get; set; } = "SUCCESS";

    /// <summary>
    /// The original data line from which the hit was obtained.
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// The proxy that was used to obtain the hit, if any.
    /// </summary>
    public MrjProxy? Proxy { get; set; } = null;

    /// <summary>
    /// The data captured by the config's script.
    /// </summary>
    public string CapturedData { get; set; } = string.Empty;
}
