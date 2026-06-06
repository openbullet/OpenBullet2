using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Scripting.Utils;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Repositories;
using RuriLib.Models.Configs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using RuriLib.Helpers;
using System.IO;
using RuriLib.Functions.Conversion;
using System.Threading;

namespace OpenBullet2.Core.Services;

// TODO: The config service should also be in charge of calling methods of the IConfigRepository
/// <summary>
/// Manages the list of available configs.
/// </summary>
public class ConfigService
{
    private readonly object configsLock = new();
    private int reloadVersion;
    private readonly ILogger<ConfigService> logger;

    /// <summary>
    /// The list of available configs.
    /// </summary>
    public List<Config> Configs { get; set; } = [];

    /// <summary>
    /// Called when a new config is selected.
    /// </summary>
    public event EventHandler<Config>? OnConfigSelected;

    /// <summary>
    /// Called when all configs from configured remote endpoints are loaded.
    /// </summary>
    public event EventHandler? OnRemotesLoaded;

    private Config selectedConfig = null!;
    private readonly IConfigRepository configRepo;
    private readonly OpenBulletSettingsService openBulletSettingsService;

    public ConfigService(
        IConfigRepository configRepo,
        OpenBulletSettingsService openBulletSettingsService,
        ILogger<ConfigService>? logger = null)
    {
        this.configRepo = configRepo;
        this.openBulletSettingsService = openBulletSettingsService;
        this.logger = logger ?? NullLogger<ConfigService>.Instance;
    }

    /// <summary>
    /// The currently selected config.
    /// </summary>
    public Config SelectedConfig
    {
        get => selectedConfig;
        set
        {
            selectedConfig = value;
            OnConfigSelected?.Invoke(this, selectedConfig);
        }
    }

    /// <summary>
    /// Reloads all configs from the <see cref="IConfigRepository"/> and remote endpoints.
    /// </summary>
    public async Task ReloadConfigsAsync()
        => await ReloadConfigsAsync(CancellationToken.None);

    /// <summary>
    /// Reloads all configs from the <see cref="IConfigRepository"/> and remote endpoints.
    /// </summary>
    public async Task ReloadConfigsAsync(CancellationToken cancellationToken)
    {
        var currentReloadVersion = Interlocked.Increment(ref reloadVersion);
        logger.LogDebug("Reloading configs using reload version {ReloadVersion}", currentReloadVersion);

        // Load from the main repository
        var localConfigs = (await configRepo.GetAllAsync()).ToList();

        if (!TryPublishLocalConfigs(localConfigs, currentReloadVersion))
        {
            return;
        }

        // Load from remotes (fire and forget)
        _ = LoadFromRemotesAsync(currentReloadVersion, cancellationToken);
    }

    private async Task LoadFromRemotesAsync(int currentReloadVersion, CancellationToken cancellationToken)
    {
        List<Config> remoteConfigs = [];

        var func = new Func<RemoteConfigsEndpoint, Task>(async endpoint =>
        {
            try
            {
                // Get the file
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("Api-Key", endpoint.ApiKey);
                using var response = await client.GetAsync(endpoint.Url, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new FileNotFoundException();
                }

                var fileStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                // Unpack the archive in memory
                await using ZipArchive archive = new(fileStream, ZipArchiveMode.Read);
                foreach (var entry in archive.Entries)
                {
                    if (!entry.Name.EndsWith(".opk"))
                    {
                        continue;
                    }

                    try
                    {
                        await using var entryStream = await entry.OpenAsync(cancellationToken);
                        var config = await ConfigPacker.UnpackAsync(entryStream);

                        // Calculate the hash of the metadata of the remote config to use as id.
                        // This is done to have a consistent id through successive pulls of configs
                        // from remotes, so that jobs can reference the id and retrieve the correct one
                        config.Id = HexConverter.ToHexString(config.Metadata.GetUniqueHash());
                        config.IsRemote = true;

                        // If a config with the same hash is not already present (e.g. same exact config
                        // from another source) add it to the list
                        if (!remoteConfigs.Any(c => c.Id == config.Id))
                        {
                            remoteConfigs.Add(config);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug(ex, "Skipped invalid remote config entry {EntryName} from endpoint {EndpointUrl}",
                            entry.Name, endpoint.Url);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to pull configs from remote endpoint {EndpointUrl}", endpoint.Url);
            }
        });

        var tasks = openBulletSettingsService.Settings.RemoteSettings.ConfigsEndpoints
            .Select(endpoint => func.Invoke(endpoint));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        if (!TryPublishRemoteConfigs(remoteConfigs, currentReloadVersion))
        {
            return;
        }

        logger.LogDebug("Loaded {RemoteConfigCount} remote config(s) for reload version {ReloadVersion}",
            remoteConfigs.Count, currentReloadVersion);
        OnRemotesLoaded?.Invoke(this, EventArgs.Empty);
    }

    private bool TryPublishLocalConfigs(List<Config> localConfigs, int currentReloadVersion)
    {
        lock (configsLock)
        {
            if (!IsLatestReload(currentReloadVersion))
            {
                return false;
            }

            Configs = localConfigs;
            selectedConfig = null!;
        }

        logger.LogDebug("Published {LocalConfigCount} local config(s) for reload version {ReloadVersion}",
            localConfigs.Count, currentReloadVersion);
        OnConfigSelected?.Invoke(this, selectedConfig);
        return true;
    }

    private bool TryPublishRemoteConfigs(List<Config> remoteConfigs, int currentReloadVersion)
    {
        lock (configsLock)
        {
            if (!IsLatestReload(currentReloadVersion))
            {
                return false;
            }

            var existingIds = Configs.Select(c => c.Id).ToHashSet();
            foreach (var config in remoteConfigs)
            {
                if (existingIds.Add(config.Id))
                {
                    Configs.Add(config);
                }
            }
        }

        return true;
    }

    private bool IsLatestReload(int currentReloadVersion)
        => Volatile.Read(ref reloadVersion) == currentReloadVersion;

    /// <summary>
    /// Saves a config through the configured repository.
    /// </summary>
    public Task SaveAsync(Config config)
        => configRepo.SaveAsync(config);

    /// <summary>
    /// Saves the currently selected config.
    /// </summary>
    public Task SaveSelectedConfigAsync()
    {
        if (selectedConfig is null)
        {
            throw new InvalidOperationException("No config selected");
        }

        return configRepo.SaveAsync(selectedConfig);
    }
}
