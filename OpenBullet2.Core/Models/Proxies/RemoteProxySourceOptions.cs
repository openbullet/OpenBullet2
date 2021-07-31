using RuriLib.Models.Proxies;

namespace OpenBullet2.Core.Models.Proxies
{
    public class RemoteProxySourceOptions : ProxySourceOptions
    {
        public string Url { get; set; } = string.Empty;
        public ProxyType DefaultType { get; set; } = ProxyType.Http;
    }
}
