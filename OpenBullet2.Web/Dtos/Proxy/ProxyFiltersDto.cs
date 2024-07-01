using OpenBullet2.Web.Dtos.Common;
using RuriLib.Models.Proxies;

namespace OpenBullet2.Web.Dtos.Proxy;

/// <summary>
/// Filters to describe a subset of proxies.
/// </summary>
public class ProxyFiltersDto : PaginationDto
{
    /// <summary>
    /// The id of the proxy group. Set to -1 for all, where supported.
    /// </summary>
    public int ProxyGroupId { get; set; } = -1;

    /// <summary>
    /// The search term to filter results by the proxy host,
    /// port, username or country. Optional.
    /// </summary>
    public string? SearchTerm { get; set; } = null;

    /// <summary>
    /// The proxy type filter, if any.
    /// </summary>
    public ProxyType? Type { get; set; } = null;

    /// <summary>
    /// The proxy status filter, if any.
    /// </summary>
    public ProxyWorkingStatus? Status { get; set; } = null;
    
    /// <summary>
    /// The field to sort proxies by, if any.
    /// </summary>
    public ProxySortField? SortBy { get; set; } = null;
    
    /// <summary>
    /// Whether to sort the proxies in descending order.
    /// </summary>
    public bool SortDescending { get; set; } = false;
}
