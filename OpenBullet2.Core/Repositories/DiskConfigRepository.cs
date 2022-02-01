using RuriLib.Models.Configs;
using RuriLib.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using RuriLib.Helpers.Transpilers;
using RuriLib.Services;
using RuriLib.Legacy.Configs;
using System.Text;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores configs on disk.
    /// </summary>
    public class DiskConfigRepository : IConfigRepository
    {
        private readonly RuriLibSettingsService rlSettings;

        private string BaseFolder { get; init; }

        public DiskConfigRepository(RuriLibSettingsService rlSettings, string baseFolder)
        {
            this.rlSettings = rlSettings;
            BaseFolder = baseFolder;
            Directory.CreateDirectory(baseFolder);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Config>> GetAll()
        {
            // Try to convert legacy configs automatically before loading
            foreach (var file in Directory.GetFiles(BaseFolder).Where(file => file.EndsWith(".loli")))
            {
                try
                {
                    var id = Path.GetFileNameWithoutExtension(file);
                    var converted = ConfigConverter.Convert(File.ReadAllText(file), id);
                    await Save(converted);
                    File.Delete(file);
                    Console.WriteLine($"Converted legacy .loli config ({file}) to the new .opk format");
                }
                catch
                {
                    Console.WriteLine($"Could not convert legacy .loli config ({file}) to the new .opk format");
                }
            }

            var tasks = Directory.GetFiles(BaseFolder).Where(file => file.EndsWith(".opk"))
                .Select(async file => 
                {
                    try
                    {
                        return await Get(Path.GetFileNameWithoutExtension(file));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not unpack {file} properly: {ex.Message}");
                        return null;
                    }
                });

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null);
        }

        /// <inheritdoc/>
        public async Task<Config> Get(string id)
        {
            var file = GetFileName(id);

            if (File.Exists(file))
            {
                using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

                var config = await ConfigPacker.Unpack(fileStream);
                config.Id = id;
                return config;
            }

            throw new FileNotFoundException();
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetBytes(string id)
        {
            var file = GetFileName(id);

            if (File.Exists(file))
            {
                using FileStream fileStream = new(file, FileMode.Open, FileAccess.Read);
                using var ms = new MemoryStream();
                await fileStream.CopyToAsync(ms);

                return ms.ToArray();
            }

            throw new FileNotFoundException();
        }

        /// <inheritdoc/>
        public async Task<Config> Create(string id = null)
        {
            var config = new Config();

            if (id is not null)
            {
                config.Id = id;
            }

            config.Settings.DataSettings.AllowedWordlistTypes = new string[]
            {
                rlSettings.Environment.WordlistTypes.First().Name
            };

            await Save(config);
            return config;
        }

        /// <inheritdoc/>
        public async Task Upload(Stream stream, string fileName)
        {
            var extension = Path.GetExtension(fileName);

            // If it's a .opk config
            if (extension == ".opk")
            {
                var config = await ConfigPacker.Unpack(stream);
                await File.WriteAllBytesAsync(GetFileName(config), await ConfigPacker.Pack(config));
            }
            // Otherwise it's a .loli config
            else if (extension == ".loli")
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var content = Encoding.UTF8.GetString(ms.ToArray());
                var id = Path.GetFileNameWithoutExtension(fileName);
                var converted = ConfigConverter.Convert(content, id);
                await Save(converted);
            }
            else
            {
                throw new Exception($"Unsupported file type: {extension}");
            }
        }

        /// <inheritdoc/>
        public async Task Save(Config config)
        {
            // Update the last modified date
            config.Metadata.LastModified = DateTime.Now;

            // If not a csharp config, try to build the stack to get required plugins
            if (config.Mode != ConfigMode.CSharp)
            {
                try
                {
                    var stack = config.Mode == ConfigMode.Stack
                        ? config.Stack
                        : Loli2StackTranspiler.Transpile(config.LoliCodeScript);

                    // Write the required plugins in the config's metadata
                    config.Metadata.Plugins = stack.Select(b => b.Descriptor.AssemblyFullName)
                        .Where(n => n != null && !n.Contains("RuriLib")).ToList();
                }
                catch
                {
                    // Don't do anything, it's not the end of the world if we don't write some metadata ^_^
                }
            }

            await File.WriteAllBytesAsync(GetFileName(config), await ConfigPacker.Pack(config));
        }

        /// <inheritdoc/>
        public void Delete(Config config)
        {
            var file = GetFileName(config);

            if (File.Exists(file))
                File.Delete(file);
        }

        private string GetFileName(Config config)
            => GetFileName(config.Id);

        private string GetFileName(string id)
            => Path.Combine(BaseFolder, $"{id}.opk").Replace('\\', '/');
    }
}
