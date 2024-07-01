using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Service that reads the announcement from the OpenBullet2 repository
/// on github.com.
/// </summary>
public class AnnouncementService : IAnnouncementService
{
    private readonly ILogger<AnnouncementService> _logger;
    private CachedAnnouncement? _cached;

    /// <summary></summary>
    public AnnouncementService(ILogger<AnnouncementService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public DateTime? LastFetched => _cached?.LastFetch;

    /// <inheritdoc />
    public async Task<string> FetchAnnouncementAsync()
    {
        var isDebug = false;

#if DEBUG
        isDebug = true;
#endif

#pragma warning disable S2583
        if (isDebug)
#pragma warning restore S2583
        {
            await Task.Delay(1);
            return
                "This is a **test** `message`"; // Change this to some valid markdown string to test announcements in debug mode
        }

        // If the cache doesn't exist or is more than 3 hours old, fetch from the web
        if (_cached is null || DateTime.UtcNow > _cached.LastFetch + TimeSpan.FromHours(3))
        {
            try
            {
                // For now, we point it to the english announcement, if later
                // on we decide to implement localization we need to change this.
#pragma warning disable S1075
                var url = "https://raw.githubusercontent.com/openbullet/OpenBullet2/master/Announcements/en.md";
#pragma warning restore S1075
                using HttpClient client = new();
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");
                using var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Could not load the announcement: {StatusCode}", response.StatusCode);
                    return "Could not load the announcement... are you offline?";
                }

                _cached = new CachedAnnouncement(await response.Content.ReadAsStringAsync(), DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load the announcement");
                return "Could not load the announcement... are you offline?";
            }
        }

        return _cached.Content;
    }

    private sealed record CachedAnnouncement(string Content, DateTime LastFetch);
}
