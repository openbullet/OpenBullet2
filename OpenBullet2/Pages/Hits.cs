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
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Hits
    {
        [Inject] private IModalService Modal { get; set; }
        [Inject] private IHitRepository HitRepo { get; set; }
        [Inject] private AuthenticationStateProvider Auth { get; set; }
        [Inject] private VolatileSettingsService VolatileSettings { get; set; }

        private List<HitEntity> hits = new();
        private HitEntity selectedHit;
        private int uid = -1;

        private GridComponent<HitEntity> gridComponent;
        private CGrid<HitEntity> grid;
        private Task gridLoad;

        protected override async Task OnParametersSetAsync()
        {
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            hits = uid == 0
                ? await HitRepo.GetAll().ToListAsync()
                : await HitRepo.GetAll().Where(h => h.OwnerId == uid).ToListAsync();

            Action<IGridColumnCollection<HitEntity>> columns = c =>
            {
                c.Add(h => h.Data).Titled(Loc["Data"]);
                c.Add(h => h.Type).Titled(Loc["Type"]);
                c.Add(h => h.ConfigName).Titled(Loc["Config"]);
                c.Add(h => h.Date).Titled(Loc["Date"]);
                c.Add(h => h.WordlistName).Titled(Loc["Wordlist"]);
                c.Add(h => h.Proxy).Titled(Loc["Proxy"]);
                c.Add(h => h.CapturedData).Titled(Loc["CapturedData"]);
            };

            var query = new QueryDictionary<StringValues>();
            query.Add("grid-page", "2");

            var client = new GridClient<HitEntity>(q => GetGridRows(columns, q), query, false, "hitsGrid", columns, CultureInfo.CurrentCulture)
                .Sortable()
                .Filterable()
                .WithMultipleFilters()
                .SetKeyboard(true)
                .ChangePageSize(true)
                .WithGridItemsCount()
                .ExtSortable()
                .Selectable(true, false, true);
            grid = client.Grid;

            // Try to set a previous filter
            if (VolatileSettings.GridQueries.ContainsKey("hitsGrid"))
            {
                grid.Query = VolatileSettings.GridQueries["hitsGrid"];
            }

            // Set new items to grid
            gridLoad = client.UpdateGrid();
            await gridLoad;
        }

        private ItemsDTO<HitEntity> GetGridRows(Action<IGridColumnCollection<HitEntity>> columns,
                QueryDictionary<StringValues> query)
        {
            VolatileSettings.GridQueries["hitsGrid"] = query;

            var server = new GridServer<HitEntity>(hits, new QueryCollection(query),
                true, "hitsGrid", columns, 15).Sortable().Filterable().WithMultipleFilters();

            // Return items to displays
            return server.ItemsToDisplay;
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

            await gridComponent.UpdateGrid();
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
            var selected = grid.SelectedItems.Cast<HitEntity>().ToList();

            if (selected.Count == 0)
            {
                await ShowNoHitSelectedWarning();
                return;
            }

            if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {selected.Count} {Loc["hits"]}?", Loc["Cancel"]))
            {
                // Delete the hit from the db
                await HitRepo.Delete(selected);

                // Delete the hit from the local list
                selected.ForEach(h => hits.Remove(h));
            }

            await RefreshList();
        }

        private async Task DeleteFiltered()
        {
            var selected = grid.Items.ToList();

            if (selected.Count == 0)
            {
                await ShowNoHitSelectedWarning();
                return;
            }

            if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {selected.Count} {Loc["hits"]}?", Loc["Cancel"]))
            {
                // Delete the hit from the db
                await HitRepo.Delete(selected);

                // Delete the hit from the local list
                selected.ForEach(h => hits.Remove(h));
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
    }
}
