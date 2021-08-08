using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for OBSettings.xaml
    /// </summary>
    public partial class OBSettings : Page
    {
        private readonly OBSettingsViewModel vm;

        public OBSettings()
        {
            vm = SP.GetService<ViewModelsService>().OBSettings;
            DataContext = vm;

            InitializeComponent();

            configSectionOnLoadCombobox.ItemsSource = Enum.GetValues(typeof(ConfigSection)).Cast<ConfigSection>();
            jobDisplayModeCombobox.ItemsSource = Enum.GetValues(typeof(JobDisplayMode)).Cast<JobDisplayMode>();
        }

        private async void Save(object sender, RoutedEventArgs e) => await vm.Save();
        private void Reset(object sender, RoutedEventArgs e) => vm.Reset();
        private void RemoveProxyCheckTarget(object sender, RoutedEventArgs e) => vm.RemoveProxyCheckTarget((ProxyCheckTarget)(sender as Button).Tag);
    }
}
