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
using OpenBullet2.Core.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Shared.Forms
{
    public partial class WordlistSelector
    {
        [Inject] private AuthenticationStateProvider Auth { get; set; }
        [Inject] private IWordlistRepository WordlistRepo { get; set; }
        [Inject] private IModalService ModalService { get; set; }
        [Inject] private VolatileSettingsService VolatileSettings { get; set; }

        [CascadingParameter] BlazoredModalInstance BlazoredModal { get; set; }

        private List<WordlistEntity> wordlists = new();
        private WordlistEntity selectedWordlist;
        private string linesPreview = string.Empty;
        private int uid = -1;

        private GridComponent<WordlistEntity> gridComponent;
        private CGrid<WordlistEntity> grid;
        private Task gridLoad;

        protected override async Task OnParametersSetAsync()
        {
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            wordlists = uid == 0
                ? await WordlistRepo.GetAll().ToListAsync()
                : await WordlistRepo.GetAll().Include(w => w.Owner).Where(w => w.Owner.Id == uid).ToListAsync();

            Action<IGridColumnCollection<WordlistEntity>> columns = c =>
            {
                c.Add(w => w.Name).Titled(Loc["Name"]);
                c.Add(w => w.Type).Titled(Loc["Type"]);
                c.Add(w => w.Purpose).Titled(Loc["Purpose"]);
                c.Add(w => w.Total).Titled(Loc["Lines"]);
                c.Add(w => w.FileName).Titled(Loc["FileName"]);
            };

            var query = new QueryDictionary<StringValues>();
            query.Add("grid-page", "1");

            var client = new GridClient<WordlistEntity>(q => GetGridRows(columns, q), query, false, "wordlistsGrid", columns, CultureInfo.CurrentCulture)
                .Sortable()
                .Filterable()
                .ChangePageSize(true)
                .WithMultipleFilters()
                .SetKeyboard(true)
                .Selectable(true, false, false);
            grid = client.Grid;

            // Try to set a previous filter
            if (VolatileSettings.GridQueries.ContainsKey((uid, "wordlistsGrid")))
            {
                grid.Query = VolatileSettings.GridQueries[(uid, "wordlistsGrid")];
            }

            // Set new items to grid
            gridLoad = client.UpdateGrid();
            await gridLoad;
        }

        private ItemsDTO<WordlistEntity> GetGridRows(Action<IGridColumnCollection<WordlistEntity>> columns,
                QueryDictionary<StringValues> query)
        {
            VolatileSettings.GridQueries[(uid, "wordlistsGrid")] = query;

            var server = new GridServer<WordlistEntity>(wordlists, new QueryCollection(query),
                true, "wordlistsGrid", columns, 15).Sortable().Filterable().WithMultipleFilters();

            // Return items to displays
            return server.ItemsToDisplay;
        }

        protected void OnWordlistSelected(object item)
        {
            if (item.GetType() == typeof(WordlistEntity))
            {
                selectedWordlist = (WordlistEntity)item;
                PreviewSelected();
            }
        }

        private void PreviewSelected()
        {
            var previewAmount = Math.Min(selectedWordlist.Total, 10);

            try
            {
                var lines = System.IO.File.ReadLines(selectedWordlist.FileName).Take(previewAmount);
                linesPreview = string.Join(Environment.NewLine, lines);
            }
            catch
            {
                linesPreview = string.Empty;
            }

            StateHasChanged();
        }

        private async Task Select()
        {
            if (selectedWordlist == null)
            {
                await js.AlertError(Loc["Uh-Oh"], Loc["SelectWordlistFirst"]);
                return;
            }

            BlazoredModal.Close(ModalResult.Ok(selectedWordlist));
        }
    }
}
