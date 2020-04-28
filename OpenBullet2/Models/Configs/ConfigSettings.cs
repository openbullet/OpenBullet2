using OpenBullet2.Models.Configs.Settings;

namespace OpenBullet2.Models.Configs
{
    public class ConfigSettings
    {
        public GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();
        public RequestsSettings RequestsSettings { get; set; } = new RequestsSettings();
        public ProxySettings ProxySettings { get; set; } = new ProxySettings();
        public InputSettings InputSettings { get; set; } = new InputSettings();
        public DataSettings DataSettings { get; set; } = new DataSettings();
        public SeleniumSettings SeleniumSettings { get; set; } = new SeleniumSettings();
    }
}
