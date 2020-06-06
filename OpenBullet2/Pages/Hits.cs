using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Shared.Forms;
using Radzen.Blazor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Hits
    {
        [Inject] IModalService Modal { get; set; }
        [Inject] IHitRepository HitRepo { get; set; }

        private List<HitEntity> hits;
        private HitEntity selectedHit;

        RadzenGrid<HitEntity> hitsGrid;
        private int resultsPerPage = 15;

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
            await RefreshList();

            await base.OnInitializedAsync();
        }

        private async Task OnResultsPerPageChanged(int value)
        {
            resultsPerPage = value;
            await RefreshList();
            StateHasChanged();
        }

        private void SelectHit(HitEntity hit)
        {
            selectedHit = hit;
        }

        private async Task RefreshList()
        {
            hits = await HitRepo.GetAll().ToListAsync();

            StateHasChanged();
        }

        private async Task EditHit()
        {
            if (selectedHit == null)
            {
                await js.AlertError("Hmm", "You must select a hit first");
                return;
            }

            var parameters = new ModalParameters();
            parameters.Add(nameof(HitEdit.Hit), selectedHit);

            var modal = Modal.Show<HitEdit>("Edit hit", parameters);
            await modal.Result;
        }

        private async Task DeleteHit()
        {
            if (selectedHit == null)
            {
                await js.AlertError("Hmm", "You must select a hit first");
                return;
            }

            if (await js.Confirm("Are you sure?", $"Do you really want to delete {selectedHit.Data}?"))
            {
                // Delete the hit from the db
                await HitRepo.Delete(selectedHit);

                // Delete the hit from the local list
                hits.Remove(selectedHit);
            }
        }

        private async Task PurgeHits()
        {
            if (await js.Confirm("Are you sure?", $"Do you really want to DELETE ALL of your hits?"))
            {
                HitRepo.Purge();
                await RefreshList();
            }
        }
    }
}
