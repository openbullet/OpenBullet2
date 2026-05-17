using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Native.Services;

public class UpdateService : IDisposable
{
    private readonly ILogger<UpdateService> _logger;
    private readonly string versionFile = "version.txt";
    private readonly Timer timer;

    public Version CurrentVersion { get; private set; } = new(0, 3, 3);
    public Version RemoteVersion { get; private set; } = new(0, 3, 3);
    public bool IsUpdateAvailable => RemoteVersion > CurrentVersion;
    public string CurrentVersionType => CurrentVersion.Major == 0
        ? (CurrentVersion.Minor == 0 ? "Alpha" : "Beta")
        : "Release";

    public event Action? UpdateAvailable;

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;

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
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Could not read the current version from {VersionFile}, recreating it with {CurrentVersion}",
                versionFile, CurrentVersion);
            File.WriteAllText(versionFile, CurrentVersion.ToString());
        }

        // Check for updates once a day
        timer = new Timer(new TimerCallback(async _ => await FetchRemoteVersionAsync()),
                null, 0, (int)TimeSpan.FromDays(1).TotalMilliseconds);
        _logger.LogDebug("Initialized update service for Native client, current version is {CurrentVersion}",
            CurrentVersion);
    }

    private async Task FetchRemoteVersionAsync()
    {
        var isDebug = false;

#if DEBUG
        isDebug = true;
#endif

        if (isDebug)
        {
            _logger.LogDebug("Skipped update check in debug mode");
            await Task.Delay(1);
            return;
        }

        try
        {
            // Query the github api to get a list of the latest releases
            using HttpClient client = new();
            client.BaseAddress = new Uri("https://api.github.com/repos/openbullet/OpenBullet2/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");
            var response = await client.GetAsync("releases/latest");
            response.EnsureSuccessStatusCode();

            // Take the first and get its name
            var json = await response.Content.ReadAsStringAsync();
            var release = JToken.Parse(json);
            var releaseName = release["tag_name"]?.ToString()
                ?? throw new InvalidOperationException("GitHub release payload did not contain tag_name");

            // Try to parse that name to a Version object
            RemoteVersion = Version.Parse(releaseName);

            if (IsUpdateAvailable)
            {
                _logger.LogInformation("A new update is available for Native client: {RemoteVersion}", RemoteVersion);
                UpdateAvailable?.Invoke();
            }
            else
            {
                _logger.LogDebug("Native client is up to date at version {CurrentVersion}", CurrentVersion);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check for updates. I will retry in 1 day.");
        }
    }

    public void Dispose() => timer?.Dispose();
}
