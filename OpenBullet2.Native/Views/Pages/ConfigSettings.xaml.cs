using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Data.Rules;
using System;
using System.Linq;
using System.Windows;
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

        private void AddCustomInput(object sender, RoutedEventArgs e) => vm.AddCustomInput();
        private void RemoveCustomInput(object sender, RoutedEventArgs e)
            => vm.RemoveCustomInput((CustomInput)(sender as Button).Tag);

        private void AddLinesFromFileResource(object sender, RoutedEventArgs e) => vm.AddLinesFromFileResource();
        private void AddRandomLinesFromFileResource(object sender, RoutedEventArgs e) => vm.AddRandomLinesFromFileResource();
        private void RemoveResource(object sender, RoutedEventArgs e)
            => vm.RemoveResource((ConfigResourceOptions)(sender as Button).Tag);

        private void AddSimpleDataRule(object sender, RoutedEventArgs e) => vm.AddSimpleDataRule();
        private void AddRegexDataRule(object sender, RoutedEventArgs e) => vm.AddRegexDataRule();
        private void RemoveDataRule(object sender, RoutedEventArgs e)
            => vm.RemoveDataRule((DataRule)(sender as Button).Tag);

        private void SetRTBContents()
        {
            blockedUrlsRTB.Document.Blocks.Clear();
            blockedUrlsRTB.AppendText(string.Join(Environment.NewLine, vm.BlockedUrls), Colors.White);
        }

        private void TestDataRules(object sender, RoutedEventArgs e)
            => new MainDialog(new TestDataRulesDialog(vm.TestDataForRules, vm.TestWordlistTypeForRules, vm.DataRulesCollection), "Test Results").ShowDialog();
    }
}
