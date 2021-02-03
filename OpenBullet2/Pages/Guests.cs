using Blazored.Modal;
using Blazored.Modal.Services;
using GridBlazor;
using GridBlazor.Pages;
using GridMvc.Server;
using GridShared;
using GridShared.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Shared.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Guests
    {
        [Inject] IModalService Modal { get; set; }
        [Inject] IGuestRepository GuestRepo { get; set; }
        [Inject] IJobRepository JobRepo { get; set; }
        [Inject] IWordlistRepository WordlistRepo { get; set; }
        [Inject] IProxyGroupRepository ProxyGroupRepo { get; set; }

        private List<GuestEntity> guests;
        private GuestEntity selectedGuest;

        private GridComponent<GuestEntity> gridComponent;
        private CGrid<GuestEntity> grid;
        private Task gridLoad;

        protected override async Task OnParametersSetAsync()
        {
            guests = await GuestRepo.GetAll().ToListAsync();

            Action<IGridColumnCollection<GuestEntity>> columns = c =>
            {
                c.Add(g => g.Username).Titled(Loc["Username"]);
                c.Add(g => g.AccessExpiration).Titled(Loc["AccessExpiration"]);
                c.Add(g => g.AllowedAddresses).Titled(Loc["AllowedAddresses"]);
            };

            var query = new QueryDictionary<StringValues>();
            query.Add("grid-page", "2");

            var client = new GridClient<GuestEntity>(q => GetGridRows(columns, q), query, false, "guestsGrid", columns, CultureInfo.CurrentCulture)
                .Sortable()
                .Filterable()
                .SetKeyboard(true)
                .Selectable(true, false, false);
            grid = client.Grid;

            // Set new items to grid
            gridLoad = client.UpdateGrid();
            await gridLoad;
        }

        private ItemsDTO<GuestEntity> GetGridRows(Action<IGridColumnCollection<GuestEntity>> columns,
                QueryDictionary<StringValues> query)
        {
            var server = new GridServer<GuestEntity>(guests, new QueryCollection(query),
                true, "guestsGrid", columns, 15).Sortable().Filterable().WithMultipleFilters();

            // Return items to displays
            return server.ItemsToDisplay;
        }

        protected void OnGuestSelected(object item)
        {
            if (item.GetType() == typeof(GuestEntity))
            {
                selectedGuest = (GuestEntity)item;
            }
        }

        private async Task RefreshList()
        {
            guests = await GuestRepo.GetAll().ToListAsync();

            await gridComponent.UpdateGrid();
            StateHasChanged();
        }

        private async Task AddGuest()
        {
            var modal = Modal.Show<GuestAdd>(Loc["AddGuest"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var entity = (GuestEntity)result.Data;
                await GuestRepo.Add(entity);
                await RefreshList();
            }
        }

        private async Task EditGuestInfo()
        {
            if (selectedGuest == null)
            {
                await ShowNoGuestSelectedWarning();
                return;
            }

            var parameters = new ModalParameters();
            parameters.Add(nameof(GuestEdit.Guest), selectedGuest);

            var modal = Modal.Show<GuestEdit>(Loc["EditGuest"], parameters);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                await GuestRepo.Update(selectedGuest);
                await RefreshList();
            }
        }

        private async Task EditGuestPassword()
        {
            if (selectedGuest == null)
            {
                await ShowNoGuestSelectedWarning();
                return;
            }

            var modal = Modal.Show<NewPasswordForm>($"{Loc["NewPasswordForGuest"]}: {selectedGuest.Username}");
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                selectedGuest.PasswordHash = BCrypt.Net.BCrypt.HashPassword(result.Data as string);
                await GuestRepo.Update(selectedGuest);
                await RefreshList();
            }
        }

        private async Task DeleteGuest()
        {
            if (selectedGuest == null)
            {
                await ShowNoGuestSelectedWarning();
                return;
            }

            if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {selectedGuest.Username}?", Loc["Cancel"]))
            {
                // We have to delete all the guest's jobs, proxy groups and wordlists first
                // otherwise we get a FOREIGN KEY CONSTRAINT FAILED exception.
                if (await js.Confirm(Loc["DeleteEverything"], Loc["DeleteEverythingMessage"], Loc["Cancel"]))
                {
                    // Delete jobs
                    var jobsToDelete = await JobRepo.GetAll().Include(j => j.Owner)
                        .Where(j => j.Owner.Id == selectedGuest.Id).ToListAsync();

                    await JobRepo.Delete(jobsToDelete);

                    // Delete proxy groups
                    var proxyGroupsToDelete = await ProxyGroupRepo.GetAll().Include(g => g.Owner)
                        .Where(g => g.Owner.Id == selectedGuest.Id).ToListAsync();

                    await ProxyGroupRepo.Delete(proxyGroupsToDelete);

                    // Delete wordlists
                    var wordlistsToDelete = await WordlistRepo.GetAll().Include(w => w.Owner)
                        .Where(w => w.Owner.Id == selectedGuest.Id).ToListAsync();

                    foreach (var w in wordlistsToDelete)
                    {
                        await WordlistRepo.Delete(w, false);
                    }

                    // Delete the guest from the db
                    await GuestRepo.Delete(selectedGuest);

                    // Delete the guest from the local list
                    guests.Remove(selectedGuest);
                }
            }

            await RefreshList();
        }

        private async Task ShowNoGuestSelectedWarning()
            => await js.AlertError(Loc["Uh-Oh"], Loc["NoGuestSelectedWarning"]);
    }
}
