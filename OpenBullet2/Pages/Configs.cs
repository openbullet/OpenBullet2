using BlazorDownloadFile;
using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Settings;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Extensions;
using RuriLib.Helpers;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Configs
    {
        [Inject] IConfigRepository ConfigRepo { get; set; }
        [Inject] NavigationManager Nav { get; set; }
        [Inject] ConfigService ConfigService { get; set; }
        [Inject] PersistentSettingsService PersistentSettings { get; set; }
        [Inject] IBlazorDownloadFileService BlazorDownloadFileService { get; set; }

        Config selectedConfig;
        List<Config> configs;
        bool detailedView = false;

        protected override void OnInitialized()
        {
            configs = ConfigService.Configs.OrderByDescending(c => c.Metadata.LastModified).ToList();
            selectedConfig = ConfigService.SelectedConfig;
        }

        private async Task ReloadConfigs()
        {
            if (await js.Confirm(Loc["AreYouSure"], Loc["ConfigReloadWarning"], Loc["Cancel"]))
            {
                ConfigService.Configs = await ConfigRepo.GetAll();
                configs = ConfigService.Configs.OrderByDescending(c => c.Metadata.LastModified).ToList();

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
            selectedConfig.Metadata.Author = PersistentSettings.OpenBulletSettings.GeneralSettings.DefaultAuthor;
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

            if (await js.Confirm(Loc["AreYouSure"], $"{Loc["ReallyDelete"]} {selectedConfig.Metadata.Name}?", Loc["Cancel"]))
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

            var section = PersistentSettings.OpenBulletSettings.GeneralSettings.ConfigSectionOnLoad;
            var uri = section switch
            {
                ConfigSection.Metadata => "config/edit/metadata",
                ConfigSection.Readme => "config/edit/readme",
                ConfigSection.Stacker => selectedConfig.Mode == ConfigMode.CSharp ? "config/edit/code" : "config/edit/stacker",
                ConfigSection.LoliCode => selectedConfig.Mode == ConfigMode.CSharp ? "config/edit/code" : "config/edit/lolicode",
                ConfigSection.Settings => "config/edit/settings",
                ConfigSection.CSharpCode => "config/edit/code",
                _ => throw new NotImplementedException()
            };

            Nav.NavigateTo(uri);
        }

        private async Task ProcessUploadedConfigs(IFileListEntry[] files)
        {
            if (files.Length == 0)
                return;

            try
            {
                foreach (var file in files)
                {
                    using var reader = new StreamReader(file.Data);
                    var ms = new MemoryStream();
                    await file.Data.CopyToAsync(ms);
                    await ConfigRepo.Upload(ms);
                }

                await js.AlertSuccess(Loc["AllDone"], $"{Loc["ConfigsSuccessfullyUploaded"]}: {files.Length}");
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

        private void ToggleView() => detailedView = !detailedView;
    }
}
