using OpenBullet2.Core.Services;
using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for MultiRunJobViewer.xaml
    /// </summary>
    public partial class MultiRunJobViewer : Page
    {
        private readonly MainWindow mainWindow;
        private readonly OpenBulletSettingsService obSettingsService;
        private MultiRunJobViewerViewModel vm;
        private GridViewColumnHeader listViewSortCol;
        private SortAdorner listViewSortAdorner;

        private IEnumerable<HitViewModel> SelectedHits => hitsListView.SelectedItems.Cast<HitViewModel>().ToList();

        public MultiRunJobViewer()
        {
            mainWindow = SP.GetService<MainWindow>();
            obSettingsService = SP.GetService<OpenBulletSettingsService>();
            InitializeComponent();
        }

        public void BindViewModel(MultiRunJobViewModel jobVM)
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

            vm = new MultiRunJobViewerViewModel(jobVM);
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

        private void ChangeOptions(object sender, RoutedEventArgs e) => mainWindow.EditJob(vm.Job);

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

        private void CopySelectedHits(object sender, RoutedEventArgs e)
            => SelectedHits.CopyToClipboard(h => h.Data);

        private void CopySelectedProxies(object sender, RoutedEventArgs e)
            => SelectedHits.CopyToClipboard(h => h.Proxy);

        private void CopySelectedHitsCapture(object sender, RoutedEventArgs e)
            => SelectedHits.CopyToClipboard(h => $"{h.Data} | {h.Capture}");

        private void SendToDebugger(object sender, RoutedEventArgs e)
        {
            var hitVM = SelectedHits.FirstOrDefault();

            if (hitVM is not null)
            {
                var debugger = SP.GetService<ViewModelsService>().Debugger;
                debugger.TestData = hitVM.Data;

                if (hitVM.Hit.Proxy is not null)
                {
                    debugger.TestProxy = hitVM.Hit.Proxy.ToString();
                    debugger.ProxyType = hitVM.Hit.Proxy.Type;
                }
            }
        }

        private void SelectAll(object sender, RoutedEventArgs e) => hitsListView.SelectAll();

        private void ShowBotLog(object sender, RoutedEventArgs e)
        {
            var hitVM = SelectedHits.FirstOrDefault();

            if (hitVM is null) return;

            if (hitVM.Hit.Config.Mode == ConfigMode.DLL)
            {
                Alert.Error("Bot log unavailable", "The bot log is not available for pre-compiled configs");
                return;
            }
            
            new MainDialog(new BotLogDialog(hitVM.Hit.BotLogger), $"Bot log for {hitVM.Data}").Show();
        }

        private void ColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            var column = sender as GridViewColumnHeader;
            var sortBy = column.Tag.ToString();

            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                botsListView.Items.SortDescriptions.Clear();
            }

            var newDir = ListSortDirection.Ascending;

            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
            {
                newDir = ListSortDirection.Descending;
            }

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            botsListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void LVIRightClick(object sender, MouseButtonEventArgs e)
        {
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