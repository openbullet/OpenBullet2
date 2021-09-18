using BlazorDownloadFile;
using Blazored.Modal;
using Blazored.Modal.Services;
using GridBlazor;
using GridBlazor.Pages;
using GridMvc.Server;
using GridShared;
using GridShared.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using OpenBullet2.Auth;
using OpenBullet2.Components;
using OpenBullet2.DTOs;
using OpenBullet2.Core.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenBullet2.Core.Helpers;
using OpenBullet2.Core.Services;

namespace OpenBullet2.Pages
{
    public partial class ProxyGroups
    {
        [Inject] private IModalService Modal { get; set; }
        [Inject] private IProxyGroupRepository ProxyGroupsRepo { get; set; }
        [Inject] private IProxyRepository ProxyRepo { get; set; }
        [Inject] private IGuestRepository GuestRepo { get; set; }
        [Inject] private JobManagerService JobManagerService { get; set; }
        [Inject] private AuthenticationStateProvider Auth { get; set; }
        [Inject] private VolatileSettingsService VolatileSettings { get; set; }
        [Inject] private IBlazorDownloadFileService BlazorDownloadFileService { get; set; }

        private InputSelectNumber<int> groupSelectElement;
        private List<ProxyGroupEntity> groups = new();
        private int currentGroupId = -1;
        private List<ProxyEntity> proxies = new();
        private int maxPing = 5000;
        private int uid = -1;
        private int WorkingProxies => proxies.Count(p => p.Status == ProxyWorkingStatus.Working);
        private int NotWorkingProxies => proxies.Count(p => p.Status == ProxyWorkingStatus.NotWorking);

        private GridComponent<ProxyEntity> gridComponent;
        private CGrid<ProxyEntity> grid;
        private Task gridLoad;

        private Action<IGridColumnCollection<ProxyEntity>> gridColumns;

        protected override async Task OnParametersSetAsync()
        {
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            groups = uid == 0
                ? await ProxyGroupsRepo.GetAll().Include(g => g.Owner).ToListAsync()
                : await ProxyGroupsRepo.GetAll().Include(g => g.Owner).Where(g => g.Owner.Id == uid).ToListAsync();

            proxies = uid == 0
                    ? await ProxyRepo.GetAll().ToListAsync()
                    : await ProxyRepo.GetAll().Include(p => p.Group).ThenInclude(g => g.Owner)
                        .Where(p => p.Group.Owner.Id == uid).ToListAsync();

            gridColumns = c =>
            {
                c.Add(p => p.Type).Titled(Loc["Type"]);
                c.Add(p => p.Host).Titled(Loc["Host"]);
                c.Add(p => p.Port).Titled(Loc["Port"]);
                c.Add(p => p.Username).Titled(Loc["Username"]);
                c.Add(p => p.Password).Titled(Loc["Password"]);
                c.Add(p => p.Country).Titled(Loc["Country"]);
                c.Add(p => p.Status).Titled(Loc["Status"]);
                c.Add(p => p.Ping).Titled(Loc["Ping"]);
                c.Add(p => p.LastChecked).Titled(Loc["LastChecked"]).SetFilterWidgetType("DateTimeLocal").Format("{0:dd/MM/yyyy HH:mm}");
            };

            var query = new QueryDictionary<StringValues>();
            query.Add("grid-page", "1");

            var client = new GridClient<ProxyEntity>(q => GetGridRows(gridColumns, q), query, false, "proxiesGrid", gridColumns, CultureInfo.CurrentCulture)
                .Sortable()
                .Filterable()
                .WithMultipleFilters()
                .SetKeyboard(true)
                .ChangePageSize(true)
                .WithGridItemsCount()
                .ExtSortable();
            grid = client.Grid;

            // Try to set a previous filter
            if (VolatileSettings.GridQueries.ContainsKey((uid, "proxiesGrid")))
            {
                grid.Query = VolatileSettings.GridQueries[(uid, "proxiesGrid")];
            }

            // Set new items to grid
            gridLoad = client.UpdateGrid();
            await gridLoad;
        }

