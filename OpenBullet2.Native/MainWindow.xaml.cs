using MahApps.Metro.Controls;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.Utils;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Pages;
using RuriLib.Models.Configs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet2.Native
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private UpdateService updateService;
        private readonly MainWindowViewModel vm;

        private bool hoveringConfigsMenuOption = false;
        private bool hoveringConfigSubmenu = false;

        private readonly Label[] labels;

        private Home homePage;
        private Proxies proxiesPage;
        private Wordlists wordlistsPage;
        private Configs configsPage;
        private Views.Pages.ConfigMetadata configMetadataPage;
        private OBSettings obSettingsPage;
        private RLSettings rlSettingsPage;
        private Plugins pluginsPage;
        private About aboutPage;
        private Page currentPage;

        public MainWindow()
        {
            vm = new MainWindowViewModel();
            DataContext = vm;

            InitializeComponent();

            labels = new Label[]
            {
                menuOptionAbout,
                menuOptionConfigs,
                menuOptionConfigSettings,
                menuOptionCSharpCode,
                menuOptionHits,
                menuOptionHome,
                menuOptionJobs,
                menuOptionLoliCode,
                menuOptionMetadata,
                menuOptionMonitor,
                menuOptionOBSettings,
                menuOptionPlugins,
                menuOptionProxies,
                menuOptionReadme,
                menuOptionRLSettings,
                menuOptionStacker,
                menuOptionWordlists
            };

            updateService = SP.GetService<UpdateService>();
            Title = $"OpenBullet 2 - {updateService.CurrentVersion} [{updateService.CurrentVersionType}]";
        }

        public void Init()
        {
            homePage = new();
            proxiesPage = new();
            wordlistsPage = new();
            configsPage = new();
            configMetadataPage = new();
            obSettingsPage = new();
            rlSettingsPage = new();
            pluginsPage = new();
            aboutPage = new();

            ChangePage(homePage, menuOptionHome);
        }

        // We recreate the homepage each time to display updated announcements
        private void OpenHomePage(object sender, MouseEventArgs e)
        {
            homePage = new Home();
            ChangePage(homePage, menuOptionHome);
        }

        private void OpenJobsPage(object sender, MouseEventArgs e) { }
        private void OpenMonitorPage(object sender, MouseEventArgs e) { }
        private void OpenProxiesPage(object sender, MouseEventArgs e) => ChangePage(proxiesPage, menuOptionProxies);
        private void OpenWordlistsPage(object sender, MouseEventArgs e) => ChangePage(wordlistsPage, menuOptionWordlists);
        private void OpenConfigsPage(object sender, MouseEventArgs e)
        {
            configsPage.UpdateViewModel();
            ChangePage(configsPage, menuOptionConfigs);
        }
        private void OpenHitsPage(object sender, MouseEventArgs e) { }
        private void OpenPluginsPage(object sender, MouseEventArgs e) => ChangePage(pluginsPage, menuOptionPlugins);
        private void OpenOBSettingsPage(object sender, MouseEventArgs e) => ChangePage(obSettingsPage, menuOptionOBSettings);
        private void OpenRLSettingsPage(object sender, MouseEventArgs e) => ChangePage(rlSettingsPage, menuOptionRLSettings);
        private void OpenAboutPage(object sender, MouseEventArgs e) => ChangePage(aboutPage, menuOptionAbout);

        private void OpenMetadataPage(object sender, MouseEventArgs e)
        {
            configMetadataPage.UpdateViewModel();
            ChangePage(configMetadataPage, menuOptionMetadata);
        }
        private void OpenReadmePage(object sender, MouseEventArgs e) { }
        private void OpenStackerPage(object sender, MouseEventArgs e) { }
        private void OpenLoliCodePage(object sender, MouseEventArgs e) { }
        private void OpenConfigSettingsPage(object sender, MouseEventArgs e) { }
        private void OpenCSharpCodePage(object sender, MouseEventArgs e) { }

        private void ChangePage(Page newPage, Label newLabel)
        {
            currentPage = newPage;
            mainFrame.Content = newPage;

            // Update the selected menu item
            foreach (var label in labels)
            {
                label.Foreground = Brush.Get("ForegroundMain");
            }

            newLabel.Foreground = Brush.Get("ForegroundMenuSelected");
        }

        private void TakeScreenshot(object sender, RoutedEventArgs e)
            => Screenshot.Take((int)Width, (int)Height, (int)Top, (int)Left);

        #region Dropdown submenu logic
        private void ConfigSubmenuMouseEnter(object sender, MouseEventArgs e)
        {
            if (vm.IsConfigSelected)
            {
                hoveringConfigSubmenu = true;
                configSubmenu.Visibility = Visibility.Visible;
            }
        }

        private async void ConfigSubmenuMouseLeave(object sender, MouseEventArgs e)
        {
            hoveringConfigSubmenu = false;
            await CheckCloseSubmenu();
        }

        private void ConfigsMenuOptionMouseEnter(object sender, MouseEventArgs e)
        {
            if (vm.IsConfigSelected)
            {
                hoveringConfigsMenuOption = true;
                configSubmenu.Visibility = Visibility.Visible;
            }
        }

        private async void ConfigsMenuOptionMouseLeave(object sender, MouseEventArgs e)
        {
            hoveringConfigsMenuOption = false;
            await CheckCloseSubmenu();
        }

        private async Task CheckCloseSubmenu()
        {
            await Task.Delay(50);

            if (!hoveringConfigSubmenu && !hoveringConfigsMenuOption)
            {
                configSubmenu.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ConfigService configService;
        public event Action<Config> ConfigSelected;

        public bool IsConfigSelected => configService.SelectedConfig != null;

        public MainWindowViewModel()
        {
            configService = SP.GetService<ConfigService>();
            configService.OnConfigSelected += (sender, config) =>
            {
                OnPropertyChanged(nameof(IsConfigSelected));
                ConfigSelected?.Invoke(config);
            };
        }
    }
}
