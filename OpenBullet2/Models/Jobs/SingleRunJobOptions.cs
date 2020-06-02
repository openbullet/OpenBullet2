using OpenBullet2.Models.Hits;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using System.Collections.Generic;

namespace OpenBullet2.Models.Jobs
{
    public class SingleRunJobOptions : JobOptions
    {
        public string ConfigId { get; set; }
        public string Data { get; set; } = string.Empty;
        public string WordlistType { get; set; } = "Default";
        public JobProxyMode ProxyMode { get; set; } = JobProxyMode.Default;
        public string Proxy { get; set; } = string.Empty;
        public ProxyType ProxyType { get; set; } = ProxyType.Http;
        public List<HitOutputOptions> HitOutputs { get; set; } = new List<HitOutputOptions>();
    }
}
