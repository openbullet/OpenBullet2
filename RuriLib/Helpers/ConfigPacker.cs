using Newtonsoft.Json;
using RuriLib.Extensions;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
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
                        await CreateZipEntryFromString(archive, "startup.loli", config.StartupLoliCodeScript);
                        break;

                    case ConfigMode.LoliCode:
                        await CreateZipEntryFromString(archive, "script.loli", config.LoliCodeScript);
                        await CreateZipEntryFromString(archive, "startup.loli", config.StartupLoliCodeScript);
                        break;

                    case ConfigMode.CSharp:
                        await CreateZipEntryFromString(archive, "script.cs", config.CSharpScript);
                        await CreateZipEntryFromString(archive, "startup.cs", config.StartupCSharpScript);
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
        /// Packs multiple <paramref name="configs"/> into a single archive.
        /// </summary>
        public static async Task<byte[]> Pack(IEnumerable<Config> configs)
        {
            // Use a dictionary to keep track of filenames and avoid duplicates
            var fileNames = new Dictionary<string, int>();

            using var packageStream = new MemoryStream();
            using (var archive = new ZipArchive(packageStream, ZipArchiveMode.Create, false))
            {
                foreach (var config in configs)
                {
                    var fileName = config.Metadata.Name.ToValidFileName();

                    // If a config with the same filename was already added, append a number
                    // and increase it for the next round
                    if (fileNames.ContainsKey(fileName))
                    {
                        fileNames[fileName]++;
                        fileName += fileNames[fileName];
                    }
                    // Otherwise create a new entry in the dictionary
                    else
                    {
                        fileNames[fileName] = 1;
                    }

                    var packedConfig = await Pack(config);
                    await CreateZipEntryFromBytes(archive, fileName + ".opk", packedConfig);
                }
            }

            return packageStream.ToArray();
        }

        /// <summary>
        /// Unpacks a <paramref name="stream"/> to a Config.
        /// </summary>
        public static Task<Config> Unpack(Stream stream)
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

                    // startup.cs
                    config.StartupCSharpScript = ReadStringFromZipEntry(archive, "startup.cs", essential: false);
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

                    // startup.loli
                    config.StartupLoliCodeScript = ReadStringFromZipEntry(archive, "startup.loli", essential: false);
                }
            }

            config.UpdateHashes();
            return Task.FromResult(config);
        }

        private static async Task CreateZipEntryFromString(ZipArchive archive, string path, string content)
        {
            var zipFile = archive.CreateEntry(path);

            using var sourceFileStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await using var zipEntryStream = zipFile.Open();
            await sourceFileStream.CopyToAsync(zipEntryStream);
        }

        private static async Task CreateZipEntryFromBytes(ZipArchive archive, string path, byte[] content)
        {
            var zipFile = archive.CreateEntry(path);

            using var sourceFileStream = new MemoryStream(content);
            await using var zipEntryStream = zipFile.Open();
            await sourceFileStream.CopyToAsync(zipEntryStream);
        }

        private static string ReadStringFromZipEntry(ZipArchive archive, string path, bool essential = true)
            => Encoding.UTF8.GetString(ReadBytesFromZipEntry(archive, path, essential));

        private static byte[] ReadBytesFromZipEntry(ZipArchive archive, string path, bool essential = true)
        {
            var entry = archive.GetEntry(path);

            if (entry is null && !essential)
            {
                return Array.Empty<byte>();
            }

            using var stream = entry.Open();
            using var ms = new MemoryStream();

            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
