namespace OpenBullet2.Web.Interfaces;

/// <summary>
/// Service that reads the announcement.
/// </summary>
public interface IAnnouncementService
{
    /// <summary>
    /// When the announcement was last fetched from the remote source.
    /// If null, the fetch failed or did not happen yet.
    /// </summary>
    DateTime? LastFetched { get; }

    /// <summary>
    /// Fetches the markdown text of the announcement and caches it.
    /// </summary>
    Task<string> FetchAnnouncementAsync();
}
