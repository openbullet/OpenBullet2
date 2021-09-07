using Blazored.Modal;
using Blazored.Modal.Services;
using GridBlazor;
using GridBlazor.Pages;
using GridMvc.Server;
using GridShared;
using GridShared.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Primitives;
using OpenBullet2.Helpers;
using OpenBullet2.Core.Models.Sharing;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OpenBullet2.Core.Services;

namespace OpenBullet2.Pages
{
    public partial class Sharing
    {
        [Inject] private IModalService Modal { get; set; }
        [Inject] private ConfigSharingService ConfigSharing { get; set; }
        [Inject] private ConfigService ConfigService { get; set; }
        [Inject] private VolatileSettingsService VolatileSettings { get; set; }

        private string selectedEndpointName = string.Empty;
        private Endpoint selectedEndpoint;
        private List<Config> configs = new();
        private Config selectedConfig;

        private GridComponent<Config> gridComponent;
        private CGrid<Config> grid;
        private Task gridLoad;

        protected override async Task OnParametersSetAsync()
        {
            selectedEndpoint = ConfigSharing.Endpoints.FirstOrDefault();

            if (selectedEndpoint != null)
            {
                selectedEndpointName = selectedEndpoint.Route;
            }

            configs = selectedEndpoint != null
                ? ConfigService.Configs.Where(c => selectedEndpoint.ConfigIds.Contains(c.Id)).ToList()
                : new();

            Action<IGridColumnCollection<Config>> columns = c =>
            {
                c.Add(x => x.Metadata.Name).Titled(Loc["Name"]).Encoded(false).Sanitized(false)
                    .RenderValueAs(x => $"<div class=\"grid-element-with-icon\"><img src=\"data:image/png;base64,{x.Metadata.Base64Image}\"/><span>{x.Metadata.Name}</span></div>");
                c.Add(x => x.Metadata.Author).Titled(Loc["Author"]);
                c.Add(x => x.Metadata.Category).Titled(Loc["Category"]);
                c.Add(x => x.IsRemote).Titled(Loc["Remote"]);
                c.Add(x => x.Settings.ProxySettings.UseProxies).Titled(Loc["Proxies"]);
                c.Add(x => x.Metadata.CreationDate).Titled(Loc["CreationDate"]);
                c.Add(x => x.Metadata.LastModified).Titled(Loc["LastModified"])
                    .Sortable(true).SortInitialDirection(GridShared.Sorting.GridSortDirection.Descending);
            };

            var query = new QueryDictionary<StringValues>();
            query.Add("grid-page", "1");

            var client = new GridClient<Config>(q => GetGridRows(columns, q), query, false, "sharingGrid", columns, CultureInfo.CurrentCulture)
                .Sortable()
                .Filterable()
                .WithMultipleFilters()
                .SetKeyboard(true)
                .ChangePageSize(true)
                .WithGridItemsCount()
                .Selectable(true, false, false);
            grid = client.Grid;

            // Try to set a previous filter
            if (VolatileSettings.GridQueries.ContainsKey((0, "sharingGrid")))
            {
                grid.Query = VolatileSettings.GridQueries[(0, "sharingGrid")];
            }

            // Set new items to grid
            gridLoad = client.UpdateGrid();
            await gridLoad;
        }

        private ItemsDTO<Config> GetGridRows(Action<IGridColumnCollection<Config>> columns,
                QueryDictionary<StringValues> query)
        {
            VolatileSettings.GridQueries[(0, "sharingGrid")] = query;

            var server = new GridServer<Config>(configs, new Microsoft.AspNetCore.Http.QueryCollection(query),
                true, "sharingGrid", columns, 15).Sortable().Filterable().WithMultipleFilters();

            // Return items to displays
            return server.ItemsToDisplay;
        }

