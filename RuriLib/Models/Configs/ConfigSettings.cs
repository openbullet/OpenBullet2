using RuriLib.Models.Configs.Settings;
using System.Text.Json.Serialization;

namespace RuriLib.Models.Configs
{
    public class ConfigSettings
    {
        public GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();
        public ProxySettings ProxySettings { get; set; } = new ProxySettings();
        public InputSettings InputSettings { get; set; } = new InputSettings();
        public DataSettings DataSettings { get; set; } = new DataSettings();

        [JsonPropertyName("PuppeteerSettings")] // For backwards compatibility
        public BrowserSettings BrowserSettings { get; set; } = new BrowserSettings();

        public ScriptSettings ScriptSettings { get; set; } = new ScriptSettings();
    }
}
