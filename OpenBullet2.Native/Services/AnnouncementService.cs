using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenBullet2.Native.Services
{
    public class AnnouncementService
    {
        private record CachedAnnouncement(string Content, DateTime LastFetch);
        private CachedAnnouncement cached = null;

        public async Task<string> FetchAnnouncement()
        {
#if DEBUG
            await Task.Delay(1);
            return "No announcement shown in DEBUG mode"; // Change this to some valid markdown string to test announcements in debug mode
#else
            // If the cache for the current culture doesn't exist or is more than 1 day old, fetch from the web
            if (cached is null || DateTime.Now - cached.LastFetch > TimeSpan.FromDays(1))
            {
                try
                {
                    var url = $"https://raw.githubusercontent.com/openbullet/OpenBullet2/master/Announcements/native.md";
                    using HttpClient client = new();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");
                    var response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                        throw null;

                    cached = new(await response.Content.ReadAsStringAsync(), DateTime.Now);
                }
                catch
                {
                    return string.Empty;
                }
            }

            return cached.Content;
#endif
        }
    }
}
