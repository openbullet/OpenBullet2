using RuriLib.Models.Jobs;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Overview information about a multi run job.
/// </summary>
public class MultiRunJobOverviewDto : JobOverviewDto
{
    /// <summary>
    /// The name of the config.
    /// </summary>
    public string? ConfigName { get; set; }

    /// <summary>
    /// Information about the data pool being used.
    /// </summary>
    public string DataPoolInfo { get; set; } = string.Empty;

    /// <summary>
    /// The number of bots.
    /// </summary>
    public int Bots { get; set; }

    /// <summary>
    /// Whether to use proxies (depends on both the proxy mode
    /// and the config's default).
    /// </summary>
    public bool UseProxies { get; set; }

    /// <summary>
    /// The number of hits this job got.
    /// </summary>
    public int DataHits { get; set; }

    /// <summary>
    /// The number of custom results this job got.
    /// </summary>
    public int DataCustom { get; set; }

    /// <summary>
    /// The number of results to check.
    /// </summary>
    public int DataToCheck { get; set; }

    /// <summary>
    /// The total number of data lines to test.
    /// </summary>
    public long DataTotal { get; set; }

    /// <summary>
    /// The amount of tested data lines.
    /// </summary>
    public long DataTested { get; set; }

    /// <summary>
    /// The number of checks per minute.
    /// </summary>
    public int CPM { get; set; }

    /// <summary>
    /// The overall progress.
    /// </summary>
    public double Progress { get; set; }
}
