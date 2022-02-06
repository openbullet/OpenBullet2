using OpenBullet2.Core.Services;
using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ProxyCheckJobViewer.xaml
    /// </summary>
    public partial class ProxyCheckJobViewer : Page
    {
        private readonly OpenBulletSettingsService obSettingsService;
        private ProxyCheckJobViewerViewModel vm;

        public ProxyCheckJobViewer()
        {
            obSettingsService = SP.GetService<OpenBulletSettingsService>();
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

            vm = new ProxyCheckJobViewerViewModel(jobVM);
            vm.NewMessage += OnResultMessage;
            DataContext = vm;
        }

        private async void Start(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => jobLog.Clear());
                jobLog.BufferSize = obSettingsService.Settings.GeneralSettings.LogBufferSize;
                await vm.Start();
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
                await vm.Stop();
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
                await vm.Pause();
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
                await vm.Resume();
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
                await vm.Abort();
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
                vm.SkipWait();
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }

        private void ChangeBots(object sender, MouseButtonEventArgs e)
            => new MainDialog(new ChangeBotsDialog(this, vm.Job.Bots), "Change bots").ShowDialog();

        public async void ChangeBots(int newValue)
        {
            try
            {
                await vm.ChangeBots(newValue);
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }

        private void OnResultMessage(object sender, string message, Color color)
            => Application.Current.Dispatcher.Invoke(() =>
            {
                if (obSettingsService.Settings.GeneralSettings.EnableJobLogging)
                {
                    jobLog.Append(message, color);
                }
            });
    }
}
