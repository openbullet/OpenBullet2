using OpenBullet2.Models.Data;
using OpenBullet2.Models.Hits;
using OpenBullet2.Models.Proxies;
using RuriLib.Models.Jobs;
using System.Collections.Generic;

namespace OpenBullet2.Models.Jobs
{
    public class MultiRunJobOptions : JobOptions
    {
        public string ConfigId { get; set; }
        public int Bots { get; set; } = 1;
        public int Skip { get; set; } = 0;
        public JobProxyMode ProxyMode { get; set; } = JobProxyMode.Default;
        public DataPoolOptions DataPool { get; set; } = new WordlistDataPoolOptions();
        public ProxySourceOptions ProxySource { get; set; } = new GroupProxySourceOptions();
        public List<HitOutputOptions> HitOutputs { get; set; } = new List<HitOutputOptions>();
    }
}
