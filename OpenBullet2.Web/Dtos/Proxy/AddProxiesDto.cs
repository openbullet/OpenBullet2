using RuriLib.Models.Proxies;
using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Proxy;

/// <summary>
/// DTO that contains information about proxies that need to be
/// saved and added to a group.
/// </summary>
public abstract class AddProxiesDto
{
    /// <summary>
    /// The proxy type to use when not specified in the string,
    /// http by default.
    /// </summary>
    public ProxyType DefaultType { get; set; } = ProxyType.Http;

    /// <summary>
    /// The default username to use when not specified. Empty
    /// if the proxies do not require authentication.
    /// </summary>
    public string DefaultUsername { get; set; } = string.Empty;

    /// <summary>
    /// The default password to use when not specified. Empty
    /// if the proxies do not require authentication.
    /// </summary>
    public string DefaultPassword { get; set; } = string.Empty;

    /// <summary>
    /// The id of the proxy group to which proxies should be assigned.
    /// </summary>
    public required int ProxyGroupId { get; set; }
}
