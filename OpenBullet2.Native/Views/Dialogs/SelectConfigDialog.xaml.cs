using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectConfigDialog.xaml
    /// </summary>
    public partial class SelectConfigDialog : Page
    {
        private readonly object caller;
        private readonly SelectConfigDialogViewModel vm;
        private GridViewColumnHeader listViewSortCol;
        private SortAdorner listViewSortAdorner;

        public SelectConfigDialog(object caller)
        {
            this.caller = caller;

            vm = new SelectConfigDialogViewModel();
            DataContext = vm;

            InitializeComponent();
        }

        private void UpdateSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                vm.SearchString = filterTextbox.Text;
            }
        }

        private void Search(object sender, RoutedEventArgs e) => vm.SearchString = filterTextbox.Text;

        private void ItemHovered(object sender, SelectionChangedEventArgs e)
        {
            var items = e.AddedItems as IList<object>;

            if (items.Count == 1)
            {
                vm.HoveredConfig = items[0] as ConfigViewModel;
            }
        }

        private void ListItemDoubleClick(object sender, MouseButtonEventArgs e) => ConfirmSelection();
        private void Accept(object sender, RoutedEventArgs e) => ConfirmSelection();

        private void ConfirmSelection()
        {
            if (vm.HoveredConfig is null)
            {
                ShowNoConfigSelectedError();
                return;
            }

            if (caller is MultiRunJobOptionsDialog page)
            {
                page.SelectConfig(vm.HoveredConfig);
            }

            ((MainDialog)Parent).Close();
        }

        private void ColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            var column = sender as GridViewColumnHeader;
            var sortBy = column.Tag.ToString();

            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                configsListView.Items.SortDescriptions.Clear();
            }

            var newDir = ListSortDirection.Ascending;

            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
            {
                newDir = ListSortDirection.Descending;
            }

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            configsListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void ShowNoConfigSelectedError() => Alert.Error("No config selected", "Please select a config first!");
    }

    public class SelectConfigDialogViewModel : ViewModelBase
    {
        private readonly ConfigsViewModel configsViewModel;
        private readonly ConfigService configService;

        private ObservableCollection<ConfigViewModel> configsCollection;
        public ObservableCollection<ConfigViewModel> ConfigsCollection
        {
            get => configsCollection;
            set
            {
                configsCollection = value;
                OnPropertyChanged();
            }
        }

        private string searchString = string.Empty;
        public string SearchString
        {
            get => searchString;
            set
            {
                searchString = value;

                if (configsViewModel is not null)
                {
                    configsViewModel.SearchString = value;
                }

                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(ConfigsCollection).Refresh();
            }
        }

        private ConfigViewModel hoveredConfig;
        public ConfigViewModel HoveredConfig
        {
            get => hoveredConfig;
            set
            {
                hoveredConfig = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsConfigHovered));
            }
        }

        public bool IsConfigHovered => HoveredConfig != null;

        public SelectConfigDialogViewModel()
        {
            configService = SP.GetService<ConfigService>();
            CreateCollection();

            configsViewModel = SP.GetService<ViewModelsService>().Configs;

            if (configsViewModel is not null)
            {
                SearchString = configsViewModel.SearchString;
            }
        }

        private void CreateCollection()
        {
            var viewModels = configService.Configs.Select(c => new ConfigViewModel(c));
            ConfigsCollection = new ObservableCollection<ConfigViewModel>(viewModels);
            Application.Current.Dispatcher.Invoke(() => HookFilters());
        }

        public void HookFilters()
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(ConfigsCollection);
            view.Filter = ConfigsFilter;
        }

        private bool ConfigsFilter(object item) => (item as ConfigViewModel).Config
            .Metadata.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }
}
