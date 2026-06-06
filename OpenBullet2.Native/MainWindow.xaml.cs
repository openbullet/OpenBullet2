using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.Logging;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.Utils;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
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
using System.Windows.Threading;

namespace OpenBullet2.Native;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MetroWindow
{
    private readonly ILogger<MainWindow> logger;
    private readonly IUiFactory uiFactory;
    private readonly UpdateService updateService;
    private readonly MainWindowViewModel vm;

    private bool hoveringConfigsMenuOption;
    private bool hoveringConfigSubmenu;
    private readonly DispatcherTimer toastTimer = new() { Interval = TimeSpan.FromSeconds(2.5) };

    private readonly TextBlock[] labels;

    private Home? homePage;
    private Jobs? jobsPage;
    private Monitor? monitorPage;
    private MultiRunJobViewer? multiRunJobViewerPage;
    private ProxyCheckJobViewer? proxyCheckJobViewerPage;
    private Proxies? proxiesPage;
    private Wordlists? wordlistsPage;
    private Configs? configsPage;
    private Views.Pages.ConfigMetadata? configMetadataPage;
    private ConfigReadme? configReadmePage;
    private ConfigEditor? configEditorPage;
    private Views.Pages.ConfigSettings? configSettingsPage;
    private Hits? hitsPage;
    private OBSettings? obSettingsPage;
    private RLSettings? rlSettingsPage;
    private Plugins? pluginsPage;
    private About? aboutPage;

    public Page? CurrentPage { get; private set; }

    public MainWindow(
        ILogger<MainWindow> logger,
        IUiFactory uiFactory,
        MainWindowViewModel vm,
        UpdateService updateService,
        OpenBulletSettingsService obSettingsService)
    {
        this.logger = logger;
        this.uiFactory = uiFactory;
        this.vm = vm;
        this.updateService = updateService;
        DataContext = vm;
        Closing += vm.OnWindowClosing;
        Loaded += OnLoaded;

        InitializeComponent();
        toastTimer.Tick += HideToast;

        labels =
        [
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
        ];

        Title = $"OpenBullet 2 - {updateService.CurrentVersion} [{updateService.CurrentVersionType}]";

        // Set the theme
        var customization = obSettingsService.Settings.CustomizationSettings;
        SetTheme(customization);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        logger.LogDebug("Main window loaded");

        // Preload heavier pages after the window is fully constructed to avoid circular resolution
        // when those pages depend back on MainWindow.
        configsPage ??= uiFactory.Create<Configs>();
        logger.LogDebug("Preloaded configs page");
    }

