using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private readonly HomeViewModel vm;

        public Home()
        {
            InitializeComponent();

            vm = new HomeViewModel();
            DataContext = vm;
        }

        private void OpenInstallationTutorial(object sender, RoutedEventArgs e)
            => Url.Open("https://discourse.openbullet.dev/t/how-to-download-and-start-openbullet-2/29");

        private void ShowChangelog(object sender, RoutedEventArgs e)
            => new MainDialog(new ShowChangelogDialog(), "Changelog", true).ShowDialog();

        private void ShowUpdateConfirmation(object sender, RoutedEventArgs e)
            => new MainDialog(new UpdateConfirmationDialog(vm.CurrentVersion, vm.RemoteVersion), "Update confirmation").ShowDialog();
    }

    public class HomeViewModel : ViewModelBase
    {
        private readonly AnnouncementService annService;
        private readonly UpdateService updateService;

        public bool UpdateAvailable => updateService.IsUpdateAvailable;
        public Version CurrentVersion => updateService.CurrentVersion;
        public Version RemoteVersion => updateService.RemoteVersion;

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

        public HomeViewModel()
        {
            annService = SP.GetService<AnnouncementService>();
            updateService = SP.GetService<UpdateService>();

            updateService.UpdateAvailable += NotifyUpdateAvailable;

            FetchAnnouncement();
        }

        private async void FetchAnnouncement() => Announcement = await annService.FetchAnnouncement();

        private void NotifyUpdateAvailable()
        {
            OnPropertyChanged(nameof(UpdateAvailable));
            OnPropertyChanged(nameof(CurrentVersion));
            OnPropertyChanged(nameof(RemoteVersion));
        }
    }
}
