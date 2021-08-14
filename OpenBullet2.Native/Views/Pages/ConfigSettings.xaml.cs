using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigSettings.xaml
    /// </summary>
    public partial class ConfigSettings : Page
    {
        private readonly ConfigSettingsViewModel vm;

        public ConfigSettings()
        {
            vm = SP.GetService<ViewModelsService>().ConfigSettings;
            DataContext = vm;

            InitializeComponent();
            SetRTBContents();
        }

        public void UpdateViewModel() => vm.UpdateViewModel();

        private void BlockedUrlsChanged(object sender, TextChangedEventArgs e)
            => vm.BlockedUrls = blockedUrlsRTB.Lines().ToList();

        private void SetRTBContents()
        {
            blockedUrlsRTB.Document.Blocks.Clear();
            blockedUrlsRTB.AppendText(string.Join(Environment.NewLine, vm.BlockedUrls), Colors.White);
        }
    }
}
