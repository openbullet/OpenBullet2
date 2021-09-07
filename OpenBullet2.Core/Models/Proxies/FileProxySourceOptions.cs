using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;

namespace OpenBullet2.Core.Models.Proxies
{
    /// <summary>
    /// Options for a <see cref="FileProxySource"/>
    /// </summary>
    public class FileProxySourceOptions : ProxySourceOptions
    {
        /// <summary>
        /// The path to the file where proxies are stored in a UTF-8 text format, one per line,
        /// in a format that is supported by OB2.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// The default proxy type when not specified by the format of the proxy.
        /// </summary>
        public ProxyType DefaultType { get; set; } = ProxyType.Http;
    }
}
