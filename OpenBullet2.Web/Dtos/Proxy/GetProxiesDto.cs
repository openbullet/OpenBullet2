using OpenBullet2.Web.Dtos.Common;
using RuriLib.Models.Proxies;

namespace OpenBullet2.Web.Dtos.Proxy;

/// <summary>
/// Filters to get all proxies.
/// </summary>
public class GetProxiesDto : PaginationDto
{
    /// <summary>
    /// The search term to filter results by the proxy host,
    /// port, username or country. Optional.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// The proxy type filter, if any.
    /// </summary>
    public ProxyType? Type { get; set; }

    /// <summary>
    /// The proxy status filter, if any.
    /// </summary>
    public ProxyWorkingStatus? Status { get; set; }
}
