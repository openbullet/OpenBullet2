namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Overview information about a proxy check job.
/// </summary>
public class ProxyCheckJobOverviewDto : JobOverviewDto
{
    /// <summary>
    /// The number of bots.
    /// </summary>
    public int Bots { get; set; }

    /// <summary>
    /// The total number of proxies to test.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// The amount of tested proxies.
    /// </summary>
    public long Tested { get; set; }

    /// <summary>
    /// How many proxies tested as working.
    /// </summary>
    public int Working { get; set; }

    /// <summary>
    /// How many proxies tested as not working.
    /// </summary>
    public int NotWorking { get; set; }

    /// <summary>
    /// The number of checks per minute.
    /// </summary>
    public int CPM { get; set; }

    /// <summary>
    /// The overall progress.
    /// </summary>
    public double Progress { get; set; }
}
