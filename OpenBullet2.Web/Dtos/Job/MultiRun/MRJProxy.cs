namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// A proxy in a multi run job.
/// </summary>
public class MRJProxy
{
    /// <summary>
    /// The host of the proxy.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// The port of the proxy.
    /// </summary>
    public int? Port { get; set; }
}
