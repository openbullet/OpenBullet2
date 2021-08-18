using OpenBullet2.Core.Entities;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Environment;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for Wordlists.xaml
    /// </summary>
    public partial class Wordlists : Page
    {
        private readonly WordlistsViewModel vm;
        private readonly EnvironmentSettings env;
        private readonly MainWindow window;
        private GridViewColumnHeader listViewSortCol;
        private SortAdorner listViewSortAdorner;

        private IEnumerable<WordlistEntity> SelectedWordlists => wordlistListView.SelectedItems.Cast<WordlistEntity>().ToList();

        public Wordlists()
        {
            vm = SP.GetService<ViewModelsService>().Wordlists;
            DataContext = vm;
            _ = vm.Initialize();

            InitializeComponent();
            window = SP.GetService<MainWindow>();
            env = SP.GetService<RuriLibSettingsService>().Environment;
        }

        private void Add(object sender, RoutedEventArgs e)
            => new MainDialog(new AddWordlistDialog(this), "Add a wordlist").ShowDialog();

        private async void DeleteSelected(object sender, RoutedEventArgs e)
        {
            foreach (var wordlist in SelectedWordlists)
            {
                await vm.Delete(wordlist);
            }

            Alert.Success("Done", "Successfully deleted the selected wordlist references from the DB");
        }

        private void DeleteAll(object sender, RoutedEventArgs e)
        {
            vm.DeleteAll();
            Alert.Success("Done", "Successfully deleted all wordlist references from the DB");
        }

        private async void DeleteNotFound(object sender, RoutedEventArgs e)
        {
            var deleted = await vm.DeleteNotFound();
            Alert.Success("Done", $"Successfully deleted {deleted} unresolved wordlist references from the DB");
        }

        private void UpdateSearch(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                vm.SearchString = filterTextbox.Text;
            }
        }

        private void Search(object sender, RoutedEventArgs e) => vm.SearchString = filterTextbox.Text;

        public async void AddWordlist(WordlistEntity wordlist)
        {
            try
            {
                await vm.Add(wordlist);
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
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

        private async void HandleDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files.Where(f => f.EndsWith(".txt")))
                {
                    try
                    {
                        var path = file;
                        var cwd = Directory.GetCurrentDirectory();

                        // Make the path relative if inside the CWD
                        if (path.StartsWith(cwd))
                        {
                            path = path[(cwd.Length + 1)..];
                        }

                        var firstLine = File.ReadLines(path).FirstOrDefault(l => !string.IsNullOrWhiteSpace(l)) ?? string.Empty;

                        var entity = new WordlistEntity
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            FileName = path,
                            Type = env.RecognizeWordlistType(firstLine),
                            Purpose = string.Empty,
                            Total = File.ReadLines(path).Count()
                        };

                        await vm.Add(entity);
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
