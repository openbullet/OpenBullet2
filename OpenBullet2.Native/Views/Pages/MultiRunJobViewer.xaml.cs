using OpenBullet2.Core.Services;
using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for MultiRunJobViewer.xaml
/// </summary>
public partial class MultiRunJobViewer : Page
{
    private readonly IUiFactory uiFactory;
    private readonly MainWindow mainWindow;
    private readonly OpenBulletSettingsService obSettingsService;
    private readonly OpenBullet2.Native.ViewModels.DebuggerViewModel debuggerViewModel;
    private MultiRunJobViewerViewModel? vm;
    private GridViewColumnHeader? listViewSortCol;
    private SortAdorner? listViewSortAdorner;

    private IEnumerable<HitViewModel> SelectedHits => hitsListView.GetSelectedItemsInDisplayOrder<HitViewModel>();
    private MultiRunJobViewerViewModel ViewModel => vm
        ?? throw new InvalidOperationException("The job viewer has not been bound yet");

    public MultiRunJobViewer(
        IUiFactory uiFactory,
        MainWindow mainWindow,
        OpenBulletSettingsService obSettingsService,
        OpenBullet2.Native.ViewModels.DebuggerViewModel debuggerViewModel)
    {
        this.uiFactory = uiFactory;
        this.mainWindow = mainWindow;
        this.obSettingsService = obSettingsService;
        this.debuggerViewModel = debuggerViewModel;
        InitializeComponent();
    }

    public void BindViewModel(MultiRunJobViewModel jobVM)
    {
        if (vm is not null)
        {
            vm.Dispose();

            try
            {
                vm.NewMessage -= OnResultMessage;
            }
            catch
            {
            }
        }

        vm = uiFactory.Create<MultiRunJobViewerViewModel>(jobVM);
        vm.NewMessage += OnResultMessage;
        DataContext = vm;
    }

    private async void Start(object sender, RoutedEventArgs e)
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() => jobLog.Clear());
            jobLog.BufferSize = obSettingsService.Settings.GeneralSettings.LogBufferSize;
            await ViewModel.StartAsync();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private async void Stop(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.StopAsync();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private async void Pause(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.PauseAsync();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private async void Resume(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.ResumeAsync();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private async void Abort(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.AbortAsync();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private void SkipWait(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel.SkipWait();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private async void ChangeOptions(object sender, RoutedEventArgs e)
    {
        try
        {
            await mainWindow.EditJobAsync(ViewModel.Job);
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private void ChangeBots(object sender, MouseButtonEventArgs e)
        => new MainDialog(uiFactory.Create<ChangeBotsDialog>(this, ViewModel.Job.Bots), "Change bots").ShowDialog();

    public Task ChangeBotsAsync(int newValue) => ViewModel.ChangeBotsAsync(newValue);

    private void CopySelectedHits(object sender, RoutedEventArgs e)
        => SelectedHits.CopyToClipboard(h => h.Data);

    private void CopySelectedProxies(object sender, RoutedEventArgs e)
        => SelectedHits.CopyToClipboard(h => h.Proxy);

    private void CopySelectedHitsCapture(object sender, RoutedEventArgs e)
        => SelectedHits.CopyToClipboard(h => $"{h.Data} | {h.Capture}");

    private void SendToDebugger(object sender, RoutedEventArgs e)
    {
        var hitVM = SelectedHits.FirstOrDefault();

        if (hitVM is not null)
        {
            debuggerViewModel.TestData = hitVM.Data;

            if (hitVM.Hit.Proxy is not null)
            {
                debuggerViewModel.TestProxy = hitVM.Hit.Proxy.ToString();
                debuggerViewModel.ProxyType = hitVM.Hit.Proxy.Type;
            }
        }
    }

    private void SelectAll(object sender, RoutedEventArgs e) => hitsListView.SelectAll();

    private void ShowBotLog(object sender, RoutedEventArgs e)
    {
        var hitVM = SelectedHits.FirstOrDefault();

        if (hitVM is null)
        {
            return;
        }

        if (hitVM.Hit.Config.Mode == ConfigMode.DLL)
        {
            Alert.Error("Bot log unavailable", "The bot log is not available for pre-compiled configs");
            return;
        }

        if (hitVM.Hit.BotLogger is null)
        {
            Alert.Error("Bot log unavailable", "No bot log was captured for this hit");
            return;
        }

        new MainDialog(uiFactory.Create<BotLogDialog>(hitVM.Hit.BotLogger), $"Bot log for {hitVM.Data}").Show();
    }

    private void ColumnHeaderClicked(object sender, RoutedEventArgs e)
    {
        var column = sender as GridViewColumnHeader;
        var sortBy = column?.Tag?.ToString();

        if (string.IsNullOrEmpty(sortBy) || column is null)
        {
            return;
        }

        if (listViewSortCol != null)
        {
            AdornerLayer.GetAdornerLayer(listViewSortCol)?.Remove(listViewSortAdorner);
            botsListView.Items.SortDescriptions.Clear();
        }

        var newDir = ListSortDirection.Ascending;

        if (listViewSortCol == column && listViewSortAdorner?.Direction == newDir)
        {
            newDir = ListSortDirection.Descending;
        }

        listViewSortCol = column;
        listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
        AdornerLayer.GetAdornerLayer(listViewSortCol)?.Add(listViewSortAdorner);
        botsListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
    }

    private void LVIRightClick(object sender, MouseButtonEventArgs e)
    {
    }

    private void OnResultMessage(object? sender, string message, Color color)
        => Application.Current.Dispatcher.Invoke(() =>
        {
            if (obSettingsService.Settings.GeneralSettings.EnableJobLogging)
            {
                jobLog.Append(message, color);
            }
        });
}
