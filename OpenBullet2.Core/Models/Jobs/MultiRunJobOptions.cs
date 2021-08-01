using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Proxies;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using System.Collections.Generic;

namespace OpenBullet2.Core.Models.Jobs
{
    /// <summary>
    /// Options for a <see cref="MultiRunJob"/>.
    /// </summary>
    public class MultiRunJobOptions : JobOptions
    {
        /// <summary>
        /// The ID of the config to use.
        /// </summary>
        public string ConfigId { get; set; }

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
        /// How long should proxies be banned for. ONLY use this when <see cref="NoValidProxyBehaviour"/>
        /// is set to <see cref="NoValidProxyBehaviour.Unban"/>.
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
        /// to the value of <see cref="NoValidProxyBehaviour"/>.
        /// </summary>
        public int PeriodicReloadIntervalSeconds { get; set; } = 0;

        /// <summary>
        /// The options for the data pool that provides data lines to the job.
        /// </summary>
        public DataPoolOptions DataPool { get; set; } = new WordlistDataPoolOptions();

        /// <summary>
        /// The options for the proxy sources that will be used to fill the proxy pool whenever it requests a reload.
        /// </summary>
        public List<ProxySourceOptions> ProxySources { get; set; } = new List<ProxySourceOptions>();

        /// <summary>
        /// The options for the outputs where hits will be stored.
        /// </summary>
        public List<HitOutputOptions> HitOutputs { get; set; } = new List<HitOutputOptions>();
    }
}
