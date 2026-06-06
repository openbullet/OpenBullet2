namespace RuriLib.Models.Proxies;

/// <summary>
/// Configures proxy-pool filtering behavior.
/// </summary>
public class ProxyPoolOptions
{
    /// <summary>
    /// Gets or sets the proxy types allowed in the pool.
    /// </summary>
    public ProxyType[] AllowedTypes { get; set; } = [ProxyType.Http, ProxyType.Socks4, ProxyType.Socks5];
}
