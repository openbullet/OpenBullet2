using OpenBullet2.Models.Data;
using OpenBullet2.Models.Hits;
using OpenBullet2.Models.Proxies;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using System.Collections.Generic;

namespace OpenBullet2.Models.Jobs
{
    public class MultiRunJobOptions : JobOptions
    {
        public string ConfigId { get; set; }
        public int Bots { get; set; } = 1;
        public int Skip { get; set; } = 0;
        public JobProxyMode ProxyMode { get; set; } = JobProxyMode.Default;
        public bool ShuffleProxies { get; set; } = true;
        public NoValidProxyBehaviour NoValidProxyBehaviour { get; set; } = NoValidProxyBehaviour.Reload;
        public int ProxyBanTimeSeconds { get; set; } = 0;
        public bool MarkAsToCheckOnAbort { get; set; } = false;
        public bool NeverBanProxies { get; set; } = false;
        public bool ConcurrentProxyMode { get; set; } = false;
        public int PeriodicReloadIntervalSeconds { get; set; } = 0;
        public DataPoolOptions DataPool { get; set; } = new WordlistDataPoolOptions();
        public List<ProxySourceOptions> ProxySources { get; set; } = new List<ProxySourceOptions>();
        public List<HitOutputOptions> HitOutputs { get; set; } = new List<HitOutputOptions>();
    }
}
