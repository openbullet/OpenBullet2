using Newtonsoft.Json;
using OpenBullet2.Models.Sharing;
using OpenBullet2.Repositories;
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
        private string BaseFolder { get; init; }
        private string EndpointsFile => Path.Combine(BaseFolder, "sharingEndpoints.json");

        public List<Endpoint> Endpoints { get; set; } = new();

        public ConfigSharingService(IConfigRepository configRepo, string baseFolder)
        {
            this.configRepo = configRepo;
            BaseFolder = baseFolder;
            Directory.CreateDirectory(baseFolder);

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

        public async Task<byte[]> GetArchive(string endpointName)
        {
            var endpoint = Endpoints.FirstOrDefault(e => e.Route.Equals(endpointName, StringComparison.OrdinalIgnoreCase));

            if (endpoint == null)
                throw new Exception("Invalid endpoint");

            using MemoryStream ms = new();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var configId in endpoint.ConfigIds)
                {
                    // Create the file entry and write the file content
                    var zipArchiveEntry = archive.CreateEntry($"{configId}.opk", CompressionLevel.Fastest);
                    var fileContent = await configRepo.GetBytes(configId);
                    using var zipStream = zipArchiveEntry.Open();
                    zipStream.Write(fileContent, 0, fileContent.Length);
                }
            }

            return ms.ToArray();
        }
    }
}
