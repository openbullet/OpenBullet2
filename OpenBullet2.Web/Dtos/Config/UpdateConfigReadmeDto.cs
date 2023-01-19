namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO used to update a config's readme.
/// </summary>
public class UpdateConfigReadmeDto
{
    /// <summary>
    /// The id of the config.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// The markdown text of the readme.
    /// </summary>
    public string MarkdownText { get; set; } = default!;
}
