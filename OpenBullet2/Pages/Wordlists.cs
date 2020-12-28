using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Auth;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Shared.Forms;
using Radzen.Blazor;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Linq;

namespace OpenBullet2.Pages
{
    public partial class Wordlists
    {
        [Inject] IModalService Modal { get; set; }
        [Inject] IWordlistRepository WordlistRepo { get; set; }
        [Inject] public IGuestRepository GuestRepo { get; set; }
        [Inject] AuthenticationStateProvider Auth { get; set; }

        private List<WordlistEntity> wordlists = new();
        private WordlistEntity selectedWordlist;
        private int uid = -1;

        RadzenGrid<WordlistEntity> wordlistsGrid;
        private int resultsPerPage = 15;

        protected override async Task OnInitializedAsync()
        {
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();
            await RefreshList();

            await base.OnInitializedAsync();
        }

        private async Task OnResultsPerPageChanged(int value)
        {
            resultsPerPage = value;
            await RefreshList();
            StateHasChanged();
        }

        private void SelectWordlist(WordlistEntity wordlist)
        {
            selectedWordlist = wordlist;
        }

        private async Task RefreshList()
        {
            wordlists = uid == 0
                ? await WordlistRepo.GetAll().ToListAsync()
                : await WordlistRepo.GetAll().Include(w => w.Owner).Where(w => w.Owner.Id == uid).ToListAsync();

            StateHasChanged();
        }

        private async Task AddWordlist()
        {
            var modal = Modal.Show<WordlistAdd>(Loc["AddWordlist"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var entity = result.Data as WordlistEntity;
                entity.Owner = await GuestRepo.Get(uid);
                await WordlistRepo.Add(entity);
                wordlists.Add(entity);
                await js.AlertSuccess(Loc["Added"], Loc["AddedWordlist"]);
            }

            await RefreshList();
        }

        private async Task EditWordlist()
        {
            if (selectedWordlist == null)
            {
                await ShowNoWordlistSelectedWarning();
                return;
            }

            var parameters = new ModalParameters();
            parameters.Add(nameof(WordlistEdit.Wordlist), selectedWordlist);

            var modal = Modal.Show<WordlistEdit>(Loc["EditWordlist"], parameters);
            await modal.Result;

            await RefreshList();
        }

        private async Task DeleteWordlist()
        {
            if (selectedWordlist == null)
            {
                await ShowNoWordlistSelectedWarning();
                return;
            }

            if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {selectedWordlist.Name}?", Loc["Cancel"]))
            {
                var deleteFile = await js.Confirm(Loc["AlsoDeleteFile"], 
                    $"{Loc["DeleteFileText1"]} {selectedWordlist.FileName} {Loc["DeleteFileText2"]}", Loc["KeepFile"]);

                // Delete the wordlist from the DB and disk
                await WordlistRepo.Delete(selectedWordlist, deleteFile);
            }

            await RefreshList();
        }

        private async Task ShowNoWordlistSelectedWarning()
            => await js.AlertError(Loc["Uh-Oh"], Loc["NoWordlistSelected"]);
    }
}
