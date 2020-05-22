using OpenBullet2.Models;
using RuriLib.Models.Environment;
using RuriLib.Models.Settings;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace OpenBullet2.Services
{
    public class PersistentSettingsService
    {
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        private readonly string envFile = "Environment.ini";
        private readonly string rlSettFile = "RuriLibSettings.json";

        public EnvironmentSettings Environment { get; set; }
        public GlobalSettings RuriLibSettings { get; set; } = new GlobalSettings();
        public SecurityOptions SecurityOptions { get; set; } = new SecurityOptions();

        public PersistentSettingsService(IConfiguration configuration)
        {
            if (File.Exists(envFile)) Environment = EnvironmentSettings.FromIni(envFile);
            else throw new FileNotFoundException(envFile);

            RuriLibSettings = File.Exists(rlSettFile)
                ? JsonConvert.DeserializeObject<GlobalSettings>(File.ReadAllText(rlSettFile), jsonSettings)
                : new GlobalSettings();

            SecurityOptions = configuration.GetSection("Security").Get<SecurityOptions>();
        }

        public string[] GetStatuses()
        {
            return (new string[]
            {
            "SUCCESS", "NONE", "FAIL", "RETRY", "BAN", "ERROR"
                }).Concat(Environment.CustomStatuses.Select(s => s.Name)).ToArray();
        }
    }
}
