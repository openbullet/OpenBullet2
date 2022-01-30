using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using RuriLib.Functions.Captchas;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for RLSettings.xaml
    /// </summary>
    public partial class RLSettings : Page
    {
        private readonly RLSettingsViewModel vm;

        public RLSettings()
        {
            vm = SP.GetService<ViewModelsService>().RLSettings;
            vm.CaptchaServiceChanged += UpdateCaptchaTabControl;
            DataContext = vm;

            InitializeComponent();

            UpdateCaptchaTabControl(vm.CurrentCaptchaService);
            SetRTBContents();
        }

        private void CustomUserAgentsChanged(object sender, TextChangedEventArgs e)
            => vm.UserAgents = customUserAgentsListRTB.Lines().ToList();

        private void GlobalBanKeysChanged(object sender, TextChangedEventArgs e)
            => vm.GlobalBanKeys = globalBanKeysRTB.Lines().ToList();

        private void GlobalRetryKeysChanged(object sender, TextChangedEventArgs e)
            => vm.GlobalRetryKeys = globalRetryKeysRTB.Lines().ToList();

        private async void Save(object sender, RoutedEventArgs e) => await vm.Save();

        private void Reset(object sender, RoutedEventArgs e)
        {
            vm.Reset();
            SetRTBContents();
        }

        private async void CheckCaptchaBalance(object sender, RoutedEventArgs e)
        {
            try
            {
                var balance = await vm.CheckCaptchaBalance();
                Alert.Success("Success", $"Balance: {balance}");
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }

        private void UpdateCaptchaTabControl(CaptchaServiceType service)
        {
            var values = Enum.GetValues(typeof(CaptchaServiceType)).Cast<CaptchaServiceType>().ToList();
            var index = values.IndexOf(service);
            captchaServiceTabControl.SelectedIndex = index;
        }

        private void SetRTBContents()
        {
            customUserAgentsListRTB.Document.Blocks.Clear();
            customUserAgentsListRTB.AppendText(string.Join(Environment.NewLine, vm.UserAgents), Colors.White);

            globalBanKeysRTB.Document.Blocks.Clear();
            globalBanKeysRTB.AppendText(string.Join(Environment.NewLine, vm.GlobalBanKeys), Colors.White);

            globalRetryKeysRTB.Document.Blocks.Clear();
            globalRetryKeysRTB.AppendText(string.Join(Environment.NewLine, vm.GlobalRetryKeys), Colors.White);
        }
    }
}
