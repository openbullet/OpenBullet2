using OpenBullet2.Core.Entities;
using OpenBullet2.Native.DTOs;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Proxies;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for Proxies.xaml
    /// </summary>
    public partial class Proxies : Page
    {
        private readonly ProxiesViewModel vm;
        private readonly MainWindow window;
        private GridViewColumnHeader listViewSortCol;
        private SortAdorner listViewSortAdorner;

        public Proxies()
        {
            vm = SP.GetService<ViewModelsService>().Proxies;
            DataContext = vm;
            _ = vm.Initialize();

            InitializeComponent();
            window = SP.GetService<MainWindow>();
        }

        private async void AddGroup(object sender, RoutedEventArgs e) => throw new NotImplementedException();
        private async void EditGroup(object sender, RoutedEventArgs e) => throw new NotImplementedException();
        private async void DeleteGroup(object sender, RoutedEventArgs e) => throw new NotImplementedException();
        private async void DeleteNotWorking(object sender, RoutedEventArgs e) => throw new NotImplementedException();
        private async void DeleteDuplicates(object sender, RoutedEventArgs e) => throw new NotImplementedException();
        private async void DeleteUntested(object sender, RoutedEventArgs e) => throw new NotImplementedException();
        private async void Import(object sender, RoutedEventArgs e) => throw new NotImplementedException();
        private async void ExportSelected(object sender, RoutedEventArgs e) => throw new NotImplementedException();
        private async void CopySelectedProxies(object sender, RoutedEventArgs e) => throw new NotImplementedException();
        private async void CopySelectedProxiesFull(object sender, RoutedEventArgs e) => throw new NotImplementedException();

        private async void DeleteSelected(object sender, RoutedEventArgs e)
        {
            foreach (var wordlist in proxiesListView.SelectedItems.Cast<ProxyEntity>())
            {
                await vm.Delete(wordlist);
            }

            Alert.Info("Done", "Successfully deleted the selected proxies from the group");
        }

        private void ColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            var column = sender as GridViewColumnHeader;
            var sortBy = column.Tag.ToString();

            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                proxiesListView.Items.SortDescriptions.Clear();
            }

            var newDir = ListSortDirection.Ascending;

            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
            {
                newDir = ListSortDirection.Descending;
            }

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            proxiesListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void ProxyListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files.Where(f => f.EndsWith(".txt")))
                {
                    var lines = File.ReadAllLines(file);
                    var dto = new ProxiesForImportDto { Lines = lines };

                    if (file.Contains("socks4a", StringComparison.OrdinalIgnoreCase))
                    {
                        dto.DefaultType = ProxyType.Socks4a;
                    }
                    else if (file.Contains("socks4", StringComparison.OrdinalIgnoreCase))
                    {
                        dto.DefaultType = ProxyType.Socks4;
                    }
                    else if (file.Contains("socks5", StringComparison.OrdinalIgnoreCase))
                    {
                        dto.DefaultType = ProxyType.Socks5;
                    }
                    else // Default to HTTP
                    {
                        dto.DefaultType = ProxyType.Http;
                    }
                }
            }
        }

        private void ItemRightClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
