using Newtonsoft.Json;
using OpenBullet2.Core.Models.Sharing;
using OpenBullet2.Core.Repositories;
using RuriLib.Helpers;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Services
{
    public class ConfigSharingService
    {
        private readonly IConfigRepository configRepo;
        private readonly JsonSerializerSettings jsonSettings;
        private string SettingsFolder { get; init; }
        private string EndpointsFile => Path.Combine(SettingsFolder, "sharingEndpoints.json");

        public List<Endpoint> Endpoints { get; set; } = new();

        public ConfigSharingService(IConfigRepository configRepo, string settingsFolder)
        {
            this.configRepo = configRepo;
            SettingsFolder = settingsFolder;
            Directory.CreateDirectory(settingsFolder);

            jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };

            if (File.Exists(EndpointsFile))
            {
                Endpoints = JsonConvert.DeserializeObject<List<Endpoint>>(File.ReadAllText(EndpointsFile), jsonSettings);
            }
        }

        public void Save()
        {
            File.WriteAllText(EndpointsFile, JsonConvert.SerializeObject(Endpoints, jsonSettings));
        }

        public Endpoint GetEndpoint(string endpointName)
            => Endpoints.FirstOrDefault(e => e.Route.Equals(endpointName, StringComparison.OrdinalIgnoreCase));

        public async Task<byte[]> GetArchive(string endpointName)
        {
            var endpoint = GetEndpoint(endpointName);

            if (endpoint == null)
                throw new Exception("Invalid endpoint");

            using MemoryStream ms = new();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var configId in endpoint.ConfigIds)
                {
                    try
                    {
                        // Get the config from the repo, randomize the id, convert it to C# and repack it into bytes
                        var config = await configRepo.Get(configId);
                        config.ChangeMode(ConfigMode.CSharp);
                        config.Id = Guid.NewGuid().ToString();
                        var bytes = await ConfigPacker.Pack(config);

                        // Create the entry and write the data
                        var zipArchiveEntry = archive.CreateEntry($"{configId}.opk", CompressionLevel.Fastest);
                        using var zipStream = zipArchiveEntry.Open();
                        zipStream.Write(bytes, 0, bytes.Length);
                    }
                    catch (Exception ex)
                    {
                        // If something happens, simply log it and omit the config from the archive
                        Console.WriteLine($"Error while packing config {configId} for endpoint {endpoint.Route}: {ex.Message}");
                    }
                }
            }

            return ms.ToArray();
        }
    }
}
