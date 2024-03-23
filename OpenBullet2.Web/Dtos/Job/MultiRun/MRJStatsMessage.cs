namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Sent at regular intervals while the job runs, contains
/// information about the current stats.
/// </summary>
public class MrjStatsMessage
{
    /// <summary>
    /// Statistics about the data lines.
    /// </summary>
    public MrjDataStatsDto DataStats { get; set; } = new();

    /// <summary>
    /// Statistics about the proxies.
    /// </summary>
    public MrjProxyStatsDto ProxyStats { get; set; } = new();

    /// <summary>
    /// The checks per minute.
    /// </summary>
    public int CPM { get; set; }

    /// <summary>
    /// The number of captcha credits left, if a captcha solver
    /// is being used.
    /// </summary>
    public decimal CaptchaCredit { get; set; }

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
