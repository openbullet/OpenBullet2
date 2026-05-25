using Newtonsoft.Json.Linq;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
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
    private readonly OpenBulletSettingsService _settingsService;
    private readonly string versionFile = "version.txt";
    private readonly Timer timer;
    private Version latestReleaseVersion = new(0, 3, 3);
    private Version latestStagingVersion = new(0, 3, 3);

    public Version CurrentVersion { get; private set; } = new(0, 3, 3);
    public Version RemoteVersion => UpdateChannel switch
    {
        UpdateChannel.Staging => latestStagingVersion,
        UpdateChannel.Release => latestReleaseVersion,
        _ => CurrentVersion
    };
    public bool IsUpdateAvailable => UpdateChannel != UpdateChannel.Disabled
        && RemoteVersion > CurrentVersion;
    public string CurrentVersionType => CurrentVersion.Major == 0
        ? (CurrentVersion.Minor == 0 ? "Alpha" : "Beta")
        : "Release";
    public UpdateChannel UpdateChannel => _settingsService.Settings.GeneralSettings.UpdateChannel;

    public event Action? UpdateAvailable;

    public UpdateService(ILogger<UpdateService> logger, OpenBulletSettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;

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

        latestReleaseVersion = CurrentVersion;
        latestStagingVersion = CurrentVersion;

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

        if (UpdateChannel == UpdateChannel.Disabled)
        {
            _logger.LogDebug("Skipped update check because update alerts are disabled");
            return;
        }

        try
        {
            // Query the GitHub API to get the available releases.
            using HttpClient client = new();
            client.BaseAddress = new Uri("https://api.github.com/repos/openbullet/OpenBullet2/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");
            var response = await client.GetAsync("releases");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var releases = JArray.Parse(json)
                .Select(release => new
                {
                    Version = Version.Parse(release["tag_name"]?.ToString()
                        ?? throw new InvalidOperationException("GitHub release payload did not contain tag_name")),
                    IsPrerelease = release["prerelease"]?.ToObject<bool>() ?? false
                })
                .ToList();

            latestStagingVersion = releases.Count > 0
                ? releases.MaxBy(release => release.Version)!.Version
                : CurrentVersion;

            latestReleaseVersion = releases
                .Where(release => !release.IsPrerelease)
                .MaxBy(release => release.Version)?.Version ?? CurrentVersion;

            if (IsUpdateAvailable)
            {
                _logger.LogInformation("A new update is available for the Native client on the {Channel} channel: {RemoteVersion}",
                    UpdateChannel, RemoteVersion);
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
