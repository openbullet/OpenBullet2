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
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Services;
using RuriLib.Extensions;
using RuriLib.Helpers;
using RuriLib.Models.Configs;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenBullet2.Core.Services;
using System.IO.Compression;

namespace OpenBullet2.Pages
{
    public partial class Configs : IDisposable
    {
        [Inject] private IConfigRepository ConfigRepo { get; set; }
        [Inject] private PluginRepository PluginRepo { get; set; }
        [Inject] private NavigationManager Nav { get; set; }
        [Inject] private ConfigService ConfigService { get; set; }
        [Inject] private VolatileSettingsService VolatileSettings { get; set; }
        [Inject] private OpenBulletSettingsService OBSettingsService { get; set; }
        [Inject] private IBlazorDownloadFileService BlazorDownloadFileService { get; set; }

        private Config selectedConfig;
        private List<Config> configs;
        private DateTime lastRowClickTime;

        private GridComponent<Config> gridComponent;
        private CGrid<Config> grid;
        private Task gridLoad;

        protected override Task OnParametersSetAsync()
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
                c.Add(x => x.Settings.DataSettings.AllowedWordlistTypesString).Titled(Loc["Wordlists"]);
                c.Add(x => x.Metadata.CreationDate).Titled(Loc["CreationDate"]).SetFilterWidgetType("DateTimeLocal").Format("{0:dd/MM/yyyy HH:mm}");
                c.Add(x => x.Metadata.LastModified).Titled(Loc["LastModified"]).SetFilterWidgetType("DateTimeLocal").Format("{0:dd/MM/yyyy HH:mm}")
                    .Sortable(true).SortInitialDirection(GridShared.Sorting.GridSortDirection.Descending);
            };

            var query = new QueryDictionary<StringValues>();
            query.Add("grid-page", "1");

            var client = new GridClient<Config>(q => GetGridRows(columns, q), query, false, "configsGrid", columns, CultureInfo.CurrentCulture)
                .Sortable()
                .ExtSortable()
                .Filterable()
                .WithMultipleFilters()
                .SetKeyboard(true)
                .ChangePageSize(true)
                .WithGridItemsCount()
                .Selectable(true, false, false);
            grid = client.Grid;

            // Try to set a previous filter
            if (VolatileSettings.GridQueries.ContainsKey((0, "configsGrid")))
            {
                grid.Query = VolatileSettings.GridQueries[(0, "configsGrid")];
            }

            // Set new items to grid
            gridLoad = client.UpdateGrid();
            return gridLoad;
        }

        private ItemsDTO<Config> GetGridRows(Action<IGridColumnCollection<Config>> columns,
                QueryDictionary<StringValues> query)
        {
            VolatileSettings.GridQueries[(0, "configsGrid")] = query;

            var server = new GridServer<Config>(configs, new QueryCollection(query),
                true, "configsGrid", columns, 10).Sortable().Filterable().WithMultipleFilters();

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
                // If the row was double clicked, edit the config
                if (selectedConfig == (Config)item && (DateTime.Now - lastRowClickTime).TotalMilliseconds < 500)
                {
                    lastRowClickTime = DateTime.Now;
                    _ = EditConfig();
                }
                else
                {
                    selectedConfig = (Config)item;
                    lastRowClickTime = DateTime.Now;
                    StateHasChanged();
                }
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
            
            selectedConfig.Metadata.Author = OBSettingsService.Settings.GeneralSettings.DefaultAuthor;
            VolatileSettings.DebuggerLog = new();
            Nav.NavigateTo("config/edit/metadata");
        }

        private async Task CloneConfig()
        {
            if (selectedConfig == null)
            {
                await ShowNoConfigSelectedMessage();
                return;
            }

            if (selectedConfig.IsRemote)
            {
                await js.AlertError(Loc["RemoteConfig"], Loc["CannotCloneRemoteConfig"]);
                return;
            }

            // Pack and unpack to clone
            var packed = await ConfigPacker.Pack(selectedConfig);
            using var ms = new MemoryStream(packed);
            var newConfig = await ConfigPacker.Unpack(ms);
            
            // Change the id and save it again
            newConfig.Id = Guid.NewGuid().ToString();
            await ConfigRepo.Save(newConfig);

            // Set it as currently selected config
            configs.Insert(0, newConfig);
            selectedConfig = newConfig;
            ConfigService.Configs.Add(selectedConfig);
            ConfigService.SelectedConfig = selectedConfig;

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
                ConfigService.Configs.Remove(selectedConfig);
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

            var section = OBSettingsService.Settings.GeneralSettings.ConfigSectionOnLoad;
            var uri = string.Empty;

            if (selectedConfig.Mode == ConfigMode.DLL)
            {
                uri = "config/edit/metadata";
            }
            else if (selectedConfig.Mode == ConfigMode.Legacy)
            {
                uri = section switch
                {
                    ConfigSection.Metadata => "config/edit/metadata",
                    ConfigSection.Readme => "config/edit/readme",
                    ConfigSection.Settings => "config/edit/settings",
                    _ => "config/edit/loliscript"
                };
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
                    _ => "config/edit/metadata"
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
                    ms.Seek(0, SeekOrigin.Begin);

                    // Upload it to the repo
                    await ConfigRepo.Upload(ms, file.Name);
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

        private async Task DownloadAll()
        {
            // Only download configs that are not remote
            var configsToPack = configs.Where(c => !c.IsRemote);

            if (!configsToPack.Any())
            {
                return;
            }

            try
            {
                var bytes = await ConfigPacker.Pack(configsToPack);
                await BlazorDownloadFileService.DownloadFile("configs.zip", bytes, "application/octet-stream");
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
        {
            RemoveEventHandlers();
            GC.SuppressFinalize(this);
        }

        ~Configs()
            => Dispose();
    }
}
