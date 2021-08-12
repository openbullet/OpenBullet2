using RuriLib.Models.Configs;
using RuriLib.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using RuriLib.Helpers.Transpilers;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores configs on disk.
    /// </summary>
    public class DiskConfigRepository : IConfigRepository
    {
        private string BaseFolder { get; init; }

        public DiskConfigRepository(string baseFolder)
        {
            BaseFolder = baseFolder;
            Directory.CreateDirectory(baseFolder);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Config>> GetAll()
        {
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
        public async Task<Config> Create()
        {
            var config = new Config();
            await Save(config);
            return config;
        }

        /// <inheritdoc/>
        public async Task<Config> Create(string id)
        {
            var config = new Config
            {
                Id = id
            };
            await Save(config);
            return config;
        }

        /// <inheritdoc/>
        public async Task Upload(Stream stream)
        {
            var config = await ConfigPacker.Unpack(stream);
            await File.WriteAllBytesAsync(GetFileName(config), await ConfigPacker.Pack(config));
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
