using Newtonsoft.Json;
using RuriLib.Models.Configs;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Helpers
{
    public static class ConfigPacker
    {
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Packs the <paramref name="config"/> and returns the bytes of the resulting archive.
        /// </summary>
        public static async Task<byte[]> Pack(Config config)
        {
            using var packageStream = new MemoryStream();
            using (var archive = new ZipArchive(packageStream, ZipArchiveMode.Create, false))
            {
                await CreateZipEntryFromString(archive, "readme.md", config.Readme);
                await CreateZipEntryFromString(archive, "metadata.json", JsonConvert.SerializeObject(config.Metadata, jsonSettings));
                await CreateZipEntryFromString(archive, "settings.json", JsonConvert.SerializeObject(config.Settings, jsonSettings));

                switch (config.Mode)
                {
                    case ConfigMode.Stack:
                        config.ChangeMode(ConfigMode.LoliCode);
                        goto case ConfigMode.LoliCode;

                    case ConfigMode.LoliCode:
                        await CreateZipEntryFromString(archive, "script.loli", config.LoliCodeScript);
                        break;

                    case ConfigMode.CSharp:
                        await CreateZipEntryFromString(archive, "script.cs", config.CSharpScript);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            config.UpdateHashes();
            return packageStream.ToArray();
        }

        /// <summary>
        /// Unpacks a <paramref name="stream"/> to a Config.
        /// </summary>
        public static async Task<Config> Unpack(Stream stream)
        {
            var config = new Config();

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                try
                {
                    // Reading the readme is not essential
                    config.Readme = ReadStringFromZipEntry(archive, "readme.md");
                }
                catch
                {
                    Console.WriteLine($"Could not read readme.md in config with id {config.Id}");
                }

                config.Metadata = JsonConvert.DeserializeObject<ConfigMetadata>(ReadStringFromZipEntry(archive, "metadata.json"), jsonSettings);
                config.Settings = JsonConvert.DeserializeObject<ConfigSettings>(ReadStringFromZipEntry(archive, "settings.json"), jsonSettings);

                if (archive.Entries.Any(e => e.Name.Contains("script.cs")))
                {
                    config.CSharpScript = ReadStringFromZipEntry(archive, "script.cs");
                    config.Mode = ConfigMode.CSharp;
                }
                else
                {
                    config.LoliCodeScript = ReadStringFromZipEntry(archive, "script.loli");
                    config.Mode = ConfigMode.LoliCode;
                }
            }

            config.UpdateHashes();
            return await Task.FromResult(config);
        }

        private static async Task CreateZipEntryFromString(ZipArchive archive, string path, string content)
        {
            var zipFile = archive.CreateEntry(path);

            using var sourceFileStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            using var zipEntryStream = zipFile.Open();
            await sourceFileStream.CopyToAsync(zipEntryStream);
        }

        private static string ReadStringFromZipEntry(ZipArchive archive, string path)
            => Encoding.UTF8.GetString(ReadBytesFromZipEntry(archive, path));

        private static byte[] ReadBytesFromZipEntry(ZipArchive archive, string path)
        {
            var entry = archive.GetEntry(path);

            using Stream stream = entry.Open();
            using var ms = new MemoryStream();

            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