    public void NavigateTo(MainWindowPage page)
    {
        logger.LogDebug("Navigating to Native page {Page}", page);

        // Needed to save the content of the LoliCode editor when changing page
        if (CurrentPage == configEditorPage)
        {
            configEditorPage?.OnPageChanged();
        }

        switch (page)
        {
            case MainWindowPage.Home:
                homePage = uiFactory.Create<Home>(); // We recreate the homepage each time to display updated announcements
                ChangePage(homePage, menuOptionHome);
                break;

            case MainWindowPage.Jobs:
                jobsPage ??= uiFactory.Create<Jobs>();
                ChangePage(jobsPage, menuOptionJobs);
                break;

            case MainWindowPage.Monitor:
                monitorPage ??= uiFactory.Create<Monitor>();
                monitorPage.UpdateViewModel();
                ChangePage(monitorPage, menuOptionMonitor);
                break;

            case MainWindowPage.Proxies:
                proxiesPage ??= uiFactory.Create<Proxies>();
                proxiesPage.UpdateViewModel();
                ChangePage(proxiesPage, menuOptionProxies);
                break;

            case MainWindowPage.Wordlists:
                wordlistsPage ??= uiFactory.Create<Wordlists>();
                ChangePage(wordlistsPage, menuOptionWordlists);
                break;

            case MainWindowPage.Configs:
                configsPage ??= uiFactory.Create<Configs>();
                configsPage.UpdateViewModel();
                ChangePage(configsPage, menuOptionConfigs);
                break;

            case MainWindowPage.Hits:
                hitsPage ??= uiFactory.Create<Hits>();
                hitsPage.UpdateViewModel();
                ChangePage(hitsPage, menuOptionHits);
                break;

            case MainWindowPage.Plugins:
                pluginsPage ??= uiFactory.Create<Plugins>();
                ChangePage(pluginsPage, menuOptionPlugins);
                break;

            case MainWindowPage.OBSettings:
                obSettingsPage ??= uiFactory.Create<OBSettings>();
                ChangePage(obSettingsPage, menuOptionOBSettings);
                break;

            case MainWindowPage.RLSettings:
                rlSettingsPage ??= uiFactory.Create<RLSettings>();
                ChangePage(rlSettingsPage, menuOptionRLSettings);
                break;

            case MainWindowPage.About:
                aboutPage ??= uiFactory.Create<About>();
                ChangePage(aboutPage, menuOptionAbout);
                break;

            // Initialize config pages when we click on them because a user might not even load them
            // so we save some RAM (especially the heavy ones that involve a WebBrowser control)

            case MainWindowPage.ConfigMetadata:
                CloseSubmenu();
                configMetadataPage ??= uiFactory.Create<Views.Pages.ConfigMetadata>();
                configMetadataPage.UpdateViewModel();
                ChangePage(configMetadataPage, menuOptionMetadata);
                break;

            case MainWindowPage.ConfigReadme:
                CloseSubmenu();
                configReadmePage ??= uiFactory.Create<ConfigReadme>();
                configReadmePage.UpdateViewModel();
                ChangePage(configReadmePage, menuOptionReadme);
                break;

            case MainWindowPage.ConfigStacker:

                if (vm.Config.Mode is not ConfigMode.Stack and not ConfigMode.LoliCode)
                {
                    return;
                }

                CloseSubmenu();
                configEditorPage ??= uiFactory.Create<ConfigEditor>();
                configEditorPage.NavigateTo(ConfigEditorSection.Stacker);
                ChangePage(configEditorPage, menuOptionStacker);
                break;

            case MainWindowPage.ConfigLoliCode:

                if (vm.Config.Mode is not ConfigMode.Stack and not ConfigMode.LoliCode)
                {
                    return;
                }

                CloseSubmenu();
                configEditorPage ??= uiFactory.Create<ConfigEditor>();
                configEditorPage.NavigateTo(ConfigEditorSection.LoliCode);
                ChangePage(configEditorPage, menuOptionLoliCode);
                break;

            case MainWindowPage.ConfigSettings:
                CloseSubmenu();
                configSettingsPage ??= uiFactory.Create<Views.Pages.ConfigSettings>();
                configSettingsPage.UpdateViewModel();
                ChangePage(configSettingsPage, menuOptionConfigSettings);
                break;

            case MainWindowPage.ConfigCSharpCode:

                if (vm.Config.Mode is not ConfigMode.Stack and not ConfigMode.LoliCode and not ConfigMode.CSharp)
                {
                    return;
                }

                CloseSubmenu();
                configEditorPage ??= uiFactory.Create<ConfigEditor>();
                configEditorPage.NavigateTo(ConfigEditorSection.CSharp);
                ChangePage(configEditorPage, menuOptionCSharpCode);
                break;

            case MainWindowPage.ConfigLoliScript:

                if (vm.Config.Mode is not ConfigMode.Legacy)
                {
                    return;
                }

                CloseSubmenu();
                configEditorPage ??= uiFactory.Create<ConfigEditor>();
                configEditorPage.NavigateTo(ConfigEditorSection.LoliScript);
                ChangePage(configEditorPage, menuOptionLoliScript);
                break;
        }
    }

