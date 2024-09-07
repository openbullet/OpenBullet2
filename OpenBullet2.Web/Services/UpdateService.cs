using Newtonsoft.Json.Linq;
using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Services;

/// <summary>
/// The update service as a background service.
/// </summary>
public class UpdateService : BackgroundService, IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private readonly string _versionFile = "version.txt";

    /// <summary></summary>
    public UpdateService(ILogger<UpdateService> logger)
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

        _logger = logger;
    }

    /// <inheritdoc />
    public Version CurrentVersion { get; } = new(0, 3, 2);

    /// <inheritdoc />
    public Version RemoteVersion { get; private set; } = new(0, 3, 2);

    /// <inheritdoc />
    public bool IsUpdateAvailable => RemoteVersion > CurrentVersion;

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
            await FetchRemoteVersionAsync();
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private static VersionType GetVersionType(Version version) => version switch {
        { Major: 0, Minor: 0 } => VersionType.Alpha,
        { Major: 0 } => VersionType.Beta,
        _ => VersionType.Release
    };

    private async Task FetchRemoteVersionAsync()
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
            await Task.Delay(1);
            return;
        }

        try
        {
            // Query the GitHub api to get a list of the latest releases
            using HttpClient client = new();
#pragma warning disable S1075
            client.BaseAddress = new Uri("https://api.github.com/repos/openbullet/OpenBullet2/");
#pragma warning restore S1075
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");
            var response = await client.GetAsync("releases/latest");

            // Take the first and get its name
            var json = await response.Content.ReadAsStringAsync();
            var release = JToken.Parse(json);
            var releaseName = release["tag_name"]?.ToString() ?? "0.0.1";

            // Try to parse that name to a Version object
            RemoteVersion = Version.Parse(releaseName);

            if (IsUpdateAvailable)
            {
                _logger.LogInformation("There is a new update! Version {Version}", RemoteVersion);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check for updates. I will retry in 3 hours.");
        }
    }
}
