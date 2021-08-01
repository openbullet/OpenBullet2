using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;

namespace OpenBullet2.Core.Models.Proxies
{
    /// <summary>
    /// Options for a <see cref="RemoteProxySource"/>
    /// </summary>
    public class RemoteProxySourceOptions : ProxySourceOptions
    {
        /// <summary>
        /// The URL to query in order to retrieve the proxies.
        /// The API should return a text-based response with one proxy per line, in a format supported by OB2.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// The default proxy type when not specified by the format of the proxy.
        /// </summary>
        public ProxyType DefaultType { get; set; } = ProxyType.Http;
    }
}
