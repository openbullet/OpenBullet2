namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO used to update a config's metadata.
/// </summary>
public class UpdateConfigMetadataDto
{
    /// <summary>
    /// The name of the config.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The category of the config.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// The author of the config.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// The image encoded as base64.
    /// </summary>
    public string Base64Image { get; set; } = string.Empty;
}
