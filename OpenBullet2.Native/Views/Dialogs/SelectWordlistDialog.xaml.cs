using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectWordlistDialog.xaml
    /// </summary>
    public partial class SelectWordlistDialog : Page
    {
        private readonly object caller;
        private readonly SelectWordlistDialogViewModel vm;
        private GridViewColumnHeader listViewSortCol;
        private SortAdorner listViewSortAdorner;

        public SelectWordlistDialog(object caller)
        {
            this.caller = caller;

            vm = new SelectWordlistDialogViewModel();
            DataContext = vm;

            InitializeComponent();
        }

        private void ItemHovered(object sender, SelectionChangedEventArgs e)
        {
            var items = e.AddedItems as IList<object>;

            if (items.Count == 1)
            {
                vm.HoveredWordlist = items[0] as WordlistEntity;
            }
        }

        private void ListItemDoubleClick(object sender, MouseButtonEventArgs e) => ConfirmSelection();
        private void Accept(object sender, RoutedEventArgs e) => ConfirmSelection();

        private void ConfirmSelection()
        {
            if (vm.HoveredWordlist is null)
            {
                ShowNoWordlistSelectedError();
                return;
            }

            if (caller is MultiRunJobOptionsDialog page)
            {
                page.SelectWordlist(vm.HoveredWordlist);
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
                wordlistListView.Items.SortDescriptions.Clear();
            }

            var newDir = ListSortDirection.Ascending;

            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
            {
                newDir = ListSortDirection.Descending;
            }

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            wordlistListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void ShowNoWordlistSelectedError() => Alert.Error("No wordlist selected", "Please select a wordlist first!");
    }

    public class SelectWordlistDialogViewModel : ViewModelBase
    {
        private readonly IWordlistRepository wordlistRepo;

        private ObservableCollection<WordlistEntity> wordlistsCollection;

        public ObservableCollection<WordlistEntity> WordlistsCollection
        {
            get => wordlistsCollection;
            set
            {
                wordlistsCollection = value;
                OnPropertyChanged();
            }
        }

        private WordlistEntity hoveredWordlist;
        public WordlistEntity HoveredWordlist
        {
            get => hoveredWordlist;
            set
            {
                hoveredWordlist = value;
                OnPropertyChanged();

                try
                {
                    LinesPreview = string.Join(Environment.NewLine, File.ReadLines(hoveredWordlist.FileName).Take(10));
                }
                catch
                {
                    LinesPreview = "Could not load the preview...";
                }
            }
        }

        private string linesPreview = "Select a wordlist to display a preview of its first 10 lines";
        public string LinesPreview
        {
            get => linesPreview;
            set
            {
                linesPreview = value;
                OnPropertyChanged();
            }
        }

        public SelectWordlistDialogViewModel()
        {
            wordlistRepo = SP.GetService<IWordlistRepository>();
            CreateCollection();
        }

        private void CreateCollection()
        {
            var entities = wordlistRepo.GetAll().ToList();
            WordlistsCollection = new ObservableCollection<WordlistEntity>(entities);
        }
    }
}
