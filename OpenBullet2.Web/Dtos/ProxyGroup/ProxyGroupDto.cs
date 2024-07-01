using OpenBullet2.Web.Dtos.User;

namespace OpenBullet2.Web.Dtos.ProxyGroup;

/// <summary>
/// DTO that contains information about a proxy group.
/// </summary>
public class ProxyGroupDto
{
    /// <summary>
    /// The id of the proxy group.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the proxy group.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The owner of this proxy group. Null if owned by the admin user.
    /// </summary>
    public OwnerDto? Owner { get; set; }
}
