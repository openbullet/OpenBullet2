using BlazorDownloadFile;
using GridBlazor;
using GridBlazor.Pages;
using GridMvc.Server;
using GridShared;
using GridShared.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Settings;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Extensions;
using RuriLib.Functions.Crypto;
using RuriLib.Helpers;
using RuriLib.Models.Configs;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Configs : IDisposable
    {
        [Inject] IConfigRepository ConfigRepo { get; set; }
        [Inject] PluginRepository PluginRepo { get; set; }
        [Inject] NavigationManager Nav { get; set; }
        [Inject] ConfigService ConfigService { get; set; }
        [Inject] VolatileSettingsService VolatileSettings { get; set; }
        [Inject] PersistentSettingsService PersistentSettings { get; set; }
        [Inject] IBlazorDownloadFileService BlazorDownloadFileService { get; set; }

        Config selectedConfig;
        List<Config> configs;

        private GridComponent<Config> gridComponent;
        private CGrid<Config> grid;
        private Task gridLoad;

        protected override async Task OnParametersSetAsync()
        {
            AddEventHandlers();

            configs = ConfigService.Configs.OrderByDescending(c => c.Metadata.LastModified).ToList();
            selectedConfig = ConfigService.SelectedConfig;

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
            query.Add("grid-page", "2");

            var client = new GridClient<Config>(q => GetGridRows(columns, q), query, false, "configsGrid", columns, CultureInfo.CurrentCulture)
                .Sortable()
                .Filterable()
                .WithMultipleFilters()
                .SetKeyboard(true)
                .ChangePageSize(true)
                .WithGridItemsCount()
                .Selectable(true, false, false);
            grid = client.Grid;

            // Set new items to grid
            gridLoad = client.UpdateGrid();
            await gridLoad;
        }

        private ItemsDTO<Config> GetGridRows(Action<IGridColumnCollection<Config>> columns,
                QueryDictionary<StringValues> query)
        {
            var server = new GridServer<Config>(configs, new QueryCollection(query),
                true, "configsGrid", columns, 15).Sortable().Filterable().WithMultipleFilters();

            // Return items to displays
            return server.ItemsToDisplay;
        }

        private void SelectConfig(Config config)
        {
            selectedConfig = config;
        }

        private void OnConfigSelected(object item)
        {
            if (item.GetType() == typeof(Config))
            {
                selectedConfig = (Config)item;
            }
        }

        private async Task RefreshList()
        {
            configs = ConfigService.Configs.OrderByDescending(c => c.Metadata.LastModified).ToList();
            await RefreshGrid();
        }

        private async Task RefreshGrid()
        {
            await gridComponent.UpdateGrid();
            StateHasChanged();
        }

        private async Task ReloadConfigs()
        {
            if (await js.Confirm(Loc["AreYouSure"], Loc["ConfigReloadWarning"], Loc["Cancel"]))
            {
                await ConfigService.ReloadConfigs();
                configs = ConfigService.Configs.OrderByDescending(c => c.Metadata.LastModified).ToList();

                selectedConfig = null;

                if (VolatileSettings.ConfigsDetailedView)
                {
                    await RefreshGrid();
                }
            }
        }

        private async Task CreateConfig()
        {
            selectedConfig = await ConfigRepo.Create();
            configs.Insert(0, selectedConfig);

            ConfigService.SelectedConfig = selectedConfig;
            ConfigService.Configs.Add(selectedConfig);
            
            selectedConfig.Metadata.Author = PersistentSettings.OpenBulletSettings.GeneralSettings.DefaultAuthor;
            VolatileSettings.DebuggerLog = new();
            Nav.NavigateTo("config/edit/metadata");
        }

        private async Task DeleteConfig()
        {
            if (selectedConfig == null)
            {
                await ShowNoConfigSelectedMessage();
                return;
            }

            if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {selectedConfig.Metadata.Name}?", Loc["Cancel"]))
            {
                ConfigRepo.Delete(selectedConfig);
                configs.Remove(selectedConfig);

                if (ConfigService.SelectedConfig == selectedConfig)
                {
                    ConfigService.SelectedConfig = null;
                }

                selectedConfig = null;

                if (VolatileSettings.ConfigsDetailedView)
                {
                    await RefreshGrid();
                }
                else
                {
                    StateHasChanged();
                }
            }
        }

        private async Task EditConfig()
        {
            if (selectedConfig == null)
            {
                await ShowNoConfigSelectedMessage();
                return;
            }

            if (selectedConfig.IsRemote)
            {
                await js.AlertError(Loc["RemoteConfig"], Loc["CannotEditRemoteConfig"]);
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
                        await js.AlertWarning(Loc["MissingPlugin"], $"{Loc["MissingPluginText"]}: {plugin}");
                    }
                }
            }

            // Check if the previous config was saved
            if (ConfigService.SelectedConfig != null && ConfigService.SelectedConfig != selectedConfig && ConfigService.SelectedConfig.HasUnsavedChanges())
            {
                if (!await js.Confirm(Loc["UnsavedChanges"], Loc["UnsavedChangesText"], Loc["Cancel"]))
                    return;
            }

            ConfigService.SelectedConfig = selectedConfig;

            var section = PersistentSettings.OpenBulletSettings.GeneralSettings.ConfigSectionOnLoad;
            var uri = string.Empty;

            if (selectedConfig.Mode == ConfigMode.DLL)
            {
                uri = "config/edit/metadata";
            }
            else
            {
                uri = section switch
                {
                    ConfigSection.Metadata => "config/edit/metadata",
                    ConfigSection.Readme => "config/edit/readme",
                    ConfigSection.Stacker => selectedConfig.Mode == ConfigMode.CSharp ? "config/edit/code" : "config/edit/stacker",
                    ConfigSection.LoliCode => selectedConfig.Mode == ConfigMode.CSharp ? "config/edit/code" : "config/edit/lolicode",
                    ConfigSection.Settings => "config/edit/settings",
                    ConfigSection.CSharpCode => "config/edit/code",
                    _ => throw new NotImplementedException()
                };
            }

            VolatileSettings.DebuggerLog = new();
            Nav.NavigateTo(uri);
        }

        private async Task ProcessUploadedConfigs(InputFileChangeEventArgs e)
        {
            if (e.FileCount == 0)
                return;

            try
            {
                // Support maximum 5000 files at a time
                foreach (var file in e.GetMultipleFiles(5000))
                {
                    // Support maximum 5 MB per file
                    var stream = file.OpenReadStream(5 * 1000 * 1000);

                    // Copy the content to a MemoryStream
                    using var reader = new StreamReader(stream);
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);

                    // Upload it to the repo
                    await ConfigRepo.Upload(ms);
                }

                await js.AlertSuccess(Loc["AllDone"], $"{Loc["ConfigsSuccessfullyUploaded"]}: {e.FileCount}");
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().Name, ex.Message);
            }

            await ReloadConfigs();
            uploadDisplay = false;
        }

        private async Task ShowNoConfigSelectedMessage()
            => await js.AlertError(Loc["Uh-Oh"], Loc["NoConfigSelectedWarning"]);


        private async Task DownloadConfig()
        {
            if (selectedConfig == null)
            {
                await ShowNoConfigSelectedMessage();
                return;
            }

            if (selectedConfig.IsRemote)
            {
                await js.AlertError(Loc["RemoteConfig"], Loc["CannotDownloadRemoteConfig"]);
                return;
            }

            try
            {
                var fileName = selectedConfig.Metadata.Name.ToValidFileName() + ".opk";
                await BlazorDownloadFileService.DownloadFile(fileName, await ConfigPacker.Pack(selectedConfig), "application/octet-stream");
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().Name, ex.Message);
                return;
            }
        }

        private void ToggleView()
            => VolatileSettings.ConfigsDetailedView = !VolatileSettings.ConfigsDetailedView;

        private void AddEventHandlers()
        {
            ConfigService.OnRemotesLoaded += RemotesLoaded;
        }

        private void RemotesLoaded(object sender, EventArgs e)
            => InvokeAsync(RefreshList);

        private void RemoveEventHandlers()
        {
            try 
            {
                ConfigService.OnRemotesLoaded -= RemotesLoaded; 
            }
            catch 
            {

            }
        }

        public void Dispose()
            => RemoveEventHandlers();

        ~Configs()
            => Dispose();
    }
}
