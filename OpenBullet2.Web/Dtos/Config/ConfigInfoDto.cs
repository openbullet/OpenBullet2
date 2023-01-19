namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO that contains overview information about a config.
/// </summary>
public class ConfigInfoDto
{
    /// <summary>
    /// The unique id of the config.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// The name of the config.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// The author of the config.
    /// </summary>
    public string Author { get; set; } = default!;

    /// <summary>
    /// The category of the config.
    /// </summary>
    public string Category { get; set; } = default!;

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
    public DateTime LastModified { get; set; } = default!;


}
