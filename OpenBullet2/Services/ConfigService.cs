using Microsoft.Scripting.Utils;
using OpenBullet2.Models.Settings;
using OpenBullet2.Repositories;
using RuriLib.Models.Configs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using RuriLib.Helpers;
using System.IO;

namespace OpenBullet2.Services
{
    public class ConfigService
    {
        public List<Config> Configs { get; set; } = new();
        public event EventHandler<Config> OnConfigSelected;
        public event EventHandler OnRemotesLoaded;

        private Config selectedConfig = null;
        private readonly IConfigRepository configRepo;
        private readonly PersistentSettingsService persistentSettings;

        public Config SelectedConfig
        {
            get => selectedConfig;
            set
            {
                selectedConfig = value;
                OnConfigSelected?.Invoke(this, selectedConfig);
            }
        }

        public ConfigService(IConfigRepository configRepo, PersistentSettingsService persistentSettings)
        {
            this.configRepo = configRepo;
            this.persistentSettings = persistentSettings;
        }

        public async Task ReloadConfigs()
        {
            // Load from the main repository
            Configs = (await configRepo.GetAll()).ToList();
            SelectedConfig = null;

            // Load from remotes (fire and forget)
            LoadFromRemotes();
        }

        private async void LoadFromRemotes()
        {
            List<Config> remoteConfigs = new();

            var func = new Func<RemoteConfigsEndpoint, Task>(async endpoint => 
            {
                try
                {
                    // Get the file
                    using HttpClient client = new();
                    client.DefaultRequestHeaders.Add("Api-Key", endpoint.ApiKey);
                    var response = await client.GetAsync(endpoint.Url);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new UnauthorizedAccessException();

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        throw new FileNotFoundException();

                    var fileStream = await response.Content.ReadAsStreamAsync();

                    // Unpack the archive in memory
                    using ZipArchive archive = new(fileStream, ZipArchiveMode.Read);
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.Name.EndsWith(".opk"))
                            continue;

                        try
                        {
                            using var entryStream = entry.Open();
                            var config = await ConfigPacker.Unpack(entryStream);
                            config.IsRemote = true;
                            remoteConfigs.Add(config);
                        }
                        catch
                        {

                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{endpoint.Url}] Failed to pull configs from endpoint: {ex.Message}");
                }
            });

            var tasks = persistentSettings.OpenBulletSettings.RemoteSettings.ConfigsEndpoints
                .Select(endpoint => func.Invoke(endpoint));

            await Task.WhenAll(tasks).ConfigureAwait(false);

            lock (Configs)
            {
                Configs.AddRange(remoteConfigs);
            }

            OnRemotesLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
