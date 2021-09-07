using OpenBullet2.Native.ViewModels;

namespace OpenBullet2.Native.Services
{
    public class ViewModelsService
    {
        public JobsViewModel Jobs { get; set; }
        public ProxiesViewModel Proxies { get; set; }
        public WordlistsViewModel Wordlists { get; set; }
        public ConfigsViewModel Configs { get; set; }
        public HitsViewModel Hits { get; set; }
        public OBSettingsViewModel OBSettings { get; set; }
        public RLSettingsViewModel RLSettings { get; set; }
        public PluginsViewModel Plugins { get; set; }

        public ConfigMetadataViewModel ConfigMetadata { get; set; }
        public ConfigReadmeViewModel ConfigReadme { get; set; }
        public ConfigStackerViewModel ConfigStacker { get; set; }
        public ConfigSettingsViewModel ConfigSettings { get; set; }

        public DebuggerViewModel Debugger { get; set; }

        public ViewModelsService()
        {
            Jobs = new();
            Proxies = new();
            Wordlists = new();
            Configs = new();
            Hits = new();
            OBSettings = new();
            RLSettings = new();
            Plugins = new();

            ConfigMetadata = new();
            ConfigReadme = new();
            ConfigStacker = new();
            ConfigSettings = new();

            Debugger = new();
        }
    }
}
