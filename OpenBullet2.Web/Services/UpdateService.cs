using Newtonsoft.Json.Linq;
using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Services;

public class UpdateService : BackgroundService, IUpdateService
{
    private readonly string _versionFile = "version.txt";
    private readonly ILogger<UpdateService> _logger;

    public Version CurrentVersion { get; private set; } = new(0, 2, 5);
    public Version RemoteVersion { get; private set; } = new(0, 2, 5);
    public bool IsUpdateAvailable => RemoteVersion > CurrentVersion;
    public VersionType CurrentVersionType => CurrentVersion switch
    {
        { Major: 0, Minor: 0 } => VersionType.Alpha,
        { Major: 0 } => VersionType.Beta,
        _ => VersionType.Release
    };

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

    // Check for updates every 3 hours
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(3));

        do
        {
            await FetchRemoteVersion();
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task FetchRemoteVersion()
    {
        var isDebug = false;

#if DEBUG
        isDebug = true;
#endif

        if (isDebug)
        {
            _logger.LogWarning("Skipped updates check in debug mode");
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
                var releaseName = release["name"]?.ToString() ?? "0.0.1";

                // Try to parse that name to a Version object
                RemoteVersion = Version.Parse(releaseName);

                if (IsUpdateAvailable)
                {
                    _logger.LogInformation($"There is a new update! Version {RemoteVersion}");
                }
            }
            catch
            {
                _logger.LogWarning("Failed to check for updates. I will retry in 3 hours.");
            }
        }
    }
}
