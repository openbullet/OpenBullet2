using OpenBullet2.Models.Configs.Settings;

namespace OpenBullet2.Models.Configs
{
    public class ConfigSettings
    {
        public ProxySettings ProxySettings { get; set; } = new ProxySettings();
        public GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();
    }
}
