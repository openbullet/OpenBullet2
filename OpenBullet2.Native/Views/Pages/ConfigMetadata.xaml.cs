using Microsoft.Win32;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigMetadata.xaml
    /// </summary>
    public partial class ConfigMetadata : Page
    {
        private readonly ConfigMetadataViewModel vm;

        public ConfigMetadata()
        {
            vm = SP.GetService<ViewModelsService>().ConfigMetadata;
            DataContext = vm;

            InitializeComponent();
        }

        public void UpdateViewModel() => vm.UpdateViewModel();

        private void OpenIcon(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Images | *.ico;*.jpg;*.jpeg;*.png;*.bmp",
                FilterIndex = 1
            };

            ofd.ShowDialog();
            
            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                try
                {
                    vm.SetIconFromFile(ofd.FileName);
                }
                catch (Exception ex)
                {
                    Alert.Exception(ex);
                }
            }
        }

        private async void DownloadIcon(object sender, RoutedEventArgs e)
        {
            try
            {
                await vm.SetIconFromUrl(urlTextbox.Text);
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }
    }
}
