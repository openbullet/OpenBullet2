using OpenBullet2.Models.Proxies;

namespace OpenBullet2.Models.Jobs
{
    public class ProxyCheckJobOptions : JobOptions
    {
        public int Bots { get; set; } = 1;
        public int GroupId { get; set; } = -1;
        public bool CheckOnlyUntested { get; set; } = true;
        public string Url { get; set; } = "https://google.com";
        public string SuccessKey { get; set; } = "title>Google";
        public int TimeoutMilliseconds { get; set; } = 10000;
        public ProxyCheckOutputOptions CheckOutput { get; set; }
    }
}
