using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.ProxyGroup;

/// <summary>
/// DTO to update a proxy group.
/// </summary>
public class UpdateProxyGroupDto
{
    /// <summary>
    /// The id of the proxy group.
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// The name of the proxy group.
    /// </summary>
    [Required, MinLength(3), MaxLength(32)]
    public string Name { get; set; } = default!;
}
