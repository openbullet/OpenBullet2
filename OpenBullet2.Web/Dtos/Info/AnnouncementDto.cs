namespace OpenBullet2.Web.Dtos.Info;

public class AnnouncementDto
{
    public DateTime? LastFetched { get; set; } = null;
    public string MarkdownText { get; set; } = string.Empty;
}
