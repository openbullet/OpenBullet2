namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Data stats.
/// </summary>
public class MrjDataStatsDto
{
    /// <summary>
    /// The number of hits this job got.
    /// </summary>
    public int Hits { get; set; }

    /// <summary>
    /// The number of custom results this job got.
    /// </summary>
    public int Custom { get; set; }

    /// <summary>
    /// The number of fails this job got.
    /// </summary>
    public int Fails { get; set; }

    /// <summary>
    /// The number of data lines that did not conform to the rules.
    /// </summary>
    public int Invalid { get; set; }

    /// <summary>
    /// The number of times data has been retried.
    /// </summary>
    public int Retried { get; set; }

    /// <summary>
    /// The number of times data has been retried with a new proxy.
    /// </summary>
    public int Banned { get; set; }

    /// <summary>
    /// The number of errors raised during the checking process.
    /// </summary>
    public int Errors { get; set; }

    /// <summary>
    /// The number of results to check.
    /// </summary>
    public int ToCheck { get; set; }

    /// <summary>
    /// The total number of data lines to test.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// The amount of tested data lines.
    /// </summary>
    public long Tested { get; set; }
}
