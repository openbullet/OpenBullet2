using OpenBullet2.Native.ViewModels;

namespace OpenBullet2.Native.Services
{
    public class ViewModelsService
    {
        public ProxiesViewModel Proxies { get; set; }
        public WordlistsViewModel Wordlists { get; set; }
        public ConfigsViewModel Configs { get; set; }
        public OBSettingsViewModel OBSettings { get; set; }
        public RLSettingsViewModel RLSettings { get; set; }
        public PluginsViewModel Plugins { get; set; }

        public ViewModelsService()
        {
            Proxies = new();
            Wordlists = new();
            Configs = new();
            OBSettings = new();
            RLSettings = new();
            Plugins = new();
        }
    }
}
