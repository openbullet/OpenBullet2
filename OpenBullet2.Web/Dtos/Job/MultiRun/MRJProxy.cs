using RuriLib.Models.Proxies;

namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// A proxy in a multi run job.
/// </summary>
public class MrjProxy
{
    /// <summary>
    /// The type of the proxy.
    /// </summary>
    public ProxyType Type { get; set; } = ProxyType.Http;
    
    /// <summary>
    /// The host of the proxy.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// The port of the proxy.
    /// </summary>
    public int? Port { get; set; }
    
    /// <summary>
    /// The username to use for authentication.
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// The password to use for authentication.
    /// </summary>
    public string? Password { get; set; }
}
