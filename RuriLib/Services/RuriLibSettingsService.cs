using Newtonsoft.Json;
using RuriLib.Helpers;
using RuriLib.Models.Environment;
using RuriLib.Models.Settings;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Services;

/// <summary>
/// Loads and persists the RuriLib environment and global settings.
/// </summary>
public class RuriLibSettingsService
{
    private readonly JsonSerializerSettings _jsonSettings;
    private string BaseFolder { get; init; }
    private string EnvFile => Path.Combine(BaseFolder, "Environment.ini");
    private string RlSettFile => Path.Combine(BaseFolder, "RuriLibSettings.json");

    /// <summary>
    /// Gets or sets the parsed environment settings.
    /// </summary>
    public EnvironmentSettings Environment { get; set; }

    /// <summary>
    /// Gets or sets the persisted RuriLib settings.
    /// </summary>
    public GlobalSettings RuriLibSettings { get; set; }

    /// <summary>
    /// Creates a settings service rooted at the given base folder.
    /// </summary>
    /// <param name="baseFolder">The folder that stores the environment and settings files.</param>
    public RuriLibSettingsService(string baseFolder)
    {
        BaseFolder = baseFolder;
        Directory.CreateDirectory(baseFolder);

        _jsonSettings = new JsonSerializerSettings
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
            ? JsonConvert.DeserializeObject<GlobalSettings>(File.ReadAllText(RlSettFile), _jsonSettings) ?? CreateGlobalSettings()
            : CreateGlobalSettings();
    }

    /// <summary>
    /// Saves the settings to the designated file.
    /// </summary>
    /// <returns>A task that completes when the settings file has been written.</returns>
    public async Task Save()
        => await File.WriteAllTextAsync(RlSettFile, JsonConvert.SerializeObject(RuriLibSettings, _jsonSettings));

    /// <summary>
    /// Gets the currently supported statuses (including the custom ones defined in the Environment settings).
    /// </summary>
    /// <returns>The built-in and custom status names.</returns>
    public string[] GetStatuses()
        => ["SUCCESS",
            "NONE",
            "FAIL",
            "RETRY",
            "BAN",
            "ERROR",
            .. Environment.CustomStatuses.Select(s => s.Name)];

    private GlobalSettings CreateGlobalSettings()
    {
        var settings = new GlobalSettings();

        if (Utils.IsDocker())
        {
            settings.PuppeteerSettings.ChromeBinaryLocation = "/usr/bin/chromium";
            settings.SeleniumSettings.ChromeBinaryLocation = "/usr/bin/chromium";
            settings.SeleniumSettings.FirefoxBinaryLocation = "/usr/bin/firefox";
        }

        return settings;
    }

    private string GetDefaultEnvironment() =>
@"[WORDLIST TYPE]
Name=Default
Regex=^.*$
Verify=False
Separator=
Slices=DATA

[WORDLIST TYPE]
Name=Emails
Regex=^[^@]+@[^\.]+\..+$
Verify=True
Separator=
Slices=EMAIL

[WORDLIST TYPE]
Name=Credentials
Regex=^.*:.*$
Verify=True
Separator=:
Slices=USERNAME,PASSWORD
SlicesAlias=USER,PASS

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
Separator=
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
