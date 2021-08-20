using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        }

        private void CreateCollection()
        {
            var viewModels = configService.Configs.Select(c => new ConfigViewModel(c));
            ConfigsCollection = new ObservableCollection<ConfigViewModel>(viewModels);
        }
    }
}
