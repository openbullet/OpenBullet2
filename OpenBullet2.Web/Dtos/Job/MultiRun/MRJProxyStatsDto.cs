namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Proxy stats.
/// </summary>
public class MrjProxyStatsDto
{
    /// <summary>
    /// The total number of proxies available.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// The number of alive proxies.
    /// </summary>
    public int Alive { get; set; }

    /// <summary>
    /// The number of bad proxies.
    /// </summary>
    public int Bad { get; set; }

    /// <summary>
    /// The number of banned proxies.
    /// </summary>
    public int Banned { get; set; }
}
