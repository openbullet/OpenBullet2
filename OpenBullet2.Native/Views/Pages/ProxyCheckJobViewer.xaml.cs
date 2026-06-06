using OpenBullet2.Core.Services;
using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for ProxyCheckJobViewer.xaml
/// </summary>
public partial class ProxyCheckJobViewer : Page
{
    private readonly IUiFactory uiFactory;
    private readonly MainWindow mainWindow;
    private readonly OpenBulletSettingsService obSettingsService;
    private ProxyCheckJobViewerViewModel? vm;
    private ProxyCheckJobViewerViewModel ViewModel => vm
        ?? throw new InvalidOperationException("The job viewer has not been bound yet");

    public ProxyCheckJobViewer(
        IUiFactory uiFactory,
        MainWindow mainWindow,
        OpenBulletSettingsService obSettingsService)
    {
        this.uiFactory = uiFactory;
        this.mainWindow = mainWindow;
        this.obSettingsService = obSettingsService;
        InitializeComponent();
    }

    public void BindViewModel(ProxyCheckJobViewModel jobVM)
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

        vm = uiFactory.Create<ProxyCheckJobViewerViewModel>(jobVM);
        vm.NewMessage += OnResultMessage;
        DataContext = vm;
    }

    private async void Start(object sender, RoutedEventArgs e)
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() => jobLog.Clear());
            jobLog.BufferSize = obSettingsService.Settings.GeneralSettings.LogBufferSize;
            await ViewModel.Start();
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
            await ViewModel.Stop();
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
            await ViewModel.Pause();
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
            await ViewModel.Resume();
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
            await ViewModel.Abort();
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

    private void OnResultMessage(object? sender, string message, Color color)
        => Application.Current.Dispatcher.Invoke(() =>
        {
            if (obSettingsService.Settings.GeneralSettings.EnableJobLogging)
            {
                jobLog.Append(message, color);
            }
        });
}
