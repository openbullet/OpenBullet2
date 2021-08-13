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
        private readonly UpdateService updateService;
        private readonly MainWindowViewModel vm;

        private bool hoveringConfigsMenuOption;
        private bool hoveringConfigSubmenu;

        private readonly Label[] labels;

        private Home homePage;
        private Proxies proxiesPage;
        private Wordlists wordlistsPage;
        private Configs configsPage;
        private Views.Pages.ConfigMetadata configMetadataPage;
        private ConfigReadme configReadmePage;
        private OBSettings obSettingsPage;
        private RLSettings rlSettingsPage;
        private Plugins pluginsPage;
        private About aboutPage;
        
        public Page CurrentPage { get; private set; }

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

        public void NavigateTo(MainWindowPage page)
        {
            switch (page)
            {
                case MainWindowPage.Home:
                    homePage = new Home(); // We recreate the homepage each time to display updated announcements
                    ChangePage(homePage, menuOptionHome);
                    break;

                case MainWindowPage.Proxies:
                    if (proxiesPage is null) proxiesPage = new();
                    ChangePage(proxiesPage, menuOptionProxies);
                    break;

                case MainWindowPage.Wordlists:
                    if (wordlistsPage is null) wordlistsPage = new();
                    ChangePage(wordlistsPage, menuOptionWordlists);
                    break;

                case MainWindowPage.Configs:
                    if (configsPage is null) configsPage = new();
                    configsPage.UpdateViewModel();
                    ChangePage(configsPage, menuOptionConfigs);
                    break;

                case MainWindowPage.Plugins:
                    if (pluginsPage is null) pluginsPage = new();
                    ChangePage(pluginsPage, menuOptionPlugins);
                    break;

                case MainWindowPage.OBSettings:
                    if (obSettingsPage is null) obSettingsPage = new();
                    ChangePage(obSettingsPage, menuOptionOBSettings);
                    break;

                case MainWindowPage.RLSettings:
                    if (rlSettingsPage is null) rlSettingsPage = new();
                    ChangePage(rlSettingsPage, menuOptionRLSettings);
                    break;

                case MainWindowPage.About:
                    if (aboutPage is null) aboutPage = new();
                    ChangePage(aboutPage, menuOptionAbout);
                    break;

                // Initialize config pages when we click on them because a user might not even load them
                // so we save some RAM (especially the heavy ones that involve a WebBrowser control)

                case MainWindowPage.ConfigMetadata:
                    CloseSubmenu();
                    if (configMetadataPage is null) configMetadataPage = new();
                    configMetadataPage.UpdateViewModel();
                    ChangePage(configMetadataPage, menuOptionMetadata);
                    break;

                case MainWindowPage.ConfigReadme:
                    CloseSubmenu();
                    if (configReadmePage is null) configReadmePage = new();
                    configReadmePage.UpdateViewModel();
                    ChangePage(configReadmePage, menuOptionReadme);
                    break;
            }
        }

        private void OpenHomePage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.Home);
        private void OpenJobsPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.Jobs);
        private void OpenMonitorPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.Monitor);
        private void OpenProxiesPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.Proxies);
        private void OpenWordlistsPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.Wordlists);
        private void OpenConfigsPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.Configs);
        private void OpenHitsPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.Hits);
        private void OpenPluginsPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.Plugins);
        private void OpenOBSettingsPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.OBSettings);
        private void OpenRLSettingsPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.RLSettings);
        private void OpenAboutPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.About);

        private void OpenMetadataPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.ConfigMetadata);
        private void OpenReadmePage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.ConfigReadme);
        private void OpenStackerPage(object sender, MouseEventArgs e) { }
        private void OpenLoliCodePage(object sender, MouseEventArgs e) { }
        private void OpenConfigSettingsPage(object sender, MouseEventArgs e) { }
        private void OpenCSharpCodePage(object sender, MouseEventArgs e) { }

        private void ChangePage(Page newPage, Label newLabel)
        {
            CurrentPage = newPage;
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

        private void CloseSubmenu() => configSubmenu.Visibility = Visibility.Collapsed;
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

    public enum MainWindowPage
    {
        Home,
        Jobs,
        Monitor,
        Proxies,
        Wordlists,
        Configs,
        ConfigMetadata,
        ConfigReadme,
        ConfigStacker,
        ConfigLoliCode,
        ConfigSettings,
        ConfigCSharpCode,
        Hits,
        Plugins,
        OBSettings,
        RLSettings,
        About
    }
}
