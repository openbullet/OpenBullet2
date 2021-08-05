using OpenBullet2.Core.Entities;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
    }
}
