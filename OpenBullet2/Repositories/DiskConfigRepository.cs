using RuriLib.Models.Configs;
using RuriLib.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using RuriLib;
using RuriLib.Models.Blocks;
using RuriLib.Helpers.Transpilers;

namespace OpenBullet2.Repositories
{
    public class DiskConfigRepository : IConfigRepository
    {
        private string configDirectory = "Configs";

        public DiskConfigRepository()
        {
            Directory.CreateDirectory(configDirectory);
        }

        public async Task<List<Config>> GetAll()
        {
            List<Config> configs = new List<Config>();

            // TODO: Parallelize this for max performance
            foreach (var file in Directory.GetFiles(configDirectory).Where(file => file.EndsWith(".opk")))
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

                var config = await ConfigPacker.Unpack(fileStream);
                config.Id = id;
                return config;
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
            var config = await ConfigPacker.Unpack(stream);
            await File.WriteAllBytesAsync(GetFileName(config), await ConfigPacker.Pack(config));
        }

        public async Task Save(Config config)
        {
            // Update the last modified date
            config.Metadata.LastModified = DateTime.Now;

            // If not a csharp config, try to build the stack to get required plugins
            if (config.Mode != ConfigMode.CSharp)
            {
                try
                {
                    List<BlockInstance> stack = null;

                    if (config.Mode == ConfigMode.Stack)
                    {
                        stack = config.Stack;
                    }
                    else
                    {
                        stack = new Loli2StackTranspiler().Transpile(config.LoliCodeScript);
                    }

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

        public void Delete(Config config)
        {
            var file = GetFileName(config);

            if (File.Exists(file))
                File.Delete(file);
        }

        private string GetFileName(Config config)
            => GetFileName(config.Id);

        private string GetFileName(string id)
            => $"{configDirectory}/{id}.opk";
    }
}
