namespace OpenBullet2.Web.Interfaces;

public interface IAnnouncementService
{
    DateTime? LastFetched { get; }
    Task<string> FetchAnnouncement();
}
