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
        private string BaseFolder { get; init; }
        private string EnvFile => Path.Combine(BaseFolder, "Environment.ini");
        private string RlSettFile => Path.Combine(BaseFolder, "RuriLibSettings.json");

        public EnvironmentSettings Environment { get; set; }
        public GlobalSettings RuriLibSettings { get; set; }

        public RuriLibSettingsService(string baseFolder)
        {
            BaseFolder = baseFolder;
            Directory.CreateDirectory(baseFolder);

            jsonSettings = new JsonSerializerSettings 
            { 
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };

            if (File.Exists(EnvFile)) Environment = EnvironmentSettings.FromIni(EnvFile);
            else throw new FileNotFoundException(EnvFile);

            RuriLibSettings = File.Exists(RlSettFile)
                ? JsonConvert.DeserializeObject<GlobalSettings>(File.ReadAllText(RlSettFile), jsonSettings)
                : new GlobalSettings();
        }

        public async Task Save()
        {
            await File.WriteAllTextAsync(RlSettFile, JsonConvert.SerializeObject(RuriLibSettings, jsonSettings));
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
