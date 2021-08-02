using Newtonsoft.Json;
using OpenBullet2.Core.Models.Settings;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Services
{
    public class OpenBulletSettingsService
    {
        private string BaseFolder { get; }
        private readonly JsonSerializerSettings jsonSettings;
        public string FileName => Path.Combine(BaseFolder, "OpenBulletSettings.json");

        public OpenBulletSettings Settings { get; private set; }

        public OpenBulletSettingsService(string baseFolder)
        {
            BaseFolder = baseFolder;
            Directory.CreateDirectory(baseFolder);

            jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };

            if (File.Exists(FileName))
            {
                Settings = JsonConvert.DeserializeObject<OpenBulletSettings>(File.ReadAllText(FileName), jsonSettings);
            }
            else
            {
                Recreate();
            }
        }

        public async Task Save() => await File.WriteAllTextAsync(FileName, JsonConvert.SerializeObject(Settings, jsonSettings));

        public void Recreate() => Settings = new OpenBulletSettings
        {
            GeneralSettings = new GeneralSettings { ProxyCheckTargets = new List<ProxyCheckTarget> { new ProxyCheckTarget() } },
            RemoteSettings = new RemoteSettings(),
            SecuritySettings = new SecuritySettings().GenerateJwtKey().SetupAdminPassword("admin"),
            CustomizationSettings = new CustomizationSettings()
        };
    }
}
