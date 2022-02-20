using MahApps.Metro.Controls;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.Utils;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Pages;
using RuriLib.Models.Configs;
using RuriLib.Models.Jobs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        private readonly TextBlock[] labels;

        private Home homePage;
        private Jobs jobsPage;
        private Monitor monitorPage;
        private MultiRunJobViewer multiRunJobViewerPage;
        private ProxyCheckJobViewer proxyCheckJobViewerPage;
        private Proxies proxiesPage;
        private Wordlists wordlistsPage;
        private Configs configsPage;
        private Views.Pages.ConfigMetadata configMetadataPage;
        private ConfigReadme configReadmePage;
        private ConfigEditor configEditorPage;
        private Views.Pages.ConfigSettings configSettingsPage;
        private Hits hitsPage;
        private OBSettings obSettingsPage;
        private RLSettings rlSettingsPage;
        private Plugins pluginsPage;
        private About aboutPage;
        
        public Page CurrentPage { get; private set; }

        public MainWindow()
        {
            vm = new MainWindowViewModel();
            DataContext = vm;
            Closing += vm.OnWindowClosing;

            InitializeComponent();

            labels = new TextBlock[]
            {
                menuOptionAbout,
                menuOptionConfigs,
                menuOptionConfigSettings,
                menuOptionCSharpCode,
                menuOptionHits,
                menuOptionHome,
                menuOptionJobs,
                menuOptionLoliCode,
                menuOptionLoliScript,
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

            // Pages to initialize as soon as the program starts. This is done to reduce the loading time
            // when clicking on them, as it can be frustrating for the user on specific pages.
            configsPage = new();

            updateService = SP.GetService<UpdateService>();
            Title = $"OpenBullet 2 - {updateService.CurrentVersion} [{updateService.CurrentVersionType}]";

            // Set the theme
            var obSettingsService = SP.GetService<OpenBulletSettingsService>();
            var customization = obSettingsService.Settings.CustomizationSettings;
            SetTheme(customization);
        }

        public void NavigateTo(MainWindowPage page)
        {
            // Needed to save the content of the LoliCode editor when changing page
            if (CurrentPage == configEditorPage)
            {
                configEditorPage?.OnPageChanged();
            }

            switch (page)
            {
                case MainWindowPage.Home:
                    homePage = new Home(); // We recreate the homepage each time to display updated announcements
                    ChangePage(homePage, menuOptionHome);
                    break;

                case MainWindowPage.Jobs:
                    if (jobsPage is null) jobsPage = new();
                    ChangePage(jobsPage, menuOptionJobs);
                    break;

                case MainWindowPage.Monitor:
                    if (monitorPage is null) monitorPage = new();
                    ChangePage(monitorPage, menuOptionMonitor);
                    break;

                case MainWindowPage.Proxies:
                    if (proxiesPage is null) proxiesPage = new();
                    proxiesPage.UpdateViewModel();
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

                case MainWindowPage.Hits:
                    if (hitsPage is null) hitsPage = new();
                    hitsPage.UpdateViewModel();
                    ChangePage(hitsPage, menuOptionHits);
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

                case MainWindowPage.ConfigStacker:

                    if (vm.Config.Mode is not ConfigMode.Stack and not ConfigMode.LoliCode)
                    {
                        return;
                    }

                    CloseSubmenu();
                    if (configEditorPage is null) configEditorPage = new();
                    configEditorPage.NavigateTo(ConfigEditorSection.Stacker);
                    ChangePage(configEditorPage, menuOptionStacker);
                    break;

                case MainWindowPage.ConfigLoliCode:

                    if (vm.Config.Mode is not ConfigMode.Stack and not ConfigMode.LoliCode)
                    {
                        return;
                    }

                    CloseSubmenu();
                    if (configEditorPage is null) configEditorPage = new();
                    configEditorPage.NavigateTo(ConfigEditorSection.LoliCode);
                    ChangePage(configEditorPage, menuOptionLoliCode);
                    break;

                case MainWindowPage.ConfigSettings:
                    CloseSubmenu();
                    if (configSettingsPage is null) configSettingsPage = new();
                    configSettingsPage.UpdateViewModel();
                    ChangePage(configSettingsPage, menuOptionConfigSettings);
                    break;

                case MainWindowPage.ConfigCSharpCode:

                    if (vm.Config.Mode is not ConfigMode.Stack and not ConfigMode.LoliCode and not ConfigMode.CSharp)
                    {
                        return;
                    }

                    CloseSubmenu();
                    if (configEditorPage is null) configEditorPage = new();
                    configEditorPage.NavigateTo(ConfigEditorSection.CSharp);
                    ChangePage(configEditorPage, menuOptionCSharpCode);
                    break;

                case MainWindowPage.ConfigLoliScript:

                    if (vm.Config.Mode is not ConfigMode.Legacy)
                    {
                        return;
                    }

                    CloseSubmenu();
                    if (configEditorPage is null) configEditorPage = new();
                    configEditorPage.NavigateTo(ConfigEditorSection.LoliScript);
                    ChangePage(configEditorPage, menuOptionLoliScript);
                    break;
            }
        }

        public void DisplayJob(JobViewModel jobVM)
        {
            switch (jobVM)
            {
                case MultiRunJobViewModel mrj:
                    if (multiRunJobViewerPage is null) multiRunJobViewerPage = new();
                    multiRunJobViewerPage.BindViewModel(mrj);
                    ChangePage(multiRunJobViewerPage, null);
                    break;

                case ProxyCheckJobViewModel pcj:
                    if (proxyCheckJobViewerPage is null) proxyCheckJobViewerPage = new();
                    proxyCheckJobViewerPage.BindViewModel(pcj);
                    ChangePage(proxyCheckJobViewerPage, null);
                    break;

                default:
                    break;
            }
        }

        public void EditJob(JobViewModel jobVM)
        {
            NavigateTo(MainWindowPage.Jobs);
            jobsPage.EditJob(jobVM);
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
        private void OpenStackerPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.ConfigStacker);
        private void OpenLoliCodePage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.ConfigLoliCode);
        private void OpenConfigSettingsPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.ConfigSettings);
        private void OpenCSharpCodePage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.ConfigCSharpCode);
        private void OpenLoliScriptPage(object sender, MouseEventArgs e) => NavigateTo(MainWindowPage.ConfigLoliScript);

        private void ChangePage(Page newPage, TextBlock newLabel)
        {
            CurrentPage = newPage;
            mainFrame.Content = newPage;

            // Update the selected menu item
            foreach (var label in labels)
            {
                label.Foreground = Brush.Get("ForegroundMain");
            }

            if (newLabel is not null)
            {
                newLabel.Foreground = Brush.Get("ForegroundMenuSelected");
            }
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

        public void SetTheme(CustomizationSettings customization)
        {
            Brush.SetAppColor("BackgroundMain", customization.BackgroundMain);
            Brush.SetAppColor("BackgroundSecondary", customization.BackgroundSecondary);
            Brush.SetAppColor("BackgroundInput", customization.BackgroundInput);
            Brush.SetAppColor("ForegroundMain", customization.ForegroundMain);
            Brush.SetAppColor("ForegroundInput", customization.ForegroundInput);
            Brush.SetAppColor("ForegroundGood", customization.ForegroundGood);
            Brush.SetAppColor("ForegroundBad", customization.ForegroundBad);
            Brush.SetAppColor("ForegroundCustom", customization.ForegroundCustom);
            Brush.SetAppColor("ForegroundRetry", customization.ForegroundRetry);
            Brush.SetAppColor("ForegroundBanned", customization.ForegroundBanned);
            Brush.SetAppColor("ForegroundToCheck", customization.ForegroundToCheck);
            Brush.SetAppColor("ForegroundMenuSelected", customization.ForegroundMenuSelected);
            Brush.SetAppColor("SuccessButton", customization.SuccessButton);
            Brush.SetAppColor("PrimaryButton", customization.PrimaryButton);
            Brush.SetAppColor("WarningButton", customization.WarningButton);
            Brush.SetAppColor("DangerButton", customization.DangerButton);
            Brush.SetAppColor("ForegroundButton", customization.ForegroundButton);
            Brush.SetAppColor("BackgroundButton", customization.BackgroundButton);

            // BACKGROUND
            if (File.Exists(customization.BackgroundImagePath))
            {
                Background = new System.Windows.Media.ImageBrush(
                    new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(customization.BackgroundImagePath)))
                {
                    Opacity = customization.BackgroundOpacity / 100,
                    Stretch = System.Windows.Media.Stretch.UniformToFill
                };
            }
            else
            {
                Background = Brush.Get("BackgroundMain");
            }
        }
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly OpenBulletSettingsService obSettingsService;
        private readonly JobManagerService jobManagerService;
        private readonly ConfigService configService;
        public event Action<Config> ConfigSelected;
        public Config Config => configService.SelectedConfig;

        public bool IsConfigSelected => Config != null;

        public MainWindowViewModel()
        {
            obSettingsService = SP.GetService<OpenBulletSettingsService>();
            jobManagerService = SP.GetService<JobManagerService>();
            configService = SP.GetService<ConfigService>();
            configService.OnConfigSelected += (sender, config) =>
            {
                OnPropertyChanged(nameof(IsConfigSelected));
                ConfigSelected?.Invoke(config);
            };
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Check if the config was saved
            if (obSettingsService.Settings.GeneralSettings.WarnConfigNotSaved && Config != null && Config.HasUnsavedChanges())
            {
                e.Cancel = !Alert.Choice("Config not saved", $"The config you are editing ({Config.Metadata.Name}) has unsaved changes, are you sure you want to quit?");
            }

            // If already cancelled, we don't need to check any other condition
            if (e.Cancel)
            {
                return;
            }

            // Check if there are running jobs
            if (jobManagerService.Jobs.Any(j => j.Status != JobStatus.Idle))
            {
                e.Cancel = !Alert.Choice("Job(s) running", "One or more jobs are still running, are you sure you want to quit?");
            }
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
        ConfigLoliScript,
        Hits,
        Plugins,
        OBSettings,
        RLSettings,
        About
    }
}
