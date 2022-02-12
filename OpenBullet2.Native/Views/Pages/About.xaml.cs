using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Views.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Page
    {
        public About()
        {
            InitializeComponent();
        }

        private void OpenLicense(object sender, RoutedEventArgs e) => new MainDialog(new LicenseDialog(), "License", true).ShowDialog();

        private void OpenDonation(object sender, RoutedEventArgs e) => Url.Open("https://discourse.openbullet.dev/t/donations/3760");

        private void OpenRepository(object sender, RoutedEventArgs e) => Url.Open("https://github.com/openbullet/OpenBullet2");

        private void OpenForum(object sender, RoutedEventArgs e) => Url.Open("https://discourse.openbullet.dev/");

        private void OpenIssues(object sender, RoutedEventArgs e) => Url.Open("https://github.com/openbullet/OpenBullet2/issues");
    }
}
