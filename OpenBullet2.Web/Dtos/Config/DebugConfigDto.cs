using RuriLib.Models.Proxies;

namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// The debugger parameters.
/// </summary>
public class DebugConfigDto
{
    /// <summary>
    /// The id of the config to debug.
    /// </summary>
    public required string ConfigId { get; set; }
    
    /// <summary>
    /// The data to test the config with, defaults to an empty string.
    /// </summary>
    public string TestData { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of data to use, defaults to "Default".
    /// </summary>
    public string WordlistType { get; set; } = "Default";
    
    /// <summary>
    /// The proxy to use, if any. Defaults to null.
    /// </summary>
    public string? TestProxy { get; set; }
    
    /// <summary>
    /// The proxy type to use, defaults to HTTP.
    /// </summary>
    public ProxyType ProxyType { get; set; } = ProxyType.Http;
}
