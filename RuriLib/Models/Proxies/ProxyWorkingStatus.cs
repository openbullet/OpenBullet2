namespace RuriLib.Models.Proxies;

/// <summary>
/// Represents the latest connectivity test result for a proxy.
/// </summary>
public enum ProxyWorkingStatus
{
    /// <summary>
    /// The proxy is working.
    /// </summary>
    Working,

    /// <summary>
    /// The proxy is not working.
    /// </summary>
    NotWorking,

    /// <summary>
    /// The proxy has not been tested yet.
    /// </summary>
    Untested
}
