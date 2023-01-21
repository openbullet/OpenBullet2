namespace OpenBullet2.Web.Dtos.Info;

/// <summary>
/// DTO for the current announcement.
/// </summary>
public class AnnouncementDto
{
    /// <summary>
    /// When the announcement was last fetched from the remote source.
    /// If null, the fetch failed or did not happen yet.
    /// </summary>
    public DateTime? LastFetched { get; set; } = null;

    /// <summary>
    /// The markdown text of the announcement.
    /// </summary>
    public string MarkdownText { get; set; } = string.Empty;
}
