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

            if (!File.Exists(EnvFile))
            {
                File.WriteAllText(EnvFile, GetDefaultEnvironment());
            }

            Environment = EnvironmentSettings.FromIni(EnvFile);

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

        private string GetDefaultEnvironment()
        {
            return
@"[WORDLIST TYPE]
Name=Default
Regex=^.*$
Verify=False
Separator=:
Slices=DATA

[WORDLIST TYPE]
Name=Email
Regex=^[^@]+@[^\.]+\..+$
Verify=True
Separator=:
Slices=EMAIL

[WORDLIST TYPE]
Name=Credentials
Regex=^.*:.*$
Verify=True
Separator=:
Slices=USERNAME,PASSWORD

[WORDLIST TYPE]
Name=Numeric
Regex=^[0-9]*$
Verify=True
Separator=:
Slices=CODE

[WORDLIST TYPE]
Name=URLs
Regex=^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$
Verify=True
Separator=:
Slices=URL

[CUSTOM STATUS]
Name=CUSTOM
Color=#FFA500

[EXPORT FORMAT]
Format=<CAPTURE>

[EXPORT FORMAT]
Format=<DATA>:<PROXY>:<CAPTURE>

[EXPORT FORMAT]
Format=<DATA>\t<PROXY>\t<CAPTURE>\t
";
        }
    }
}
