using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Service that reads the announcement from the OpenBullet2 repository
/// on github.com.
/// </summary>
public class AnnouncementService : IAnnouncementService
{
    private record CachedAnnouncement(string Content, DateTime LastFetch);
    private CachedAnnouncement? cached;

    /// <inheritdoc/>
    public DateTime? LastFetched => cached?.LastFetch;

    /// <inheritdoc/>
    public async Task<string> FetchAnnouncementAsync()
    {
        var isDebug = false;

#if DEBUG
        isDebug = true;
#endif

        if (isDebug)
        {
            await Task.Delay(1);
            return "This is a **test** `message`"; // Change this to some valid markdown string to test announcements in debug mode
        }
        else
        {
            // If the cache doesn't exist or is more than 3 hours old, fetch from the web
            if (cached is null || DateTime.Now - cached.LastFetch > TimeSpan.FromHours(3))
            {
                try
                {
                    // For now we point it to the english announcement, if later
                    // on we decide to implement localization we need to change this.
                    var url = $"https://raw.githubusercontent.com/openbullet/OpenBullet2/master/Announcements/en.md";
                    using HttpClient client = new();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");
                    using var response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception();
                    }

                    cached = new(await response.Content.ReadAsStringAsync(), DateTime.Now);
                }
                catch
                {
                    return "Could not load the announcement... are you offline?";
                }
            }

            return cached.Content;
        }
    }
}
