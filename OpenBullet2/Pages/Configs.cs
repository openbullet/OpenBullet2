using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace OpenBullet2.Pages
{
    public partial class Configs
    {
        [Inject] IConfigRepository ConfigRepo { get; set; }
        [Inject] NavigationManager Nav { get; set; }
        [Inject] ConfigService ConfigService { get; set; }

        Config selectedConfig;
        List<Config> configs;
        bool detailedView = false;
        bool uploadDisplay = false;

        protected override void OnInitialized()
        {
            configs = ConfigService.Configs;
            selectedConfig = ConfigService.SelectedConfig;
        }

        private async Task ReloadConfigs()
        {
            if (await js.Confirm("Are you sure?", "This will reload all configs from disk, so all unsaved changes will be lost. Do you want to proceed?"))
            {
                ConfigService.Configs = await ConfigRepo.GetAll();
                configs = ConfigService.Configs;
                
                ConfigService.SelectedConfig = null;
                selectedConfig = null;
            }
        }

        private void SelectConfig(Config config)
        {
            selectedConfig = config;
        }

        private async Task CreateConfig()
        {
            selectedConfig = await ConfigRepo.Create();
            configs.Add(selectedConfig);
            ConfigService.SelectedConfig = selectedConfig;
            Nav.NavigateTo("config/edit/metadata");
        }

        private async Task DeleteConfig()
        {
            if (selectedConfig == null)
            {
                await ShowNoConfigSelectedMessage();
                return;
            }

            if (await js.Confirm("Are you sure?", $"Do you really want to delete {selectedConfig.Metadata.Name}?"))
            {
                ConfigRepo.Delete(selectedConfig);
                configs.Remove(selectedConfig);

                if (ConfigService.SelectedConfig == selectedConfig)
                    ConfigService.SelectedConfig = null;

                selectedConfig = null;
            }
        }

        private async Task EditConfig()
        {
            if (selectedConfig == null)
            {
                await ShowNoConfigSelectedMessage();
                return;
            }

            ConfigService.SelectedConfig = selectedConfig;

            if (selectedConfig.Mode == ConfigMode.CSharp)
                Nav.NavigateTo("config/edit/code");
            else
                Nav.NavigateTo("config/edit/stacker");
        }

        private async Task ProcessUploadedConfigs(IFileListEntry[] files)
        {
            if (files.Length == 0)
                return;

            try
            {
                foreach (var file in files)
                {
                    using (var reader = new StreamReader(file.Data))
                    {
                        var ms = new MemoryStream();
                        await file.Data.CopyToAsync(ms);
                        await ConfigRepo.Upload(ms);
                    }
                }

                await js.AlertSuccess("All done!", $"Successfully uploaded {files.Length} configs");
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().ToString(), ex.Message);
            }

            await ReloadConfigs();
            uploadDisplay = false;
        }

        private async Task ShowNoConfigSelectedMessage()
            => await js.AlertError("404", "It looks like you didn't select any config!");


        private async Task DownloadConfig()
        {
            if (selectedConfig == null)
            {
                await ShowNoConfigSelectedMessage();
                return;
            }

            Nav.NavigateTo($"api/configs/download/{HttpUtility.UrlEncode(selectedConfig.Id)}", true);
        }

        private void ToggleView() => detailedView = !detailedView;
    }
}
