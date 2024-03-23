using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.ProxyGroup;

/// <summary>
/// DTO to create a new proxy group.
/// </summary>
public class CreateProxyGroupDto
{
    /// <summary>
    /// The name of the proxy group.
    /// </summary>
    [Required]
    [MinLength(3)]
    [MaxLength(32)]
    public string Name { get; set; } = string.Empty;
}
