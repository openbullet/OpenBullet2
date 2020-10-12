using Newtonsoft.Json;
using RuriLib.Models.Environment;
using RuriLib.Models.Settings;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Services
{
    public class RuriLibSettingsService
    {
        private readonly JsonSerializerSettings jsonSettings;
        private readonly string envFile = "Environment.ini";
        private readonly string rlSettFile = "RuriLibSettings.json";

        public EnvironmentSettings Environment { get; set; }
        public GlobalSettings RuriLibSettings { get; set; }

        public RuriLibSettingsService()
        {
            jsonSettings = new JsonSerializerSettings 
            { 
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };

            if (File.Exists(envFile)) Environment = EnvironmentSettings.FromIni(envFile);
            else throw new FileNotFoundException(envFile);

            RuriLibSettings = File.Exists(rlSettFile)
                ? JsonConvert.DeserializeObject<GlobalSettings>(File.ReadAllText(rlSettFile), jsonSettings)
                : new GlobalSettings();
        }

        public async Task Save()
        {
            await File.WriteAllTextAsync(rlSettFile, JsonConvert.SerializeObject(RuriLibSettings, jsonSettings));
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
