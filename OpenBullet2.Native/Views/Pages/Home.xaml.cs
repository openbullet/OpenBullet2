using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using OpenBullet2.Core.Models.Settings;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for Home.xaml
/// </summary>
public partial class Home : Page
{
    private readonly IUiFactory uiFactory;
    private readonly HomeViewModel vm;

    public Home(IUiFactory uiFactory, HomeViewModel vm)
    {
        this.uiFactory = uiFactory;
        this.vm = vm;
        InitializeComponent();
        DataContext = vm;
    }

    private void OpenInstallationTutorial(object sender, RoutedEventArgs e)
        => Url.Open("https://discourse.openbullet.dev/t/how-to-download-and-start-openbullet-2/29");

    private void ShowChangelog(object sender, RoutedEventArgs e)
        => new MainDialog(uiFactory.Create<ShowChangelogDialog>(), "Changelog", true).ShowDialog();

    private void ShowUpdateConfirmation(object sender, RoutedEventArgs e)
        => new MainDialog(uiFactory.Create<UpdateConfirmationDialog>(
            vm.CurrentVersion, vm.RemoteVersion, vm.UpdateChannel), "Update confirmation").ShowDialog();
}

public class HomeViewModel : ViewModelBase
{
    private readonly AnnouncementService annService;
    private readonly UpdateService updateService;

    public bool UpdateAvailable => updateService.IsUpdateAvailable;
    public Version CurrentVersion => updateService.CurrentVersion;
    public Version RemoteVersion => updateService.RemoteVersion;
    public UpdateChannel UpdateChannel => updateService.UpdateChannel;

    private string announcement = "Loading announcement...";
    public string Announcement
    {
        get => announcement;
        set
        {
            announcement = value;
            OnPropertyChanged();
        }
    }

    public HomeViewModel(AnnouncementService annService, UpdateService updateService)
    {
        this.annService = annService;
        this.updateService = updateService;

        updateService.UpdateAvailable += NotifyUpdateAvailable;

        _ = FetchAnnouncementAsync();
    }

    private async Task FetchAnnouncementAsync() => Announcement = await annService.FetchAnnouncementAsync();

    private void NotifyUpdateAvailable()
    {
        OnPropertyChanged(nameof(UpdateAvailable));
        OnPropertyChanged(nameof(CurrentVersion));
        OnPropertyChanged(nameof(RemoteVersion));
        OnPropertyChanged(nameof(UpdateChannel));
    }
}
