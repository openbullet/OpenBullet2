using RuriLib.Models.Configs;

namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO that contains overview information about a config.
/// </summary>
public class ConfigInfoDto
{
    /// <summary>
    /// The unique id of the config.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The name of the config.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The base64 encoded image of the config.
    /// </summary>
    public string Base64Image { get; set; } = string.Empty;

    /// <summary>
    /// The author of the config.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// The category of the config.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Whether the config was obtained from a remote source or locally.
    /// </summary>
    public bool IsRemote { get; set; }

    /// <summary>
    /// Whether the config needs proxies.
    /// </summary>
    public bool NeedsProxies { get; set; }

    /// <summary>
    /// The wordlist types that are allowed by this wordlist.
    /// </summary>
    public List<string> AllowedWordlistTypes { get; set; } = new();

    /// <summary>
    /// The date when the config was created.
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// The date when the config was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// The current config mode.
    /// </summary>
    public ConfigMode Mode { get; set; } = ConfigMode.LoliCode;

    /// <summary>
    /// Whether the config is dangerous and might contain malicious code.
    /// </summary>
    public bool Dangerous { get; set; }
    
    /// <summary>
    /// The number of bots that the config suggests to use.
    /// </summary>
    public int SuggestedBots { get; set; }
}
