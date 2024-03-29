namespace OpenBullet2.Web.Dtos.Proxy;

/// <summary>
/// The field to sort proxies by.
/// </summary>
public enum ProxySortField
{
    /// <summary>
    /// The proxy's host.
    /// </summary>
    Host,
    
    /// <summary>
    /// The proxy's port.
    /// </summary>
    Port,
    
    /// <summary>
    /// The proxy's username.
    /// </summary>
    Username,
    
    /// <summary>
    /// The proxy's password.
    /// </summary>
    Password,
    
    /// <summary>
    /// The proxy country.
    /// </summary>
    Country,
    
    /// <summary>
    /// The proxy's ping.
    /// </summary>
    Ping,
    
    /// <summary>
    /// When the proxy was last checked.
    /// </summary>
    LastChecked
}
