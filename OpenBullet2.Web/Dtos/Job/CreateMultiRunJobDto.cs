using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using System.Text.Json;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Information needed to create a multi run job.
/// </summary>
public class CreateMultiRunJobDto
{
    /// <summary>
    /// The ID of the config to use.
    /// </summary>
    public string ConfigId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the jobs.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When the job will start.
    /// </summary>
    public object? StartCondition { get; set; }

    /// <summary>
    /// The amount of bots that will process the data lines concurrently.
    /// </summary>
    public int Bots { get; set; } = 1;

    /// <summary>
    /// The amount of data lines to skip from the start of the data pool.
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// The proxy mode.
    /// </summary>
    public JobProxyMode ProxyMode { get; set; } = JobProxyMode.Default;

    /// <summary>
    /// Whether to shuffle the proxies in the pool before starting the job.
    /// </summary>
    public bool ShuffleProxies { get; set; } = true;

    /// <summary>
    /// The behaviour that should be applied when no more valid proxies are present in the pool.
    /// </summary>
    public NoValidProxyBehaviour NoValidProxyBehaviour { get; set; } = NoValidProxyBehaviour.Reload;

    /// <summary>
    /// How long should proxies be banned for. ONLY use this when <see cref="NoValidProxyBehaviour" />
    /// is set to <see cref="NoValidProxyBehaviour.Unban" />.
    /// </summary>
    public int ProxyBanTimeSeconds { get; set; } = 0;

    /// <summary>
    /// Whether to mark the data lines that are currently being processed as To Check when the job
    /// is aborted, in order to know which items weren't properly checked.
    /// </summary>
    public bool MarkAsToCheckOnAbort { get; set; } = false;

    /// <summary>
    /// Whether to never ban proxies in any case. Use this for rotating proxy services.
    /// </summary>
    public bool NeverBanProxies { get; set; } = false;

    /// <summary>
    /// Whether to allow multiple bots to use the same proxy. Use this for rotating proxy services.
    /// </summary>
    public bool ConcurrentProxyMode { get; set; } = false;

    /// <summary>
    /// The amount of seconds that the pool will wait before reloading all proxies from the sources (periodically).
    /// Set it to 0 to disable this behaviour and only allow the pool to reload proxies when all are banned according
    /// to the value of <see cref="NoValidProxyBehaviour" />.
    /// </summary>
    public int PeriodicReloadIntervalSeconds { get; set; } = 0;

    /// <summary>
    /// The options for the data pool that provides data lines to the job.
    /// </summary>
    public JsonElement? DataPool { get; set; }

    /// <summary>
    /// The options for the proxy sources that will be used to fill the proxy pool whenever it requests a reload.
    /// </summary>
    public List<JsonElement> ProxySources { get; set; } = new();

    /// <summary>
    /// The options for the outputs where hits will be stored.
    /// </summary>
    public List<JsonElement> HitOutputs { get; set; } = new();
}
