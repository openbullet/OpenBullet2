using Newtonsoft.Json.Linq;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Services;

/// <summary>
/// The update service as a background service.
/// </summary>
public class UpdateService : BackgroundService, IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private readonly OpenBulletSettingsService _settingsService;
    private readonly string _versionFile = "version.txt";
    private Version _latestReleaseVersion;
    private Version _latestStagingVersion;

    /// <summary></summary>
    public UpdateService(ILogger<UpdateService> logger, OpenBulletSettingsService settingsService)
    {
        // Try to read the current version from disk
        try
        {
            var content = File.ReadLines(_versionFile).First();
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
            File.WriteAllText(_versionFile, CurrentVersion.ToString());
        }

        _settingsService = settingsService;
        _logger = logger;
        _latestReleaseVersion = CurrentVersion;
        _latestStagingVersion = CurrentVersion;
    }

    /// <inheritdoc />
    public Version CurrentVersion { get; } = new(0, 3, 3);

    /// <inheritdoc />
    public Version RemoteVersion => SelectedChannel switch
    {
        UpdateChannel.Staging => _latestStagingVersion,
        UpdateChannel.Release => _latestReleaseVersion,
        _ => CurrentVersion
    };

    /// <inheritdoc />
    public bool IsUpdateAvailable => SelectedChannel != UpdateChannel.Disabled
        && RemoteVersion > CurrentVersion;

    /// <inheritdoc />
    public VersionType CurrentVersionType => GetVersionType(CurrentVersion);

    /// <inheritdoc />
    public VersionType RemoteVersionType => GetVersionType(RemoteVersion);

    /// <inheritdoc />
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Check for updates every 3 hours
        using var timer = new PeriodicTimer(TimeSpan.FromHours(3));

        do
        {
            await FetchRemoteVersionAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private static VersionType GetVersionType(Version version) => version switch
    {
        { Major: 0, Minor: 0 } => VersionType.Alpha,
        { Major: 0 } => VersionType.Beta,
        _ => VersionType.Release
    };

    private UpdateChannel SelectedChannel => _settingsService.Settings.GeneralSettings.UpdateChannel;

    private async Task FetchRemoteVersionAsync(CancellationToken cancellationToken)
    {
        var isDebug = false;

#if DEBUG
        isDebug = true;
#endif

#pragma warning disable S2583
        if (isDebug)
#pragma warning restore S2583
        {
            _logger.LogWarning("Skipped updates check in debug mode");
            await Task.Delay(1, cancellationToken);
            return;
        }

        if (SelectedChannel == UpdateChannel.Disabled)
        {
            _logger.LogDebug("Skipped update check because update alerts are disabled");
            return;
        }

        try
        {
            // Query the GitHub API to get the available releases.
            using HttpClient client = new();
#pragma warning disable S1075
            client.BaseAddress = new Uri("https://api.github.com/repos/openbullet/OpenBullet2/");
#pragma warning restore S1075
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");
            var response = await client.GetAsync("releases", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var releases = JArray.Parse(json)
                .Select(release => new
                {
                    Version = Version.Parse(release["tag_name"]?.ToString() ?? "0.0.1"),
                    IsPrerelease = release["prerelease"]?.ToObject<bool>() ?? false
                })
                .ToList();

            _latestStagingVersion = releases.Count > 0
                ? releases.MaxBy(release => release.Version)!.Version
                : CurrentVersion;

            _latestReleaseVersion = releases
                .Where(release => !release.IsPrerelease)
                .MaxBy(release => release.Version)?.Version ?? CurrentVersion;

            if (IsUpdateAvailable)
            {
                _logger.LogInformation("There is a new update on the {Channel} channel! Version {Version}",
                    SelectedChannel, RemoteVersion);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check for updates. I will retry in 3 hours.");
        }
    }
}
