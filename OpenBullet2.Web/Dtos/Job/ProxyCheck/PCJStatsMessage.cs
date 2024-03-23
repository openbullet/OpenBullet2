namespace OpenBullet2.Web.Dtos.Job.ProxyCheck;

/// <summary>
/// Sent at regular intervals while the job runs, contains
/// information about the current stats.
/// </summary>
public class PcjStatsMessage
{
    /// <summary>
    /// The number of tested proxies.
    /// </summary>
    public int Tested { get; set; }

    /// <summary>
    /// The number of working proxies.
    /// </summary>
    public int Working { get; set; }

    /// <summary>
    /// The number of not working proxies.
    /// </summary>
    public int NotWorking { get; set; }

    /// <summary>
    /// The checks per minute.
    /// </summary>
    public int CPM { get; set; }

    /// <summary>
    /// The elapsed time.
    /// </summary>
    /// <!--(note: we could pass the start time here and we calc client-side)-->
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    /// The remaining time.
    /// </summary>
    /// <!--(note: we could calc this client-side from elapsed and cpm)-->
    public TimeSpan Remaining { get; set; }

    /// <summary>
    /// The progress.
    /// </summary>
    /// <!--(note: we could calc this client-side from total and tested)-->
    public double Progress { get; set; }
}
