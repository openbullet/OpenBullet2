using OpenBullet2.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenBullet2.Models.Settings;
using System.IO;
using System.Threading.Tasks;

namespace OpenBullet2.Services
{
    public class PersistentSettingsService
    {
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        private readonly string obSettFile = "OpenBulletSettings.json";

        public SecurityOptions SecurityOptions { get; private set; }
        public OpenBulletSettings OpenBulletSettings { get; set; }

        public PersistentSettingsService(IConfiguration configuration)
        {
            SecurityOptions = configuration.GetSection("Security").Get<SecurityOptions>();

            OpenBulletSettings = File.Exists(obSettFile)
                ? JsonConvert.DeserializeObject<OpenBulletSettings>(File.ReadAllText(obSettFile), jsonSettings)
                : new OpenBulletSettings();
        }

        public async Task Save()
        {
            await File.WriteAllTextAsync(obSettFile, JsonConvert.SerializeObject(OpenBulletSettings, jsonSettings));
        }
    }
}
