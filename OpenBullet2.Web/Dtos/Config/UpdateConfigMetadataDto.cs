namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO that contains a config's metadata.
/// </summary>
public class UpdateConfigMetadataDto
{
    /// <summary>
    /// The id of the config.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// The name of the config.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// The category of the config.
    /// </summary>
    public string Category { get; set; } = default!;

    /// <summary>
    /// The author of the config.
    /// </summary>
    public string Author { get; set; } = default!;

    /// <summary>
    /// The image encoded as base64.
    /// </summary>
    public string Base64Image { get; set; } = default!;

    /// <summary>
    /// The date when the config was created.
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// The date when the config was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// The names of plugins required by the config.
    /// </summary>
    public List<string> Plugins { get; set; } = new();
}
