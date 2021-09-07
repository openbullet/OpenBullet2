using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenBullet2.Services
{
    public class AnnouncementService
    {
        private record CachedAnnouncement(string Content, DateTime LastFetch);
        private readonly Dictionary<string, CachedAnnouncement> cached = new();

        public async Task<string> FetchAnnouncement()
        {
            var isDebug = false;

#if DEBUG
            isDebug = true;
#endif
            if (isDebug)
            {

                await Task.Delay(1);
                return string.Empty; // Change this to some valid markdown string to test announcements in debug mode
            }
            else
            {
                var culture = CultureInfo.CurrentCulture.Name;

                // If the cache for the current culture doesn't exist or is more than 1 day old, fetch from the web
                if (!cached.ContainsKey(culture) || DateTime.Now - cached[culture].LastFetch > TimeSpan.FromDays(1))
                {
                    try
                    {
                        var url = $"https://raw.githubusercontent.com/openbullet/OpenBullet2/master/Announcements/{culture}.md";
                        using HttpClient client = new();
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");
                        var response = await client.GetAsync(url);

                        if (!response.IsSuccessStatusCode)
                            throw null;

                        cached[culture] = new(await response.Content.ReadAsStringAsync(), DateTime.Now);
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }

                return cached[culture].Content;
            }
        }
    }
}
