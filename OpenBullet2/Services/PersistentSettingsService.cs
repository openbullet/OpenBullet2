using OpenBullet2.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenBullet2.Models.Settings;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OpenBullet2.Services
{
    public class PersistentSettingsService
    {
        private readonly JsonSerializerSettings jsonSettings;

        private readonly string obSettFile = "OpenBulletSettings.json";

        public OpenBulletSettings OpenBulletSettings { get; set; }

        public PersistentSettingsService()
        {
            jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };

            if (File.Exists(obSettFile))
                OpenBulletSettings = JsonConvert.DeserializeObject<OpenBulletSettings>(File.ReadAllText(obSettFile), jsonSettings);
            else
                Recreate();
        }

        public async Task Save()
        {
            await File.WriteAllTextAsync(obSettFile, JsonConvert.SerializeObject(OpenBulletSettings, jsonSettings));
        }

        public void Recreate()
        {
            OpenBulletSettings = new OpenBulletSettings 
            {
                GeneralSettings = new GeneralSettings { ProxyCheckTargets = new List<ProxyCheckTarget> { new ProxyCheckTarget() } },
                RemoteSettings = new RemoteSettings(),
                SecuritySettings = new SecuritySettings().GenerateJwtKey().SetupAdminPassword("admin")
            };
        }
    }
}
