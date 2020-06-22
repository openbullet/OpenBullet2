using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Shared.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Guests
    {
        [Inject] IModalService Modal { get; set; }
        [Inject] IGuestRepository GuestRepo { get; set; }

        private List<GuestEntity> guests;
        private GuestEntity selectedGuest;

        private int resultsPerPage = 15;

        protected override async Task OnInitializedAsync()
        {
            await RefreshList();

            await base.OnInitializedAsync();
        }

        private async Task OnResultsPerPageChanged(int value)
        {
            resultsPerPage = value;
            await RefreshList();
            StateHasChanged();
        }

        private void SelectGuest(GuestEntity guest)
        {
            selectedGuest = guest;
        }

        private async Task RefreshList()
        {
            guests = await GuestRepo.GetAll().ToListAsync();

            StateHasChanged();
        }

        private async Task AddGuest()
        {
            var modal = Modal.Show<GuestAdd>("Add guest");
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
                await js.AlertError("Hmm", "You must select a guest first");
                return;
            }

            var parameters = new ModalParameters();
            parameters.Add(nameof(GuestEdit.Guest), selectedGuest);

            var modal = Modal.Show<GuestEdit>("Edit guest", parameters);
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
                await js.AlertError("Hmm", "You must select a guest first");
                return;
            }

            var modal = Modal.Show<NewPasswordForm>("New guest password");
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
                await js.AlertError("Hmm", "You must select a guest first");
                return;
            }

            if (await js.Confirm("Are you sure?", $"Do you really want to delete {selectedGuest.Username}?"))
            {
                // Delete the guest from the db
                await GuestRepo.Delete(selectedGuest);

                // Delete the guest from the local list
                guests.Remove(selectedGuest);
            }
        }
    }
}
