namespace RuriLib.Models.Proxies;

/// <summary>
/// Identifies the supported proxy protocol types.
/// </summary>
public enum ProxyType
{
    /// <summary>
    /// An HTTP proxy.
    /// </summary>
    Http = 0,

    /// <summary>
    /// A SOCKS4 proxy.
    /// </summary>
    Socks4 = 1,

    /// <summary>
    /// A SOCKS5 proxy.
    /// </summary>
    Socks5 = 2,

    /// <summary>
    /// A SOCKS4a proxy.
    /// </summary>
    Socks4a = 3
}
