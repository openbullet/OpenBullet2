using RuriLib.Models.Proxies;

namespace RuriLib.Models.Configs.Settings;

/// <summary>
/// Configures proxy usage for a config.
/// </summary>
public class ProxySettings
{
    /// <summary>
    /// Whether proxies should be used.
    /// </summary>
    public bool UseProxies { get; set; }

    /// <summary>
    /// The maximum number of uses per proxy.
    /// </summary>
    public int MaxUsesPerProxy { get; set; }

    /// <summary>
    /// The ban-loop evasion counter threshold.
    /// </summary>
    public int BanLoopEvasion { get; set; } = 100;

    /// <summary>
    /// Statuses that should ban the current proxy.
    /// </summary>
    public string[] BanProxyStatuses { get; set; } = ["BAN", "ERROR"];

    /// <summary>
    /// The proxy types allowed by the config.
    /// </summary>
    public ProxyType[] AllowedProxyTypes { get; set; } =
    [
        ProxyType.Http,
        ProxyType.Socks4,
        ProxyType.Socks4a,
        ProxyType.Socks5
    ];
}
