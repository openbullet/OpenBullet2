using RuriLib.Models.Proxies;

namespace OpenBullet2.Models.Proxies
{
    public class FileProxySourceOptions : ProxySourceOptions
    {
        public string FileName { get; set; } = string.Empty;
        public ProxyType DefaultType { get; set; } = ProxyType.Http;
    }
}
