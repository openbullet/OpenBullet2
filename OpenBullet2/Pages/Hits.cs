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
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Shared.Forms;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Hits
    {
        [Inject] IModalService Modal { get; set; }
        [Inject] IHitRepository HitRepo { get; set; }
        [Inject] AuthenticationStateProvider Auth { get; set; }

        private List<HitEntity> hits = new();
        private HitEntity selectedHit;
        private int uid = -1;

        RadzenGrid<HitEntity> hitsGrid;
        private int resultsPerPage = 15;

        private GridComponent<HitEntity> _gridComponent;
        private CGrid<HitEntity> _grid;
        private Task _task;

        private string CaptureWidth
        {
            get
            {
                if (hits.Count == 0)
                    return "200px";

                var longest = hits
                    .Select(h => h.CapturedData.Length)
                    .OrderBy(l => l)
                    .Last();

                // The 0.82 value is referred to Consolas font-style
                // since 2048 units in height correspond to 1126 units in width,
                // and the 12 is referred to 12px in the css
                var totalWidth = (int)(longest * 12 * 0.82);

                if (totalWidth < 200)
                    return "200px";

                return $"{totalWidth}px";
            }
        }

        protected override async Task OnInitializedAsync()
        {
            
        }

        protected override async Task OnParametersSetAsync()
        {
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            hits = uid == 0
                ? await HitRepo.GetAll().ToListAsync()
                : await HitRepo.GetAll().Where(h => h.OwnerId == uid).ToListAsync();

            Action<IGridColumnCollection<HitEntity>> columns = c =>
            {
                c.Add(h => h.Data).Sortable(true).Filterable(true);
                c.Add(h => h.Type).Sortable(true).Filterable(true);
                c.Add(h => h.ConfigName).Sortable(true).Filterable(true);
                c.Add(h => h.Date).Sortable(true).Filterable(true);
                c.Add(h => h.WordlistName).Sortable(true).Filterable(true);
                c.Add(h => h.Proxy).Sortable(true).Filterable(true);
                c.Add(h => h.CapturedData).Sortable(true).Filterable(true);
            };

            var query = new QueryDictionary<StringValues>();
            query.Add("grid-page", "2");

            var client = new GridClient<HitEntity>(q => GetGridRows(columns, q), query, false, "hitsGrid", columns)
                .Selectable(true, true, true);
            _grid = client.Grid;

            // Set new items to grid
            _task = client.UpdateGrid();
            await _task;
        }

        private async Task OnResultsPerPageChanged(int value)
        {
            resultsPerPage = value;
            await RefreshList();
        }

        private void SelectHit(HitEntity hit)
        {
            selectedHit = hit;
        }

        protected void OnHitSelected(object item)
        {
            if (item.GetType() == typeof(HitEntity))
            {
                selectedHit = (HitEntity)item;
            }
        }

        private async Task RefreshList()
        {
            hits = uid == 0 
                ? await HitRepo.GetAll().ToListAsync()
                : await HitRepo.GetAll().Where(h => h.OwnerId == uid).ToListAsync();

            await _gridComponent.UpdateGrid();
            StateHasChanged();
        }

        private async Task EditHit()
        {
            if (selectedHit == null)
            {
                await ShowNoHitSelectedWarning();
                return;
            }

            var parameters = new ModalParameters();
            parameters.Add(nameof(HitEdit.Hit), selectedHit);

            var modal = Modal.Show<HitEdit>(Loc["EditHit"], parameters);
            await modal.Result;

            await RefreshList();
        }

        private async Task DeleteHit()
        {
            if (selectedHit == null)
            {
                await ShowNoHitSelectedWarning();
                return;
            }

            if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {selectedHit.Data}?", Loc["Cancel"]))
            {
                // Delete the hit from the db
                await HitRepo.Delete(selectedHit);

                // Delete the hit from the local list
                hits.Remove(selectedHit);
            }

            await RefreshList();
        }

        private async Task PurgeHits()
        {
            if (await js.Confirm(Loc["AreYouSure"], Loc["ReallyDeleteAllHits"], Loc["Cancel"]))
            {
                if (uid == 0)
                {
                    HitRepo.Purge();
                }
                else
                {
                    await HitRepo.Delete(hits);
                }
                
                await RefreshList();
            }
        }

        private async Task ShowNoHitSelectedWarning()
            => await js.AlertError(Loc["Uh-Oh"], Loc["NoHitSelectedWarning"]);

        private ItemsDTO<HitEntity> GetGridRows(Action<IGridColumnCollection<HitEntity>> columns,
                QueryDictionary<StringValues> query)
        {
            var server = new GridServer<HitEntity>(hits, new QueryCollection(query),
                true, "hitsGrid", columns, resultsPerPage).Sortable().Filterable().WithMultipleFilters();

            // Return items to displays
            return server.ItemsToDisplay;
        }
    }
}
