using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.DTOs;
using OpenBullet2.Native.Utils;
using RuriLib.Functions.Files;
using RuriLib.Models.Configs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace OpenBullet2.Native.ViewModels
{
    public class ConfigsViewModel : ViewModelBase
    {
        private readonly ConfigService configService;
        private readonly IConfigRepository configRepo;

        private ObservableCollection<ConfigViewModel> configsCollection;
        public ObservableCollection<ConfigViewModel> ConfigsCollection
        {
            get => configsCollection;
            set
            {
                configsCollection = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        private string searchString = string.Empty;
        public string SearchString
        {
            get => searchString;
            set
            {
                searchString = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(ConfigsCollection).Refresh();
                OnPropertyChanged(nameof(Total));
            }
        }

        private ConfigViewModel selectedConfig;
        public ConfigViewModel SelectedConfig
        {
            get => selectedConfig;
            set
            {
                selectedConfig = value;
                configService.SelectedConfig = value?.Config;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsConfigSelected));
            }
        }

        public bool IsConfigSelected => SelectedConfig != null;

        private ConfigViewModel hoveredConfig;
        public ConfigViewModel HoveredConfig
        {
            get => hoveredConfig;
            set
            {
                hoveredConfig = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsConfigHovered));
            }
        }

        public bool IsConfigHovered => HoveredConfig != null;

        public int Total => ConfigsCollection.Count;

        public ConfigsViewModel()
        {
            configService = SP.GetService<ConfigService>();
            configService.OnRemotesLoaded += (s, e) => CreateCollection();

            configRepo = SP.GetService<IConfigRepository>();
            CreateCollection();
        }

        public async Task Create(ConfigForCreationDto dto)
        {
            // Create it in the repo
            var fileName = FileUtils.ReplaceInvalidFileNameChars($"{dto.Name}.opk");
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserData/Configs", fileName);

            if (File.Exists(filePath))
            {
                filePath = FileUtils.GetFirstAvailableFileName(filePath);
            }

            var newConfig = await configRepo.Create(Path.GetFileNameWithoutExtension(filePath));
            newConfig.Metadata.Name = dto.Name;
            newConfig.Metadata.Category = dto.Category;
            newConfig.Metadata.Author = dto.Author;

            var newConfigVM = new ConfigViewModel(newConfig);
            await Save(newConfigVM);

            // Add it to the observable collection
            ConfigsCollection.Insert(0, newConfigVM);

            // Add it to the service
            configService.SelectedConfig = newConfig;
            configService.Configs.Add(newConfig);
        }

        public void Delete(ConfigViewModel vm)
        {
            if (vm == SelectedConfig)
            {
                SelectedConfig = null;
            }

            configRepo.Delete(vm.Config);
            ConfigsCollection.Remove(vm);
        }

        public Task Save(ConfigViewModel vm)
        {
            if (vm.IsRemote)
            {
                throw new Exception("You cannot save remote configs");
            }

            return configRepo.Save(vm.Config);
        }

        public async Task Rescan()
        {
            SelectedConfig = null;
            HoveredConfig = null;
            await configService.ReloadConfigs();
            CreateCollection();
        }

        public override void UpdateViewModel()
        {
            ConfigsCollection.ToList().ForEach(c => c.UpdateViewModel());
            base.UpdateViewModel();
        }

        public void CreateCollection()
        {
            var viewModels = configService.Configs.Select(c => new ConfigViewModel(c));
            ConfigsCollection = new ObservableCollection<ConfigViewModel>(viewModels);
            Application.Current.Dispatcher.Invoke(() => HookFilters());
        }

        private void HookFilters()
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(ConfigsCollection);
            view.Filter = ConfigsFilter;
        }

        private bool ConfigsFilter(object item) => (item as ConfigViewModel).Config
            .Metadata.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }

    public class ConfigViewModel : ViewModelBase
    {
        public Config Config { get; init; }

        public string Id => Config.Id;
        public BitmapImage Icon => Images.Base64ToBitmapImage(Config.Metadata.Base64Image);
        public string Name => Config.Metadata.Name;
        public string Author => Config.Metadata.Author;
        public string Category => Config.Metadata.Category;
        public bool NeedsProxies => Config.Settings.ProxySettings.UseProxies;
        public string AllowedWordlistTypes => Config.Settings.DataSettings.AllowedWordlistTypesString;
        public DateTime CreationDate => Config.Metadata.CreationDate;
        public DateTime LastModified => Config.Metadata.LastModified;
        public string Readme => Config.Readme;
        public bool IsRemote => Config.IsRemote;

        public ConfigViewModel(Config config)
        {
            Config = config;
        }
    }
}
