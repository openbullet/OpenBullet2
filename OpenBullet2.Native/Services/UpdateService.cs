using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Native.Services
{
    public class UpdateService : IDisposable
    {
        private readonly string versionFile = "version.txt";
        private readonly Timer timer;

        public Version CurrentVersion { get; private set; } = new(0, 2, 4);
        public Version RemoteVersion { get; private set; } = new(0, 2, 4);
        public bool IsUpdateAvailable => RemoteVersion > CurrentVersion;
        public string CurrentVersionType => CurrentVersion.Major == 0
            ? (CurrentVersion.Minor == 0 ? "Alpha" : "Beta")
            : "Release";

        public event Action UpdateAvailable;

        public UpdateService()
        {
            // Try to read the current version from disk
            try
            {
                var content = File.ReadLines(versionFile).First();
                var version = Version.Parse(content);

                // If higher than the minimum expected current version, set it
                if (version > CurrentVersion)
                {
                    CurrentVersion = version;
                }
            }
            // If there is no file or the version number is invalid
            catch
            {
                File.WriteAllText(versionFile, CurrentVersion.ToString());
            }

            // Check for updates once a day
            timer = new Timer(new TimerCallback(async _ => await FetchRemoteVersion()),
                    null, 0, (int)TimeSpan.FromDays(1).TotalMilliseconds);
        }

        private async Task FetchRemoteVersion()
        {
            var isDebug = false;

#if DEBUG
            isDebug = true;
#endif

            if (isDebug)
            {
                Console.WriteLine("Skipped updates check in debug mode");
                await Task.Delay(1);
                return;
            }
            else
            {
                try
                {
                    // Query the github api to get a list of the latest releases
                    using HttpClient client = new();
                    client.BaseAddress = new Uri("https://api.github.com/repos/openbullet/OpenBullet2/");
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");
                    var response = await client.GetAsync("releases/latest");

                    // Take the first and get its name
                    var json = await response.Content.ReadAsStringAsync();
                    var release = JToken.Parse(json);
                    var releaseName = release["name"].ToString();

                    // Try to parse that name to a Version object
                    RemoteVersion = Version.Parse(releaseName);

                    if (IsUpdateAvailable)
                    {
                        UpdateAvailable?.Invoke();
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to check for updates. I will retry in 1 day.");
                }
            }
        }

        public void Dispose() => timer?.Dispose();
    }
}
