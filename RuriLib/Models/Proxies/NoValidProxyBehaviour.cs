namespace RuriLib.Models.Proxies;

/// <summary>
/// Defines how the proxy pool should react when no valid proxy is available.
/// </summary>
public enum NoValidProxyBehaviour
{
    /// <summary>
    /// Do nothing and leave the pool unchanged.
    /// </summary>
    DoNothing,

    /// <summary>
    /// Unban existing proxies and try again.
    /// </summary>
    Unban,

    /// <summary>
    /// Reload proxies from the configured sources.
    /// </summary>
    Reload
}
