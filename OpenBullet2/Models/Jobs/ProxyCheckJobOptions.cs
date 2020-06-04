using OpenBullet2.Models.Proxies;
using OpenBullet2.Models.Settings;

namespace OpenBullet2.Models.Jobs
{
    public class ProxyCheckJobOptions : JobOptions
    {
        public int Bots { get; set; } = 1;
        public int GroupId { get; set; } = -1;
        public bool CheckOnlyUntested { get; set; } = true;
        public ProxyCheckTarget Target { get; set; } = new ProxyCheckTarget();
        public int TimeoutMilliseconds { get; set; } = 10000;
        public ProxyCheckOutputOptions CheckOutput { get; set; }
    }
}
