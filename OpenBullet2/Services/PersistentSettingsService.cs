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
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        private readonly string obSettFile = "OpenBulletSettings.json";

        public SecurityOptions SecurityOptions { get; private set; }
        public OpenBulletSettings OpenBulletSettings { get; set; }

        public PersistentSettingsService(IConfiguration configuration)
        {
            SecurityOptions = configuration.GetSection("Security").Get<SecurityOptions>();

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
            OpenBulletSettings = new OpenBulletSettings();

            if (OpenBulletSettings.GeneralSettings.ProxyCheckTargets == null)
                OpenBulletSettings.GeneralSettings.ProxyCheckTargets = new List<ProxyCheckTarget> { new ProxyCheckTarget() };
        }
    }
}
