using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using System;
using System.ComponentModel;
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
        private MultiRunJobViewerViewModel vm;
        private GridViewColumnHeader listViewSortCol;
        private SortAdorner listViewSortAdorner;

        public MultiRunJobViewer()
        {
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

        private void CopySelectedHits(object sender, RoutedEventArgs e) { }
        private void CopySelectedProxies(object sender, RoutedEventArgs e) { }
        private void CopySelectedHitsCapture(object sender, RoutedEventArgs e) { }
        private void SendToDebugger(object sender, RoutedEventArgs e) { }
        private void SelectAll(object sender, RoutedEventArgs e) { }
        private void ShowBotLog(object sender, RoutedEventArgs e) { }

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
                if (vm.EnableJobLog)
                {
                    // TODO: Add buffering for the logs otherwise this is calling for trouble!!!
                    jobLogRTB.AppendText(message + Environment.NewLine, color);
                    jobLogRTB.ScrollToEnd();
                }
            });
    }
}
