namespace OpenBullet2.Web.Dtos.Proxy;

/// <summary>
/// DTO used to delete proxies by selected low-quality buckets.
/// </summary>
public class DeleteLowQualityProxiesDto
{
    /// <summary>
    /// The id of the proxy group. Set to -1 for all, where supported.
    /// </summary>
    public int ProxyGroupId { get; set; } = -1;

    /// <summary>
    /// Whether to delete proxies with unknown quality.
    /// </summary>
    public bool DeleteUnknown { get; set; } = true;

    /// <summary>
    /// Whether to delete transparent proxies.
    /// </summary>
    public bool DeleteTransparent { get; set; } = true;

    /// <summary>
    /// Whether to delete anonymous proxies.
    /// </summary>
    public bool DeleteAnonymous { get; set; } = true;
}
