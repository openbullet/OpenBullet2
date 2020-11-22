using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Components;
using OpenBullet2.DTOs;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using Radzen.Blazor;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class ProxyGroups
    {
        [Inject] IModalService Modal { get; set; }
        [Inject] IProxyGroupRepository ProxyGroupsRepo { get; set; }
        [Inject] IProxyRepository ProxyRepo { get; set; }
        [Inject] JobManagerService JobManagerService { get; set; }

        InputSelectNumber<int> groupSelectElement;
        private List<ProxyGroupEntity> groups;
        private int currentGroupId = -1;
        private List<ProxyEntity> proxies;
        private int maxPing = 5000;

        RadzenGrid<ProxyEntity> proxiesGrid;
        private int resultsPerPage = 15;

        protected override async Task OnInitializedAsync()
        {
            groups = await ProxyGroupsRepo.GetAll().ToListAsync();
            await RefreshList();

            await base.OnInitializedAsync();
        }

        private async Task OnGroupSelected(int value)
        {
            currentGroupId = value;
            await RefreshList();
        }

        private async Task OnResultsPerPageChanged(int value)
        {
            resultsPerPage = value;
            await RefreshList();
            StateHasChanged();
        }

        private async Task RefreshList()
        {
            if (currentGroupId == -1)
            {
                proxies = await ProxyRepo.GetAll().ToListAsync();
            }
            else
            {
                proxies = await ProxyRepo.GetAll()
                    .Where(p => p.GroupId == currentGroupId)
                    .ToListAsync();
            }

            StateHasChanged();
        }

        private async Task AddGroup()
        {
            var modal = Modal.Show<ProxyGroupCreate>(Loc["CreateProxyGroup"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                groups.Add(result.Data as ProxyGroupEntity);
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

                foreach (var entity in entities)
                {
                    entity.GroupId = currentGroupId;
                }

                await ProxyRepo.Add(entities);
                await ProxyRepo.RemoveDuplicates(currentGroupId);
                await RefreshList();

                await js.AlertSuccess(Loc["Imported"], $"{Loc["ProxiesImportedSuccessfully"]}: {dto.Lines.Count()}");
            }
        }

        private async Task DeleteAllProxies()
        {
            if (currentGroupId == -1)
            {
                await ShowNoGroupSelectedWarning();
                return;
            }

            var toDelete = await ProxyRepo.GetAll()
                .Where(p => p.GroupId == currentGroupId)
                .ToListAsync();
            await ProxyRepo.Delete(toDelete);
            await RefreshList();
            await js.AlertSuccess(Loc["Deleted"], $"{Loc["ProxiesDeletedSuccessfully"]}: {toDelete.Count}");
        }

        private async Task DeleteNotWorking()
        {
            if (currentGroupId == -1)
            {
                await ShowNoGroupSelectedWarning();
                return;
            }

            var toDelete = await ProxyRepo.GetAll()
                .Where(p => p.GroupId == currentGroupId && p.Status == ProxyWorkingStatus.NotWorking)
                .ToListAsync();
            await ProxyRepo.Delete(toDelete);
            await RefreshList();
            await js.AlertSuccess(Loc["Deleted"], $"{Loc["ProxiesDeletedSuccessfully"]}: {toDelete.Count}");
        }

        private async Task DeleteUntested()
        {
            if (currentGroupId == -1)
            {
                await ShowNoGroupSelectedWarning();
                return;
            }

            var toDelete = await ProxyRepo.GetAll()
                .Where(p => p.GroupId == currentGroupId && p.Status == ProxyWorkingStatus.Untested)
                .ToListAsync();
            await ProxyRepo.Delete(toDelete);
            await RefreshList();
            await js.AlertSuccess(Loc["Deleted"], $"{Loc["ProxiesDeletedSuccessfully"]}: {toDelete.Count}");
        }

        private async Task DeleteSlow()
        {
            if (currentGroupId == -1)
            {
                await ShowNoGroupSelectedWarning();
                return;
            }

            var toDelete = await ProxyRepo.GetAll()
                .Where(p => p.GroupId == currentGroupId && p.Status == ProxyWorkingStatus.Working && p.Ping > maxPing)
                .ToListAsync();
            await ProxyRepo.Delete(toDelete);
            await RefreshList();
            await js.AlertSuccess(Loc["Deleted"], $"{Loc["ProxiesDeletedSuccessfully"]}: {toDelete.Count}");
        }

        private IEnumerable<ProxyEntity> ParseProxies(ProxiesForImportDto dto)
        {
            List<Proxy> proxies = new List<Proxy>();

            foreach (var line in dto.Lines)
            {
                if (Proxy.TryParse(line, out Proxy proxy, dto.DefaultType, dto.DefaultUsername, dto.DefaultPassword))
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
