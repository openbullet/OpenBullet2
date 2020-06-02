using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Components;
using OpenBullet2.DTOs;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Proxies;
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
            var modal = Modal.Show<ProxyGroupCreate>("Create proxy group");
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                groups.Add(result.Data as ProxyGroupEntity);
                await js.AlertSuccess("Created", "The proxy group was created successfully!");
                currentGroupId = groups.Last().Id;
                await RefreshList();
            }
        }

        private async Task EditGroup()
        {
            if (currentGroupId == -1)
            {
                await js.AlertError("Hmm", "Please select an actual group first");
                return;
            }

            var groupToEdit = groups.First(g => g.Id == currentGroupId);
            var parameters = new ModalParameters();
            parameters.Add(nameof(ProxyGroupEdit.ProxyGroup), groupToEdit);

            var modal = Modal.Show<ProxyGroupEdit>("Edit proxy group", parameters);
            await modal.Result;
        }

        private async Task DeleteGroup()
        {
            if (currentGroupId == -1)
            {
                await js.AlertError("Hmm", "Please select an actual group first");
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
                    await js.AlertError("Group in use", "A job is currently using this proxy group. Please stop and remove the job from the manager and try again.");
                    return;
                }
            }

            var groupToDelete = groups.First(g => g.Id == currentGroupId);

            if (await js.Confirm("Are you sure?", $"Do you really want to delete {groupToDelete.Name}?"))
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
                await js.AlertError("Hmm", "Please create a proxy group or select a valid one");
                return;
            }

            var modal = Modal.Show<ImportProxies>("Import proxies");
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
                await RefreshList();

                await js.AlertSuccess("Imported", $"{dto.Lines.Count()} proxies were imported successfully!");
            }
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
    }
}
