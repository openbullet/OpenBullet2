namespace OpenBullet2.Web.Dtos.Info;

/// <summary>
/// DTO for the bundled software changelog.
/// </summary>
public class ChangelogDto
{
    /// <summary>
    /// The markdown text of the changelog.
    /// </summary>
    public string MarkdownText { get; set; } = string.Empty;
}
