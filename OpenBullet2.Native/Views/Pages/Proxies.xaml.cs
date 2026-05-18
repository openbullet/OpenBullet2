using Microsoft.Win32;
using Microsoft.Extensions.Logging;
using OpenBullet2.Core.Entities;
using OpenBullet2.Native.DTOs;
using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for Proxies.xaml
/// </summary>
public partial class Proxies : Page
{
    private readonly ILogger<Proxies> logger;
    private readonly IUiFactory uiFactory;
    private readonly ProxiesViewModel vm;
    private GridViewColumnHeader? listViewSortCol;
    private SortAdorner? listViewSortAdorner;

    private IEnumerable<ProxyEntity> SelectedProxies => proxiesListView.SelectedItems.Cast<ProxyEntity>().ToList();

    public Proxies(ILogger<Proxies> logger, IUiFactory uiFactory, ProxiesViewModel vm)
    {
        this.logger = logger;
        this.uiFactory = uiFactory;
        this.vm = vm;
        DataContext = vm;
        _ = vm.InitializeAsync();

        InitializeComponent();
    }

    private void AddGroup(object sender, RoutedEventArgs e)
        => new MainDialog(uiFactory.Create<AddProxyGroupDialog>(this), "Add proxy group").ShowDialog();

    private void EditGroup(object sender, RoutedEventArgs e)
    {
        if (!vm.GroupIsValid)
        {
            ShowInvalidGroupError();
            return;
        }

        new MainDialog(uiFactory.Create<AddProxyGroupDialog>(this, vm.SelectedGroup), "Edit proxy group").ShowDialog();
    }

    private async void DeleteGroup(object sender, RoutedEventArgs e)
    {
        if (!vm.GroupIsValid)
        {
            ShowInvalidGroupError();
            return;
        }

        try
        {
            await vm.DeleteSelectedGroupAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete selected proxy group from Native proxies page");
            Alert.Exception(ex);
        }
    }

    private async void DeleteNotWorking(object sender, RoutedEventArgs e)
    {
        try
        {
            await vm.DeleteNotWorkingAsync();
            Alert.Success("Done", "Successfully deleted the not working proxies from the group");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete not working proxies from Native proxies page");
            Alert.Exception(ex);
        }
    }

    private async void DeleteUntested(object sender, RoutedEventArgs e)
    {
        try
        {
            await vm.DeleteUntestedAsync();
            Alert.Success("Done", "Successfully deleted the untested proxies from the group");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete untested proxies from Native proxies page");
            Alert.Exception(ex);
        }
    }

    private void DeleteLowQuality(object sender, RoutedEventArgs e)
        => new MainDialog(uiFactory.Create<DeleteLowQualityProxiesDialog>(this),
            "Delete low-quality proxies").ShowDialog();

    private void Import(object sender, RoutedEventArgs e)
    {
        if (!vm.GroupIsValid)
        {
            ShowInvalidGroupError();
            return;
        }

        new MainDialog(uiFactory.Create<ImportProxiesDialog>(this), "Import proxies").ShowDialog();
    }

    public Task AddGroupAsync(ProxyGroupEntity entity) => vm.AddGroupAsync(entity);
    public Task EditGroupAsync(ProxyGroupEntity entity) => vm.EditGroupAsync(entity);

    public void UpdateViewModel() => vm.UpdateViewModel();

    private void ExportSelected(object sender, RoutedEventArgs e)
    {
        var sfd = new SaveFileDialog
        {
            Filter = "Text File |*.txt",
            Title = "Export proxies"
        };
        sfd.ShowDialog();

        if (!string.IsNullOrWhiteSpace(sfd.FileName))
        {
            if (SelectedProxies.Any())
            {
                SelectedProxies.SaveToFile(sfd.FileName, p => p.ToString());
            }
            else
            {
                Alert.Error("Uh-oh", "No proxies selected");
            }
        }
    }

    private void CopySelectedProxies(object sender, RoutedEventArgs e)
        => SelectedProxies.CopyToClipboard(p => $"{p.Host}:{p.Port}");

    private void CopySelectedProxiesFull(object sender, RoutedEventArgs e)
        => SelectedProxies.CopyToClipboard(p => p.ToString());

    public Task AddProxiesAsync(ProxiesForImportDto dto) => vm.AddProxiesAsync(dto);

    public async Task DeleteLowQualityAsync(DeleteLowQualityProxiesDto dto)
    {
        await vm.DeleteLowQualityAsync(dto);
        Alert.Success("Done", "Successfully deleted the selected low-quality proxies from the group");
    }

    private async void DeleteSelected(object sender, RoutedEventArgs e)
    {
        try
        {
            await vm.DeleteAsync(SelectedProxies);
            Alert.Success("Done", "Successfully deleted the selected proxies from the group");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete selected proxies from Native proxies page");
            Alert.Exception(ex);
        }
    }

    private void ColumnHeaderClicked(object sender, RoutedEventArgs e)
    {
        var column = sender as GridViewColumnHeader;
        var sortBy = column?.Tag?.ToString();

        if (column is null || string.IsNullOrEmpty(sortBy))
        {
            return;
        }

        if (listViewSortCol != null)
        {
            AdornerLayer.GetAdornerLayer(listViewSortCol)?.Remove(listViewSortAdorner);
            proxiesListView.Items.SortDescriptions.Clear();
        }

        var newDir = ListSortDirection.Ascending;

        if (listViewSortCol == column && listViewSortAdorner?.Direction == newDir)
        {
            newDir = ListSortDirection.Descending;
        }

        listViewSortCol = column;
        listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
        AdornerLayer.GetAdornerLayer(listViewSortCol)?.Add(listViewSortAdorner);
        proxiesListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
    }

    private void ProxyListViewDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files is null)
            {
                return;
            }

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

    private void ShowInvalidGroupError()
        => Alert.Error("Invalid group", "Please select or create a valid group first!");
}
