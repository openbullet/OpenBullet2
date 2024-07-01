using OpenBullet2.Web.Dtos.Job.MultiRun;
using RuriLib.Models.Jobs;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Detailed information about a multi run job.
/// </summary>
public class MultiRunJobDto : JobDto
{
    /// <summary>
    /// When the job will start.
    /// </summary>
    public required object StartCondition { get; set; }

    /// <summary>
    /// The config of the job.
    /// </summary>
    public JobConfigDto? Config { get; set; }

    /// <summary>
    /// Information about the data pool being used.
    /// </summary>
    public string DataPoolInfo { get; set; } = string.Empty;

    /// <summary>
    /// The number of bots.
    /// </summary>
    public int Bots { get; set; } = 1;

    /// <summary>
    /// How many lines to skip from the start of the data pool.
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// The proxy mode.
    /// </summary>
    public JobProxyMode ProxyMode { get; set; }

    /// <summary>
    /// The list of proxy sources.
    /// </summary>
    public List<string> ProxySources { get; set; } = new();

    /// <summary>
    /// The list of hit outputs.
    /// </summary>
    public List<string> HitOutputs { get; set; } = new();

    /// <summary>
    /// Statistics about the data lines.
    /// </summary>
    public MrjDataStatsDto DataStats { get; set; } = new();

    /// <summary>
    /// Statistics about the proxies.
    /// </summary>
    public MrjProxyStatsDto ProxyStats { get; set; } = new();

    /// <summary>
    /// The number of checks per minute.
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
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    /// The remaining time.
    /// </summary>
    public TimeSpan Remaining { get; set; }

    /// <summary>
    /// The overall progress.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// The list of hits.
    /// </summary>
    public List<MrjHitDto> Hits { get; set; } = new();
}
