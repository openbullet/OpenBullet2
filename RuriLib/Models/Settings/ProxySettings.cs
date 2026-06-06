using System.Collections.Generic;

namespace RuriLib.Models.Settings;

/// <summary>
/// Stores proxy-related runtime settings.
/// </summary>
public class ProxySettings
{
    /// <summary>Gets or sets the proxy connection timeout in milliseconds.</summary>
    public int ProxyConnectTimeoutMilliseconds { get; set; } = 5000;
    /// <summary>Gets or sets the proxy read/write timeout in milliseconds.</summary>
    public int ProxyReadWriteTimeoutMilliseconds { get; set; } = 10000;
    /// <summary>Gets or sets the global ban keys.</summary>
    public List<string> GlobalBanKeys { get; set; } = [];
    /// <summary>Gets or sets the global retry keys.</summary>
    public List<string> GlobalRetryKeys { get; set; } = [];
}
