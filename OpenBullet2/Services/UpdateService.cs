using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Services
{
    public class UpdateService : IDisposable
    {
        private readonly string versionFile = "version.txt";
        private readonly Timer timer;
        
        public Version CurrentVersion { get; private set; } = new Version(0, 0, 1);
        public Version RemoteVersion { get; private set; } = new Version(0, 0, 1);
        public bool IsUpdateAvailable => RemoteVersion > CurrentVersion;
        public string CurrentVersionType => CurrentVersion.Major == 0
            ? (CurrentVersion.Minor == 0 ? "Alpha" : "Beta")
            : "Release";

        public UpdateService()
        {
            // Try to read the current version from disk
            try
            {
                var content = File.ReadLines(versionFile).First();
                CurrentVersion = Version.Parse(versionFile);
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
            try
            {
                // Query the github api to get a list of the latest releases
                var url = $"https://api.github.com/repos/openbullet/OpenBullet2/releases";
                using HttpClient client = new();
                var response = await client.GetAsync(url);

                // Take the first and get its name
                var json = await response.Content.ReadAsStringAsync();
                var doc = JArray.Parse(json);
                var releaseName = doc.Children().First()["name"].ToString();

                // Try to parse that name to a Version object
                RemoteVersion = Version.Parse(releaseName);
            }
            catch
            {
                Console.WriteLine("Failed to check for updates. I will retry in 1 day.");
            }
        }

        public void Dispose() => timer?.Dispose();
    }
}
