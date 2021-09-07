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
using Microsoft.Extensions.Primitives;
using OpenBullet2.Auth;
using OpenBullet2.Core.Services;
using OpenBullet2.Helpers;
using OpenBullet2.Services;
using RuriLib.Models.Configs;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Shared.Forms
{
    public partial class ConfigSelector
    {
        [Inject] private ConfigService ConfigService { get; set; }
        [Inject] private PluginRepository PluginRepo { get; set; }
        [Inject] private IModalService ModalService { get; set; }
        [Inject] private AuthenticationStateProvider Auth { get; set; }
        [Inject] private VolatileSettingsService VolatileSettings { get; set; }

        [CascadingParameter] BlazoredModalInstance BlazoredModal { get; set; }

        private List<Config> configs = new();
        private Config selectedConfig;
        private int uid = -1;

        private GridComponent<Config> gridComponent;
        private CGrid<Config> grid;
        private Task gridLoad;

        protected async override Task OnParametersSetAsync()
        {
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            configs = ConfigService.Configs.OrderByDescending(c => c.Metadata.LastModified).ToList();

            Action<IGridColumnCollection<Config>> columns = c =>
            {
                c.Add(x => x.Metadata.Name).Titled(Loc["Name"]).Encoded(false).Sanitized(false)
                    .RenderValueAs(x => $"<div class=\"grid-element-with-icon\"><img src=\"data:image/png;base64,{x.Metadata.Base64Image}\"/><span>{x.Metadata.Name}</span></div>");
                c.Add(x => x.Metadata.Author).Titled(Loc["Author"]);
                c.Add(x => x.Metadata.Category).Titled(Loc["Category"]);
                c.Add(x => x.IsRemote).Titled(Loc["Remote"]);
                c.Add(x => x.Settings.ProxySettings.UseProxies).Titled(Loc["Proxies"]);
                c.Add(x => x.Settings.DataSettings.AllowedWordlistTypesString).Titled(Loc["Wordlists"]);
                c.Add(x => x.Metadata.CreationDate).Titled(Loc["CreationDate"]);
                c.Add(x => x.Metadata.LastModified).Titled(Loc["LastModified"])
                    .Sortable(true).SortInitialDirection(GridShared.Sorting.GridSortDirection.Descending);
            };

            var query = new QueryDictionary<StringValues>();
            query.Add("grid-page", "1");

            var client = new GridClient<Config>(q => GetGridRows(columns, q), query, false, "configsGrid", columns, CultureInfo.CurrentCulture)
                .Sortable()
                .Filterable()
                .ChangePageSize(true)
                .WithMultipleFilters()
                .SetKeyboard(true)
                .Selectable(true, false, false);
            grid = client.Grid;

            // Try to set a previous filter
            if (VolatileSettings.GridQueries.ContainsKey((uid, "configsGrid")))
            {
                grid.Query = VolatileSettings.GridQueries[(uid, "configsGrid")];
            }

            // Set new items to grid
            gridLoad = client.UpdateGrid();
            await gridLoad;
        }

        private ItemsDTO<Config> GetGridRows(Action<IGridColumnCollection<Config>> columns,
                QueryDictionary<StringValues> query)
        {
            VolatileSettings.GridQueries[(uid, "configsGrid")] = query;

            var server = new GridServer<Config>(configs, new QueryCollection(query),
                true, "configsGrid", columns, 15).Sortable().Filterable().WithMultipleFilters();

            // Return items to displays
            return server.ItemsToDisplay;
        }

        protected void OnConfigSelected(object item)
        {
            if (item.GetType() == typeof(Config))
            {
                selectedConfig = (Config)item;
                StateHasChanged();
            }
        }

        private async Task Select()
        {
            if (selectedConfig == null)
            {
                await js.AlertError(Loc["Uh-Oh"], Loc["NoConfigSelected"]);
                return;
            }

            // Check if we have all required plugins
            var loadedPlugins = PluginRepo.GetPlugins();
            if (selectedConfig.Metadata.Plugins != null)
            {
                foreach (var plugin in selectedConfig.Metadata.Plugins)
                {
                    if (!loadedPlugins.Any(p => p.FullName == plugin))
                    {
                        if (!await js.Confirm(Loc["MissingPlugin"], $"{Loc["MissingPluginText"]}: {plugin}", Loc["Cancel"]))
                            return;
                    }
                }
            }

            if (selectedConfig.HasCSharpCode())
            {
                if (!await js.Confirm(Loc["Danger"], Loc["DangerousConfig"]))
                    return;
            }

            BlazoredModal.Close(ModalResult.Ok(selectedConfig));
        }
    }
}
