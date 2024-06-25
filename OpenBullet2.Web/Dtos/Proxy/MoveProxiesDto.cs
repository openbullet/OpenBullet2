namespace OpenBullet2.Web.Dtos.Proxy;

/// <summary>
/// DTO used to move proxies between groups.
/// </summary>
public class MoveProxiesDto : ProxyFiltersDto
{
    /// <summary>
    /// The id of the destination proxy group.
    /// </summary>
    public required int DestinationGroupId { get; set; }
}
