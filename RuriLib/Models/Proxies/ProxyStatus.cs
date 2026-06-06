namespace RuriLib.Models.Proxies;

/// <summary>
/// Represents the current allocation state of a proxy in the pool.
/// </summary>
public enum ProxyStatus
{
    /// <summary>
    /// The proxy is available for use.
    /// </summary>
    Available,

    /// <summary>
    /// The proxy is currently being used.
    /// </summary>
    Busy,

    /// <summary>
    /// The proxy is considered bad.
    /// </summary>
    Bad,

    /// <summary>
    /// The proxy is banned temporarily.
    /// </summary>
    Banned
}
