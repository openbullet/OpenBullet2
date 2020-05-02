using Newtonsoft.Json;
using OpenBullet2.Models;
using OpenBullet2.Models.Configs;
using RuriLib.Models.Configs;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public class DiskConfigRepository : IConfigRepository
    {
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings 
        { 
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        public DiskConfigRepository()
        {
            Directory.CreateDirectory("Configs");
        }

        public async Task<List<Config>> GetAll()
        {
            List<Config> configs = new List<Config>();

            // TODO: Parallelize this for max performance
            foreach (var file in Directory.GetFiles("Configs").Where(file => file.EndsWith(".opk")))
            {
                var config = await Get(Path.GetFileNameWithoutExtension(file));
                configs.Add(config);
            }

            return configs;
        }

        public async Task<Config> Get(string id)
        {
            var file = GetFileName(id);

            if (File.Exists(file))
            {
                using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

                var config = new Config { Id = id };
                return await UnpackConfig(config, fileStream);
            }

            throw new FileNotFoundException();
        }

        public async Task<Config> Create()
        {
            var config = new Config();
            await Save(config);
            return config;
        }

        public async Task Upload(Stream stream)
        {
            var config = new Config();
            config = await UnpackConfig(config, stream);
            await File.WriteAllBytesAsync(GetFileName(config), await PackConfig(config));
        }

        public async Task Save(Config config)
        {
            await File.WriteAllBytesAsync(GetFileName(config), await PackConfig(config));
        }

        public void Delete(Config config)
        {
            var file = GetFileName(config);

            if (File.Exists(file))
                File.Delete(file);
        }

        private async Task<byte[]> PackConfig(Config config)
        {
            using var packageStream = new MemoryStream();
            using (var archive = new ZipArchive(packageStream, ZipArchiveMode.Create, false))
            {
                await CreateZipEntryFromString(archive, "readme.md", config.Readme);
                await CreateZipEntryFromString(archive, "metadata.json", JsonConvert.SerializeObject(config.Metadata));
                await CreateZipEntryFromString(archive, "settings.json", JsonConvert.SerializeObject(config.Settings));

                if (config.CSharpMode)
                {
                    await CreateZipEntryFromString(archive, "script.cs", config.CSharpScript);
                }
                else
                {
                    await CreateZipEntryFromString(archive, "blocks.json", JsonConvert.SerializeObject(config.Blocks, jsonSettings));
                }
            }

            return packageStream.ToArray();
        }

        private async Task<Config> UnpackConfig(Config config, Stream stream)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                config.Readme = ReadStringFromZipEntry(archive, "readme.md");
                config.Metadata = JsonConvert.DeserializeObject<ConfigMetadata>(ReadStringFromZipEntry(archive, "metadata.json"), jsonSettings);
                config.Settings = JsonConvert.DeserializeObject<ConfigSettings>(ReadStringFromZipEntry(archive, "settings.json"), jsonSettings);

                if (archive.Entries.Any(e => e.Name.Contains("script.cs")))
                {
                    config.CSharpScript = ReadStringFromZipEntry(archive, "script.cs");
                    config.CSharpMode = true;
                }
                else
                {
                    config.Blocks = JsonConvert.DeserializeObject<List<BlockInstance>>(ReadStringFromZipEntry(archive, "blocks.json"), jsonSettings);
                }
            }

            return await Task.FromResult(config);
        }

        private async Task CreateZipEntryFromString(ZipArchive archive, string path, string content)
        {
            var zipFile = archive.CreateEntry(path);

            using var sourceFileStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            using var zipEntryStream = zipFile.Open();
            await sourceFileStream.CopyToAsync(zipEntryStream);
        }

        private string ReadStringFromZipEntry(ZipArchive archive, string path)
            => Encoding.UTF8.GetString(ReadBytesFromZipEntry(archive, path));

        private byte[] ReadBytesFromZipEntry(ZipArchive archive, string path)
        {
            var entry = archive.GetEntry(path);

            using Stream stream = entry.Open();
            using var ms = new MemoryStream();
            
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        private string GetFileName(Config config)
            => GetFileName(config.Id);

        private string GetFileName(string id)
            => $"Configs/{id}.opk";
    }
}