        private async Task RefreshList()
        {
            configs = selectedEndpoint != null
                ? ConfigService.Configs.Where(c => selectedEndpoint.ConfigIds.Contains(c.Id)).ToList()
                : new();

            await gridComponent.UpdateGrid();
            StateHasChanged();
        }

        private async Task OnEndpointSelected(string name)
        {
            selectedEndpointName = name;
            selectedEndpoint = ConfigSharing.Endpoints.First(e => e.Route == name);
            await RefreshList();
        }

        private void OnConfigSelected(object item)
        {
            if (item.GetType() == typeof(Config))
            {
                selectedConfig = (Config)item;
            }
        }

        private async Task CreateEndpoint()
        {
            var modal = Modal.Show<EndpointCreate>(Loc["CreateEndpoint"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                selectedEndpoint = result.Data as Endpoint;
                selectedEndpointName = selectedEndpoint.Route;
                ConfigSharing.Endpoints.Add(selectedEndpoint);
                ConfigSharing.Save();
                await js.AlertSuccess(Loc["Created"], Loc["EndpointCreated"]);
                await RefreshList();
            }
        }

        private async Task EditEndpoint()
        {
            if (selectedEndpoint == null)
            {
                await ShowNoEndpointSelectedWarning();
                return;
            }

            var parameters = new ModalParameters();
            parameters.Add(nameof(EndpointEdit.Endpoint), selectedEndpoint);

            var modal = Modal.Show<EndpointEdit>(Loc["EditEndpoint"], parameters);
            await modal.Result;
            ConfigSharing.Save();
            await RefreshList();
        }

        private async Task DeleteEndpoint()
        {
            if (selectedEndpoint == null)
            {
                await ShowNoEndpointSelectedWarning();
                return;
            }

            if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {selectedEndpoint.Route}?"))
            {
                ConfigSharing.Endpoints.Remove(selectedEndpoint);
                ConfigSharing.Save();

                selectedEndpoint = ConfigSharing.Endpoints.FirstOrDefault();
                if (selectedEndpoint != null)
                {
                    selectedEndpointName = selectedEndpoint.Route;
                }

                await RefreshList();
            }
        }

        private async Task AddConfig()
        {
            if (selectedEndpoint == null)
            {
                await ShowNoEndpointSelectedWarning();
                return;
            }

            var modal = Modal.Show<ConfigSelector>(Loc["SelectConfig"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var config = result.Data as Config;
                
                if (config.IsRemote)
                {
                    await js.AlertError(Loc["RemoteConfig"], Loc["CannotShareRemoteConfig"]);
                    return;
                }

                if (!selectedEndpoint.ConfigIds.Contains(config.Id))
                {
                    selectedEndpoint.ConfigIds.Add(config.Id);
                }
                ConfigSharing.Save();
                await RefreshList();
            }
        }

        private async Task RemoveConfig()
        {
            if (selectedConfig == null)
            {
                await ShowNoConfigSelectedWarning();
                return;
            }

            selectedEndpoint.ConfigIds.Remove(selectedConfig.Id);
            ConfigSharing.Save();

            await RefreshList();
            await js.AlertSuccess(Loc["Removed"], Loc["ConfigRemovedSuccessfully"]);
        }

        private async Task RemoveAllConfigs()
        {
            if (selectedEndpoint == null)
            {
                await ShowNoEndpointSelectedWarning();
                return;
            }

            var count = selectedEndpoint.ConfigIds.Count;
            selectedEndpoint.ConfigIds.Clear();
            ConfigSharing.Save();
            
            await RefreshList();
            await js.AlertSuccess(Loc["Removed"], $"{Loc["ConfigsRemovedSuccessfully"]}: {count}");
        }

        private async Task ShowNoEndpointSelectedWarning()
            => await js.AlertError(Loc["Uh-Oh"], Loc["NoEndpointSelected"]);

        private async Task ShowNoConfigSelectedWarning()
            => await js.AlertError(Loc["Uh-Oh"], Loc["NoConfigSelected"]);
    }
}
