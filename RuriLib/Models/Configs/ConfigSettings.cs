using RuriLib.Models.Configs.Settings;

namespace RuriLib.Models.Configs
{
    public class ConfigSettings
    {
        public GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();
        public ProxySettings ProxySettings { get; set; } = new ProxySettings();
        public InputSettings InputSettings { get; set; } = new InputSettings();
        public DataSettings DataSettings { get; set; } = new DataSettings();
        public PuppeteerSettings PuppeteerSettings { get; set; } = new PuppeteerSettings();
        public ScriptSettings ScriptSettings { get; set; } = new ScriptSettings();
    }
}
