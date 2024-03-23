using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Proxy;

/// <summary>
/// DTO that contains a list of proxies that need to be
/// saved and added to a group.
/// </summary>
public class AddProxiesFromListDto : AddProxiesDto
{
    /// <summary>
    /// The list of proxies to add. Each entry should contain a proxy
    /// in this format (type)host:port:username:password, where type
    /// can be http, socks4, socks4a or socks5. The username:password
    /// part is optional.
    /// </summary>
    /// <example>(http)127.0.0.1:8080</example>
    /// <example>(socks5)myproxy.com:1234:user:secretpassword</example>
    [Required]
    public IEnumerable<string> Proxies { get; set; } = Array.Empty<string>();
}
