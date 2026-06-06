namespace RuriLib.Models.Proxies;

/// <summary>
/// Describes the anonymity quality reported by a proxy judge.
/// </summary>
public enum ProxyQuality
{
    /// <summary>
    /// The proxy quality could not be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The proxy leaks the client IP through forwarding headers.
    /// </summary>
    Transparent = 1,

    /// <summary>
    /// The proxy hides the client IP but still identifies itself as a proxy.
    /// </summary>
    Anonymous = 2,

    /// <summary>
    /// The proxy hides both the client IP and common proxy markers.
    /// </summary>
    Elite = 3
}
