using RuriLib.Models.Proxies;

namespace OpenBullet2.Web.Dtos.Job.ProxyCheck;

/// <summary>
/// Message sent when a new result is generated.
/// </summary>
public class PcjNewResultMessage
{
    /// <summary>
    /// The host of the proxy.
    /// </summary>
    public string ProxyHost { get; set; } = string.Empty;

    /// <summary>
    /// The port of the proxy.
    /// </summary>
    public int ProxyPort { get; set; }

    /// <summary>
    /// The working status of the proxy.
    /// </summary>
    public ProxyWorkingStatus WorkingStatus { get; set; }

    /// <summary>
    /// The ping of the proxy.
    /// </summary>
    public int Ping { get; set; } = 0;

    /// <summary>
    /// The detected country of the proxy, if any.
    /// </summary>
    public string? Country { get; set; } = null;
}