    public void DisplayJob(JobViewModel jobVM)
    {
        logger.LogDebug("Displaying job {JobId} in Native viewer", jobVM.Id);

        switch (jobVM)
        {
            case MultiRunJobViewModel mrj:
                multiRunJobViewerPage ??= uiFactory.Create<MultiRunJobViewer>();
                multiRunJobViewerPage.BindViewModel(mrj);
                ChangePage(multiRunJobViewerPage, null);
                break;

            case ProxyCheckJobViewModel pcj:
                proxyCheckJobViewerPage ??= uiFactory.Create<ProxyCheckJobViewer>();
                proxyCheckJobViewerPage.BindViewModel(pcj);
                ChangePage(proxyCheckJobViewerPage, null);
                break;

            default:
                break;
        }
    }

    public Task EditJobAsync(JobViewModel jobVM)
    {
        NavigateTo(MainWindowPage.Jobs);
        return jobsPage!.EditJobAsync(jobVM);
    }

    public void ShowToast(AlertType type, string title, string message)
    {
        logger.LogDebug("Showing toast {AlertType} with title {Title}", type, title);
        Dispatcher.Invoke(() =>
        {
            toastTimer.Stop();

            toastTitle.Text = title;
            toastMessage.Text = message;

            var accentColor = type switch
            {
                AlertType.Success => System.Windows.Media.Colors.YellowGreen,
                AlertType.Warning => System.Windows.Media.Colors.Orange,
                AlertType.Error => System.Windows.Media.Colors.Tomato,
                AlertType.Info => System.Windows.Media.Colors.SkyBlue,
                _ => System.Windows.Media.Colors.SkyBlue
            };

            toastIcon.Kind = type switch
            {
                AlertType.Success => PackIconOcticonsKind.Check,
                AlertType.Warning => PackIconOcticonsKind.Alert,
                AlertType.Error => PackIconOcticonsKind.X,
                AlertType.Info => PackIconOcticonsKind.Info,
                _ => PackIconOcticonsKind.Info
            };

            toastIcon.Foreground = new System.Windows.Media.SolidColorBrush(accentColor);
            toastContainer.BorderBrush = new System.Windows.Media.SolidColorBrush(accentColor);
            toastContainer.Visibility = Visibility.Visible;

            toastTimer.Start();
        });
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

    private void ChangePage(Page newPage, TextBlock? newLabel)
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

    private void HideToast(object? sender, EventArgs e)
    {
        toastTimer.Stop();
        toastContainer.Visibility = Visibility.Collapsed;
    }

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
        await CheckCloseSubmenuAsync();
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
        await CheckCloseSubmenuAsync();
    }

    private async Task CheckCloseSubmenuAsync()
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
    private readonly ILogger<MainWindowViewModel> logger;
    private readonly OpenBulletSettingsService obSettingsService;
    private readonly JobManagerService jobManagerService;
    private readonly ConfigService configService;
    public event Action<Config>? ConfigSelected;
    public Config Config => configService.SelectedConfig;

    public bool IsConfigSelected => Config != null;

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        OpenBulletSettingsService obSettingsService,
        JobManagerService jobManagerService,
        ConfigService configService)
    {
        this.logger = logger;
        this.obSettingsService = obSettingsService;
        this.jobManagerService = jobManagerService;
        this.configService = configService;
        configService.OnConfigSelected += (sender, config) =>
        {
            OnPropertyChanged(nameof(IsConfigSelected));
            ConfigSelected?.Invoke(config);
        };
    }

    public void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        // Check if the config was saved
        if (obSettingsService.Settings.GeneralSettings.WarnConfigNotSaved && Config != null && Config.HasUnsavedChanges())
        {
            logger.LogInformation("Prompting before closing Native window because config {ConfigId} has unsaved changes",
                Config.Id);
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
            logger.LogInformation("Prompting before closing Native window because {RunningJobs} job(s) are still running",
                jobManagerService.Jobs.Count(j => j.Status != JobStatus.Idle));
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
