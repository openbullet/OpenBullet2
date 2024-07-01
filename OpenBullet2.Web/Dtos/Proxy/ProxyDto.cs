using RuriLib.Models.Proxies;

namespace OpenBullet2.Web.Dtos.Proxy;

/// <summary>
/// DTO that contains information about a proxy.
/// </summary>
public class ProxyDto
{
    /// <summary>
    /// The id of the proxy.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The host on which the proxy server is running.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// The port on which the proxy server is listening.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// The protocol used by the proxy server to open a proxy tunnel.
    /// </summary>
    public ProxyType Type { get; set; }

    /// <summary>
    /// The username, if required by the proxy server.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The password, if required by the proxy server.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// The country of the proxy, detected after checking it with a geolocalization service.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// The working status of the proxy.
    /// </summary>
    public ProxyWorkingStatus Status { get; set; }

    /// <summary>
    /// The ping of the proxy in milliseconds.
    /// </summary>
    public int Ping { get; set; }

    /// <summary>
    /// The last time the proxy was checked, if it was checked at all.
    /// </summary>
    public DateTime? LastChecked { get; set; }

    /// <summary>
    /// The id of the proxy group to which the proxy belongs to.
    /// </summary>
    public int GroupId { get; set; }
    
    /// <summary>
    /// The name of the proxy group to which the proxy belongs to.
    /// </summary>
    public required string GroupName { get; set; }
}
