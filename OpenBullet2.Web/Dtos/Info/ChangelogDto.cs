namespace OpenBullet2.Web.Dtos.Info;

/// <summary>
/// DTO for the changelog of a given version of the software.
/// </summary>
public class ChangelogDto
{
    /// <summary>
    /// The version that the changelog refers to.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The markdown text of the changelog.
    /// </summary>
    public string MarkdownText { get; set; } = string.Empty;
}
