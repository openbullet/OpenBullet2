using OpenBullet2.Web.Dtos.Job.ProxyCheck;
using System.Text.Json;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Information needed to create a proxy check job.
/// </summary>
public class CreateProxyCheckJobDto
{
    /// <summary>
    /// The amount of bots that will check the proxies concurrently.
    /// </summary>
    public int Bots { get; set; } = 1;

    /// <summary>
    /// The name of the jobs.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When the job will start.
    /// </summary>
    public object? StartCondition { get; set; }

    /// <summary>
    /// The ID of the proxy group to check.
    /// </summary>
    public int GroupId { get; set; } = -1;

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
    public JsonElement? CheckOutput { get; set; }
}
