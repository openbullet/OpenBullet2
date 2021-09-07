using RuriLib.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace OpenBullet2.Native.ViewModels
{
    public class PluginsViewModel : ViewModelBase
    {
        private ObservableCollection<PluginInfo> pluginsCollection;
        private PluginRepository pluginRepo;

        public ObservableCollection<PluginInfo> PluginsCollection
        {
            get => pluginsCollection;
            set
            {
                pluginsCollection = value;
                OnPropertyChanged();
            }
        }

        public PluginsViewModel()
        {
            pluginRepo = SP.GetService<PluginRepository>();
            RefreshList();
        }

        public void Add(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            using var ms = new MemoryStream(bytes);
            ms.Seek(0, SeekOrigin.Begin);

            pluginRepo.AddPlugin(ms);
            RefreshList();
        }

        private void RefreshList()
        {
            PluginsCollection = new ObservableCollection<PluginInfo>(
                pluginRepo.GetPluginNames().Select(p => new PluginInfo(p)));
        }

        public void Delete(PluginInfo plugin)
        {
            pluginRepo.DeletePlugin(plugin.Name);
            PluginsCollection.Remove(plugin);
        }
    }

    public class PluginInfo
    {
        public string Name { get; set; }

        public PluginInfo(string name)
        {
            Name = name;
        }
    }
}
