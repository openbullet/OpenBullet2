using RuriLib.Models.Proxies;

namespace RuriLib.Models.Configs.Settings
{
    public class ProxySettings
    {
        public bool UseProxies { get; set; } = false;

        public int MaxUsesPerProxy { get; set; } = 0;

        public int BanLoopEvasion { get; set; } = 100;

        public string[] BanProxyStatuses { get; set; } = new string[] { "BAN", "ERROR" };

        public ProxyType[] AllowedProxyTypes { get; set; } = new ProxyType[]
        {
            ProxyType.Http,
            ProxyType.Socks4,
            ProxyType.Socks4a,
            ProxyType.Socks5
        };
    }
}
