using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenBullet2.Native.Services;

public class AnnouncementService
{
    private readonly ILogger<AnnouncementService> _logger;
    private record CachedAnnouncement(string Content, DateTime LastFetch);
    private CachedAnnouncement? cached;

    public AnnouncementService(ILogger<AnnouncementService> logger)
    {
        _logger = logger;
    }

    public async Task<string> FetchAnnouncementAsync()
    {
        var isDebug = false;

#if DEBUG
        isDebug = true;
#endif

        if (isDebug)
        {
            _logger.LogDebug("Skipped announcement fetch in debug mode");
            await Task.Delay(1);
            return "No announcement shown in DEBUG mode"; // Change this to some valid markdown string to test announcements in debug mode
        }

        // If the cache for the current culture doesn't exist or is more than 1 day old, fetch from the web
        if (cached is null || DateTime.Now - cached.LastFetch > TimeSpan.FromDays(1))
        {
            try
            {
                var url = "https://raw.githubusercontent.com/openbullet/OpenBullet2/master/Announcements/native.md";
                using HttpClient client = new();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Announcement fetch failed with status code {StatusCode}", response.StatusCode);
                    return "Could not load the announcement... are you offline?";
                }

                cached = new(await response.Content.ReadAsStringAsync(), DateTime.Now);
                _logger.LogDebug("Fetched Native announcement at {FetchedAt}", cached.LastFetch);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load the Native announcement");
                return "Could not load the announcement... are you offline?";
            }
        }

        return cached?.Content ?? "Could not load the announcement... are you offline?";
    }
}