        private ItemsDTO<ProxyEntity> GetGridRows(Action<IGridColumnCollection<ProxyEntity>> columns,
                QueryDictionary<StringValues> query)
        {
            VolatileSettings.GridQueries[(uid, "proxiesGrid")] = query;

            var server = new GridServer<ProxyEntity>(proxies, new QueryCollection(query),
                true, "proxiesGrid", columns, 10).Sortable().Filterable().WithMultipleFilters();

            // Return items to displays
            return server.ItemsToDisplay;
        }

        private async Task RefreshList()
        {
            if (currentGroupId == -1)
            {
                proxies = uid == 0
                    ? await ProxyRepo.GetAll().ToListAsync()
                    : await ProxyRepo.GetAll().Include(p => p.Group).ThenInclude(g => g.Owner)
                        .Where(p => p.Group.Owner.Id == uid).ToListAsync();
            }
            else
            {
                proxies = await ProxyRepo.GetAll().Where(p => p.Group.Id == currentGroupId).ToListAsync();
            }

            await gridComponent.UpdateGrid();
            StateHasChanged();
        }

        private async Task OnGroupSelected(int value)
        {
            currentGroupId = value;
            await RefreshList();
        }

        private async Task AddGroup()
        {
            var modal = Modal.Show<ProxyGroupCreate>(Loc["CreateProxyGroup"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var entity = result.Data as ProxyGroupEntity;
                entity.Owner = await GuestRepo.Get(uid);
                await ProxyGroupsRepo.Add(entity);
                groups.Add(entity);
                await js.AlertSuccess(Loc["Created"], Loc["ProxyGroupCreated"]);
                currentGroupId = groups.Last().Id;
                await RefreshList();
            }
        }

        private async Task EditGroup()
        {
            if (currentGroupId == -1)
            {
                await ShowNoGroupSelectedWarning();
                return;
            }

            var groupToEdit = groups.First(g => g.Id == currentGroupId);
            var parameters = new ModalParameters();
            parameters.Add(nameof(ProxyGroupEdit.ProxyGroup), groupToEdit);

            var modal = Modal.Show<ProxyGroupEdit>(Loc["EditProxyGroup"], parameters);
            await modal.Result;
            await RefreshList();
        }

        private async Task DeleteGroup()
        {
            if (currentGroupId == -1)
            {
                await ShowNoGroupSelectedWarning();
                return;
            }

            // TODO: Find a better way to do this
            // Get the first proxy of every ProxyCheckJob
            var firstProxies = JobManagerService.Jobs.OfType<ProxyCheckJob>()
                .Select(j => j.Proxies.FirstOrDefault()).Where(p => p != null);

            // Run through all the list of proxies
            foreach (var f in firstProxies)
            {
                // If we find that a proxy which is in use by a job belongs to the group to delete
                if (proxies.Any(p => p.Id == f.Id))
                {
                    // Prompt error and return
                    await js.AlertError(Loc["GroupInUse"], Loc["GroupInUseWarning"]);
                    return;
                }
            }

            var groupToDelete = groups.First(g => g.Id == currentGroupId);

            if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {groupToDelete.Name}?"))
            {
                // Delete the group from the DB
                await ProxyGroupsRepo.Delete(groupToDelete);

                // Delete the group from the local list
                groups.Remove(groupToDelete);

                // Delete the proxies related to that group from the DB
                await ProxyRepo.Delete(proxies);

                // Change to All and refresh
                currentGroupId = -1;
                await RefreshList();
            }
        }

        private async Task ImportProxies()
        {
            if (currentGroupId == -1)
            {
                await ShowNoGroupSelectedWarning();
                return;
            }

            var modal = Modal.Show<ImportProxies>(Loc["ImportProxies"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var dto = result.Data as ProxiesForImportDto;

                var entities = ParseProxies(dto).ToList();
                var currentGroup = await ProxyGroupsRepo.Get(currentGroupId);
                entities.ForEach(e => e.Group = currentGroup);

                await ProxyRepo.Add(entities);
                await ProxyRepo.RemoveDuplicates(currentGroupId);
                await RefreshList();

                await js.AlertSuccess(Loc["Imported"], $"{Loc["ProxiesImportedSuccessfully"]}: {dto.Lines.Distinct().Count()}");
            }
        }

        private async Task ExportProxies()
        {
            var proxiesList = proxies.Select(x => x.ToString());
            var outputProxies = string.Join(Environment.NewLine, proxiesList);
            byte[] outputBytes = Encoding.UTF8.GetBytes(outputProxies);

            await BlazorDownloadFileService.DownloadFile("proxies.txt", outputBytes, "text/plain");
        }

        private async Task DeleteAllProxies()
        {
            if (currentGroupId == -1)
            {
                await ShowNoGroupSelectedWarning();
                return;
            }

            var toDelete = await ProxyRepo.GetAll()
                .Where(p => p.Group.Id == currentGroupId)
                .ToListAsync();

            await DeleteProxies(toDelete);
        }

        private async Task DeleteNotWorking()
        {
            var all = ProxyRepo.GetAll();

            if (currentGroupId != -1)
                all = all.Where(p => p.Group.Id == currentGroupId);

            var toDelete = await all.Where(p => p.Status == ProxyWorkingStatus.NotWorking).ToListAsync();
            await DeleteProxies(toDelete);
        }

        private async Task DeleteUntested()
        {
            var all = ProxyRepo.GetAll();

            if (currentGroupId != -1)
                all = all.Where(p => p.Group.Id == currentGroupId);

            var toDelete = await all.Where(p => p.Status == ProxyWorkingStatus.Untested).ToListAsync();
            await DeleteProxies(toDelete);
        }

        private async Task DeleteSlow()
        {
            var all = ProxyRepo.GetAll();

            if (currentGroupId != -1)
                all = all.Where(p => p.Group.Id == currentGroupId);

            var toDelete = await all.Where(p => p.Status == ProxyWorkingStatus.Working && p.Ping > maxPing).ToListAsync();
            await DeleteProxies(toDelete);
        }

        private async Task DeleteFiltered()
            => await DeleteProxies(GetFiltered());

        private async Task MoveFiltered()
        {
            if (groups.Count < 2)
            {
                await js.AlertError(Loc["Uh-Oh"], Loc["NeedTwoOrMoreGroups"]);
                return;
            }

            var parameters = new ModalParameters();
            parameters.Add(nameof(ProxyGroupSelector.Groups), groups.Where(g => g.Id != currentGroupId));
            var modal = Modal.Show<ProxyGroupSelector>(Loc["SelectProxyGroup"], parameters);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var group = result.Data as ProxyGroupEntity;
                var filtered = GetFiltered();

                foreach (var proxy in filtered)
                {
                    proxy.Group = group;
                }

                await ProxyRepo.Update(filtered);

                await js.AlertSuccess(Loc["Moved"], $"{filtered.Count()} {Loc["proxiesMoved"]}");
                await RefreshList();
            }
        }

        private IEnumerable<ProxyEntity> GetFiltered()
            => new GridServer<ProxyEntity>(proxies, new QueryCollection(grid.Query),
                    true, "hitsGrid", gridColumns, null).Sortable().Filterable().WithMultipleFilters().ItemsToDisplay.Items;

        private async Task DeleteProxies(IEnumerable<ProxyEntity> toDelete)
        {
            await ProxyRepo.Delete(toDelete);
            await RefreshList();
            await js.AlertSuccess(Loc["Deleted"], $"{Loc["ProxiesDeletedSuccessfully"]}: {toDelete.Count()}");
        }

        private IEnumerable<ProxyEntity> ParseProxies(ProxiesForImportDto dto)
        {
            var proxies = new List<Proxy>();

            foreach (var line in dto.Lines)
            {
                if (Proxy.TryParse(line, out var proxy, dto.DefaultType, dto.DefaultUsername, dto.DefaultPassword))
                {
                    proxies.Add(proxy);
                }
            }

            return proxies.Select(p => Mapper.MapProxyToProxyEntity(p));
        }

        private async Task ShowNoGroupSelectedWarning()
            => await js.AlertError(Loc["Uh-Oh"], Loc["NoGroupSelected"]);
    }
}
