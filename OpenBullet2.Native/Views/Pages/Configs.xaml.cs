using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
using Microsoft.Extensions.Logging;
using OpenBullet2.Native.DTOs;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for Configs.xaml
/// </summary>
public partial class Configs : Page
{
    private readonly ILogger<Configs> logger;
    private readonly IUiFactory uiFactory;
    private readonly MainWindow mainWindow;
    private readonly DebuggerViewModel debuggerViewModel;
    private readonly OpenBulletSettingsService obSettingsService;
    private readonly ConfigService configService;
    private readonly ConfigsViewModel vm;
    private readonly VolatileSettingsService volatileSettings;
    private GridViewColumnHeader? listViewSortCol;
    private SortAdorner? listViewSortAdorner;

    private ConfigViewModel? HoveredItem => configsListView.SelectedItem as ConfigViewModel;

    private string ListViewSortBy
    {
        get => volatileSettings.ListViewSorting["configs"].By;
        set => volatileSettings.ListViewSorting["configs"].By = value;
    }

    private ListSortDirection ListViewSortDir
    {
        get => volatileSettings.ListViewSorting["configs"].Direction;
        set => volatileSettings.ListViewSorting["configs"].Direction = value;
    }

    public Configs(
        ILogger<Configs> logger,
        IUiFactory uiFactory,
        MainWindow mainWindow,
        DebuggerViewModel debuggerViewModel,
        OpenBulletSettingsService obSettingsService,
        ConfigService configService,
        VolatileSettingsService volatileSettings,
        ConfigsViewModel vm)
    {
        this.logger = logger;
        this.uiFactory = uiFactory;
        this.mainWindow = mainWindow;
        this.debuggerViewModel = debuggerViewModel;
        this.obSettingsService = obSettingsService;
        this.configService = configService;
        this.volatileSettings = volatileSettings;
        this.vm = vm;
        DataContext = vm;

        InitializeComponent();
    }

    // This is needed otherwise if properties of a config are updated by another page this page will not
    // get notified and will show the old values.
    public void UpdateViewModel()
    {
        vm.SelectedConfig?.UpdateViewModel();
        configsListView.Items.SortDescriptions.Clear();

        if (!string.IsNullOrEmpty(ListViewSortBy))
        {
            configsListView.Items.SortDescriptions.Add(new SortDescription(ListViewSortBy, ListViewSortDir));
        }
    }

    private void Create(object sender, RoutedEventArgs e)
        => new MainDialog(uiFactory.Create<CreateConfigDialog>(this), "Create config").ShowDialog();

    public Task CreateConfigAsync(ConfigForCreationDto dto) => vm.CreateAsync(dto);

    private void Edit(object sender, RoutedEventArgs e) => EditConfig();

    private async void Save(object sender, RoutedEventArgs e)
    {
        if (vm.SelectedConfig is null)
        {
            ShowNoConfigSelectedError();
            return;
        }

        try
        {
            await vm.Save(vm.SelectedConfig);
            Alert.ToastSuccess("Saved", $"{vm.SelectedConfig.Config.Metadata.Name} was saved successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save selected config from Native configs page");
            Alert.Exception(ex);
        }
    }

    private void Delete(object sender, RoutedEventArgs e)
    {
        if (HoveredItem is null)
        {
            ShowNoConfigSelectedError();
            return;
        }

        if (Alert.Choice("Are you sure?", $"Do you really want to delete {HoveredItem.Name}"))
        {
            vm.Delete(HoveredItem);
        }
    }

    // TODO: Check if current config is not saved and prompt warning
    private async void Rescan(object sender, RoutedEventArgs e) => await vm.RescanAsync();

