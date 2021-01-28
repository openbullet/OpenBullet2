using System.Collections.Generic;

namespace RuriLib.Models.Settings
{
    public class ProxySettings
    {
        public int ProxyConnectTimeoutMilliseconds { get; set; } = 5000;
        public int ProxyReadWriteTimeoutMilliseconds { get; set; } = 10000;
        public List<string> GlobalBanKeys { get; set; } = new List<string>();
        public List<string> GlobalRetryKeys { get; set; } = new List<string>();
    }
}
