using OpenBullet2.Web.Dtos.Job.ProxyCheck;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Details about a proxy check job.
/// </summary>
public class ProxyCheckJobDto : JobDto
{
    /// <summary>
    /// When the job will start.
    /// </summary>
    public required object StartCondition { get; set; }

    /// <summary>
    /// The amount of bots that will check the proxies concurrently.
    /// </summary>
    public int Bots { get; set; } = 1;

    /// <summary>
    /// The ID of the proxy group to check.
    /// </summary>
    public int GroupId { get; set; } = -1;

    /// <summary>
    /// The name of the proxy group to check.
    /// </summary>
    public string GroupName { get; set; } = "All";

    /// <summary>
    /// Whether to only check the proxies that were never been tested.
    /// </summary>
    public bool CheckOnlyUntested { get; set; } = true;

    /// <summary>
    /// The target site against which proxies should be checked.
    /// </summary>
    public ProxyCheckTargetDto? Target { get; set; } = null;

    /// <summary>
    /// The maximum timeout that a valid proxy should have, in milliseconds.
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 10000;

    /// <summary>
    /// The options for the output of a proxy check.
    /// </summary>
    public string CheckOutput { get; set; } = string.Empty;

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
}