    private void OpenFolder(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start("explorer.exe", Path.Combine(Directory.GetCurrentDirectory(), "UserData\\Configs"));
        }
        catch (Exception ex)
        {
            // This happens on access denied
            logger.LogError(ex, "Failed to open configs folder");
            Alert.Exception(ex);
        }
    }

    private void ShowNoConfigSelectedError() => Alert.Error("No config selected", "Please select a config first!");

    private void NavigateToConfigSection()
    {
        if (vm.SelectedConfig is null)
        {
            ShowNoConfigSelectedError();
            return;
        }

        var mode = vm.SelectedConfig.Config.Mode;
        var page = obSettingsService.Settings.GeneralSettings.ConfigSectionOnLoad switch
        {
            ConfigSection.Metadata => MainWindowPage.ConfigMetadata,
            ConfigSection.Readme => MainWindowPage.ConfigReadme,
            ConfigSection.Stacker => mode switch
            {
                ConfigMode.LoliCode or ConfigMode.Stack => MainWindowPage.ConfigStacker,
                ConfigMode.CSharp => MainWindowPage.ConfigCSharpCode,
                ConfigMode.Legacy => MainWindowPage.ConfigLoliScript,
                _ => MainWindowPage.ConfigMetadata
            },
            ConfigSection.LoliCode => mode switch
            {
                ConfigMode.LoliCode or ConfigMode.Stack => MainWindowPage.ConfigLoliCode,
                ConfigMode.CSharp => MainWindowPage.ConfigCSharpCode,
                ConfigMode.Legacy => MainWindowPage.ConfigLoliScript,
                _ => MainWindowPage.ConfigMetadata
            },
            ConfigSection.Settings => MainWindowPage.ConfigSettings,
            ConfigSection.CSharpCode => mode switch
            {
                ConfigMode.LoliCode or ConfigMode.Stack or ConfigMode.CSharp => MainWindowPage.ConfigLoliCode,
                ConfigMode.Legacy => MainWindowPage.ConfigLoliScript,
                _ => MainWindowPage.ConfigMetadata
            },
            ConfigSection.LoliScript => mode switch
            {
                ConfigMode.LoliCode or ConfigMode.Stack => MainWindowPage.ConfigLoliCode,
                ConfigMode.CSharp => MainWindowPage.ConfigCSharpCode,
                ConfigMode.Legacy => MainWindowPage.ConfigLoliScript,
                _ => MainWindowPage.ConfigMetadata
            },
            _ => throw new NotImplementedException(),
        };

        mainWindow.NavigateTo(page);
    }

    private void UpdateSearch(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            vm.SearchString = filterTextbox.Text;
        }
    }

    private void Search(object sender, RoutedEventArgs e) => vm.SearchString = filterTextbox.Text;

    private void ItemHovered(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 1)
        {
            vm.HoveredConfig = e.AddedItems[0] as ConfigViewModel;
        }
    }

    private void ListItemDoubleClick(object sender, MouseButtonEventArgs e) => EditConfig();

    private void EditConfig()
    {
        if (HoveredItem is null)
        {
            ShowNoConfigSelectedError();
            return;
        }

        if (HoveredItem.Config.IsRemote)
        {
            logger.LogWarning("Attempted to edit remote config {ConfigId}", HoveredItem.Id);
            Alert.Error("Remote", "You cannot edit remote configs!");
            return;
        }

        // Check if the config was saved
        if (obSettingsService.Settings.GeneralSettings.WarnConfigNotSaved
            && configService.SelectedConfig != null
            && configService.SelectedConfig.HasUnsavedChanges()
            && !Alert.Choice("Config not saved", $"The currently selected config ({configService.SelectedConfig.Metadata.Name}) has unsaved changes," +
                $" are you sure you want to edit another config?"))
        {
            return;
        }

        vm.SelectedConfig = HoveredItem;
        logger.LogInformation("Selected config {ConfigId} ({ConfigName}) for editing", HoveredItem.Id, HoveredItem.Name);

        if (obSettingsService.Settings.GeneralSettings.WarnDangerousConfig
            && HoveredItem.Config.HasCSharpCode())
        {
            Alert.ToastWarning("Potentially dangerous config", "The config you selected might have some C# code in it" +
                " (or blocks that call external programs). Do not run it unless you trust the source.");
        }

        debuggerViewModel.ClearLog();
        NavigateToConfigSection();
    }

    private void ColumnHeaderClicked(object sender, RoutedEventArgs e)
    {
        var column = sender as GridViewColumnHeader;
        var sortBy = column?.Tag?.ToString();

        if (string.IsNullOrEmpty(sortBy) || column is null)
        {
            return;
        }

        ListViewSortBy = sortBy;

        if (listViewSortCol != null)
        {
            AdornerLayer.GetAdornerLayer(listViewSortCol)?.Remove(listViewSortAdorner);
            configsListView.Items.SortDescriptions.Clear();
        }

        ListViewSortDir = ListSortDirection.Ascending;

        if (listViewSortCol == column && listViewSortAdorner?.Direction == ListViewSortDir)
        {
            ListViewSortDir = ListSortDirection.Descending;
        }

        listViewSortCol = column;
        listViewSortAdorner = new SortAdorner(listViewSortCol, ListViewSortDir);
        AdornerLayer.GetAdornerLayer(listViewSortCol)?.Add(listViewSortAdorner);
        configsListView.Items.SortDescriptions.Add(new SortDescription(ListViewSortBy, ListViewSortDir));
    }
}
