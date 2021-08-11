using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private HomeViewModel vm;

        public Home()
        {
            InitializeComponent();

            vm = new HomeViewModel();
            DataContext = vm;
        }

        private void OpenInstallationTutorial(object sender, RoutedEventArgs e) 
            => Url.Open("https://discourse.openbullet.dev/t/how-to-download-and-start-openbullet-2/29");

        private void ShowChangelog(object sender, RoutedEventArgs e)
            => new MainDialog(new ShowChangelogDialog(this), "Changelog").ShowDialog();
    }

    public class HomeViewModel : ViewModelBase
    {
        private AnnouncementService annService;

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

            _ = Task.Run(() => Announcement = annService.FetchAnnouncement().Result);
        }
    }
}
