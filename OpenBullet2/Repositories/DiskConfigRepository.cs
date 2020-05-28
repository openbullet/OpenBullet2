using RuriLib.Models.Configs;
using RuriLib.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

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
            config.Metadata.LastModified = DateTime.Now;
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
