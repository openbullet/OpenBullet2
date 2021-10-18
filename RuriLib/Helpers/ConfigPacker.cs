using Newtonsoft.Json;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Helpers
{
    /// <summary>
    /// Takes care of packing and unpacking <see cref="Config"/> objects.
    /// </summary>
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
                        config.LoliCodeScript = Stack2LoliTranspiler.Transpile(config.Stack);
                        await CreateZipEntryFromString(archive, "script.loli", config.LoliCodeScript);
                        break;

                    case ConfigMode.LoliCode:
                        await CreateZipEntryFromString(archive, "script.loli", config.LoliCodeScript);
                        break;

                    case ConfigMode.CSharp:
                        await CreateZipEntryFromString(archive, "script.cs", config.CSharpScript);
                        break;

                    case ConfigMode.DLL:
                        await CreateZipEntryFromBytes(archive, "build.dll", config.DLLBytes);
                        break;

                    case ConfigMode.Legacy:
                        await CreateZipEntryFromString(archive, "script.legacy", config.LoliScript);
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
                // readme.md (not essential)
                try
                {
                    config.Readme = ReadStringFromZipEntry(archive, "readme.md");
                }
                catch
                {
                    Console.WriteLine($"Could not read readme.md in config with id {config.Id}");
                }

                // metadata.json
                try
                {
                    config.Metadata = JsonConvert.DeserializeObject<ConfigMetadata>(ReadStringFromZipEntry(archive, "metadata.json"), jsonSettings);
                }
                catch
                {
                    throw new FileNotFoundException("File not found inside the opk archive", "metadata.json");
                }

                // settings.json
                try
                {
                    config.Settings = JsonConvert.DeserializeObject<ConfigSettings>(ReadStringFromZipEntry(archive, "settings.json"), jsonSettings);
                }
                catch
                {
                    throw new FileNotFoundException("File not found inside the opk archive", "settings.json");
                }
                
                if (archive.Entries.Any(e => e.Name.Contains("script.cs")))
                {
                    // script.cs
                    try
                    {
                        config.CSharpScript = ReadStringFromZipEntry(archive, "script.cs");
                        config.Mode = ConfigMode.CSharp;
                    }
                    catch
                    {
                        throw new FileLoadException("Could not load the file from the opk archive", "script.cs");
                    }
                }
                else if (archive.Entries.Any(e => e.Name.Contains("build.dll")))
                {
                    // build.dll
                    try
                    {
                        config.DLLBytes = ReadBytesFromZipEntry(archive, "build.dll");
                        config.Mode = ConfigMode.DLL;
                    }
                    catch
                    {
                        throw new FileLoadException("Could not load the file from the opk archive", "build.dll");
                    }
                }
                else if (archive.Entries.Any(e => e.Name.Contains("script.legacy")))
                {
                    // script.legacy
                    try
                    {
                        config.LoliScript = ReadStringFromZipEntry(archive, "script.legacy");
                        config.Mode = ConfigMode.Legacy;
                    }
                    catch
                    {
                        throw new FileLoadException("Could not load the file from the opk archive", "script.legacy");
                    }
                }
                else
                {
                    // script.loli
                    try
                    {
                        config.LoliCodeScript = ReadStringFromZipEntry(archive, "script.loli");
                        config.Mode = ConfigMode.LoliCode;
                    }
                    catch
                    {
                        throw new FileLoadException("Could not load the file from the opk archive", "script.loli");
                    }
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

        private static async Task CreateZipEntryFromBytes(ZipArchive archive, string path, byte[] content)
        {
            var zipFile = archive.CreateEntry(path);

            using var sourceFileStream = new MemoryStream(content);
            using var zipEntryStream = zipFile.Open();
            await sourceFileStream.CopyToAsync(zipEntryStream);
        }

        private static string ReadStringFromZipEntry(ZipArchive archive, string path)
            => Encoding.UTF8.GetString(ReadBytesFromZipEntry(archive, path));

        private static byte[] ReadBytesFromZipEntry(ZipArchive archive, string path)
        {
            var entry = archive.GetEntry(path);

            using var stream = entry.Open();
            using var ms = new MemoryStream();

            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
