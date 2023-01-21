using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Proxy;

/// <summary>
/// DTO that contains information about a remote source of proxies
/// that need to be saved and added to a group.
/// </summary>
public class AddProxiesFromRemoteDto : AddProxiesDto
{
    /// <summary>
    /// The URL where the proxies can be downloaded from.
    /// </summary>
    [Required]
    public string Url { get; set; } = string.Empty;
}
