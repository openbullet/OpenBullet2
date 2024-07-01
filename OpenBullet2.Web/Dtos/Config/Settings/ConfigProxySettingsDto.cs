using RuriLib.Models.Proxies;

namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// DTO that contains a config's proxy settings.
/// </summary>
public class ConfigProxySettingsDto
{
    /// <summary>
    /// Whether this config should be used with proxies or not.
    /// </summary>
    public bool UseProxies { get; set; } = false;

    /// <summary>
    /// The maximum number of times a proxy can be used before being banned.
    /// </summary>
    public int MaxUsesPerProxy { get; set; } = 0;

    /// <summary>
    /// The number of times data from the data pool can be retried due
    /// to a RETRY, BAN or ERROR status before it is marked as TOCHECK.
    /// The value 0 disabled this feature, and the config will never be
    /// marked as TOCHECK.
    /// </summary>
    public int BanLoopEvasion { get; set; } = 100;

    /// <summary>
    /// The values of a bot's status that result in banning the proxy at
    /// the end of the execution of the config.
    /// </summary>
    public string[] BanProxyStatuses { get; set; } = { "BAN", "ERROR" };

    /// <summary>
    /// The proxy types that are allowed to be used for this config.
    /// </summary>
    public ProxyType[] AllowedProxyTypes { get; set; } = {
        ProxyType.Http, ProxyType.Socks4, ProxyType.Socks4a, ProxyType.Socks5
    };
}
