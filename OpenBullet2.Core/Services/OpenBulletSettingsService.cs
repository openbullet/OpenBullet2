using Newtonsoft.Json;
using OpenBullet2.Core.Models.Settings;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Services
{
    /// <summary>
    /// Provides interaction with settings of the OpenBullet 2 application.
    /// </summary>
    public class OpenBulletSettingsService
    {
        private string BaseFolder { get; }
        private readonly JsonSerializerSettings jsonSettings;

        /// <summary>
        /// The path of the file where settings are saved.
        /// </summary>
        public string FileName => Path.Combine(BaseFolder, "OpenBulletSettings.json");

        /// <summary>
        /// The actual settings. After modifying them, call the <see cref="Save"/> method to persist them.
        /// </summary>
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

        /// <summary>
        /// Saves the <see cref="Settings"/> to disk.
        /// </summary>
        public async Task Save() => await File.WriteAllTextAsync(FileName, JsonConvert.SerializeObject(Settings, jsonSettings));

        /// <summary>
        /// Restores the default <see cref="Settings"/> (does not save to disk).
        /// </summary>
        public void Recreate() => Settings = new OpenBulletSettings
        {
            GeneralSettings = new GeneralSettings { ProxyCheckTargets = new List<ProxyCheckTarget> { new ProxyCheckTarget() } },
            RemoteSettings = new RemoteSettings(),
            SecuritySettings = new SecuritySettings().GenerateJwtKey().SetupAdminPassword("admin"),
            CustomizationSettings = new CustomizationSettings()
        };
    }
}
