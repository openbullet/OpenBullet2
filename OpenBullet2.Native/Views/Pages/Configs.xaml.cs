using OpenBullet2.Native.DTOs;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for Configs.xaml
    /// </summary>
    public partial class Configs : Page
    {
        private readonly ConfigsViewModel vm;
        private GridViewColumnHeader listViewSortCol;
        private SortAdorner listViewSortAdorner;

        private ConfigViewModel HoveredItem => (ConfigViewModel)configsListView.SelectedItem;

        public Configs()
        {
            vm = SP.GetService<ViewModelsService>().Configs;
            DataContext = vm;

            InitializeComponent();
        }

        // This is needed otherwise if properties of a config are updated by another page this page will not
        // get notified and will show the old values.
        public void UpdateViewModel() => vm.SelectedConfig?.UpdateViewModel();

        private void Create(object sender, RoutedEventArgs e)
            => new MainDialog(new CreateConfigDialog(this), "Create config").ShowDialog();

        public async void CreateConfig(ConfigForCreationDto dto) => await vm.Create(dto);

        private void Edit(object sender, RoutedEventArgs e)
        {
            if (HoveredItem is null)
            {
                ShowNoConfigSelectedError();
                return;
            }

            vm.SelectedConfig = HoveredItem;
            // TODO: Navigate to the correct page
        }

        private async void Save(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedConfig is null)
            {
                ShowNoConfigSelectedError();
                return;
            }

            await vm.Save(vm.SelectedConfig);
            Alert.Info("Success", $"{vm.SelectedConfig.Config.Metadata.Name} was saved successfully!");
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (HoveredItem is null)
            {
                ShowNoConfigSelectedError();
                return;
            }

            if (Alert.Choice("Are you sure?", $"Do you really want to delete {HoveredItem.Name}"))
            {
                vm.Delete(HoveredItem);
            }
        }

        // TODO: Check if current config is not saved and prompt warning
        private async void Rescan(object sender, RoutedEventArgs e) => await vm.Rescan();

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", Path.Combine(Directory.GetCurrentDirectory(), "UserData\\Configs"));
            }
            catch (Exception ex)
            {
                // This happens on access denied
                Alert.Exception(ex);
            }
        }

        private void ShowNoConfigSelectedError() => Alert.Error("No config selected", "Please select a config first!");

        private void ItemHovered(object sender, SelectionChangedEventArgs e)
        {
            var items = e.AddedItems as IList<object>;

            if (items.Count == 1)
            {
                vm.HoveredConfig = items[0] as ConfigViewModel;
            }
        }

        private void ListItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (HoveredItem is null)
            {
                ShowNoConfigSelectedError();
                return;
            }

            vm.SelectedConfig = HoveredItem;
            // TODO: Navigate to the correct page
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
    }
}
