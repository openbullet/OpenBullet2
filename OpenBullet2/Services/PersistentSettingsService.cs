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
        private string BaseFolder { get; init; }
        private readonly JsonSerializerSettings jsonSettings;
        private string ObSettFile => Path.Combine(BaseFolder, "OpenBulletSettings.json");

        public OpenBulletSettings OpenBulletSettings { get; set; }
        public bool SetupComplete => File.Exists(ObSettFile);

        public PersistentSettingsService(string baseFolder)
        {
            BaseFolder = baseFolder;
            Directory.CreateDirectory(baseFolder);

            jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };

            if (File.Exists(ObSettFile))
            {
                OpenBulletSettings = JsonConvert.DeserializeObject<OpenBulletSettings>(File.ReadAllText(ObSettFile), jsonSettings);
            }
            else
            {
                Recreate();
            }
        }

        public async Task Save()
        {
            await File.WriteAllTextAsync(ObSettFile, JsonConvert.SerializeObject(OpenBulletSettings, jsonSettings));
        }

        public void Recreate()
        {
            OpenBulletSettings = new OpenBulletSettings 
            {
                GeneralSettings = new GeneralSettings { ProxyCheckTargets = new List<ProxyCheckTarget> { new ProxyCheckTarget() } },
                RemoteSettings = new RemoteSettings(),
                SecuritySettings = new SecuritySettings().GenerateJwtKey().SetupAdminPassword("admin"),
                CustomizationSettings = new CustomizationSettings()
            };
        }
    }
}
