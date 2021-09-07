using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using OpenBullet2.Helpers;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Repositories;
using OpenBullet2.Shared.Forms;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenBullet2.Core.Services;

namespace OpenBullet2.Pages
{
    public partial class OBSettings
    {
        [Inject] private OpenBulletSettingsService SettingsService { get; set; }
        [Inject] private IThemeRepository ThemeRepo { get; set; }
        [Inject] private NavigationManager Nav { get; set; }
        [Inject] private IModalService Modal { get; set; }

        private OpenBulletSettings settings;
        private string[] availableThemes = Array.Empty<string>();

        protected override async Task OnInitializedAsync()
        {
            settings = SettingsService.Settings;

            try
            {
                availableThemes = (await ThemeRepo.GetNames()).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not fetch themes: {ex.Message}");
            }
        }

        private async Task RestoreDefaults()
        {
            if (await js.Confirm(Loc["AreYouSure"], Loc["RestoreDefaultSettingsConfirmation"], Loc["Cancel"]))
            {
                SettingsService.Recreate();
                Nav.NavigateTo("/settings/openbullet", true);
            }
        }

        private async Task Save()
        {
            try
            {
                await SettingsService.Save();
                await js.AlertSuccess(Loc["Saved"], Loc["SettingsSaved"]);
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().Name, ex.Message);
            }
        }

        private async Task ChangePassword()
        {
            var modal = Modal.Show<NewPasswordForm>(Loc["NewAdminPassword"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                settings.SecuritySettings.SetupAdminPassword(result.Data as string);
                await js.AlertSuccess(Loc["Done"], Loc["NewPasswordSet"]);
            }
        }

        private void OnThemeChanged(string value)
        {
            settings.CustomizationSettings.Theme = value;
            Nav.NavigateTo("/settings/openbullet", true);
        }

        private async Task ProcessUploadedTheme(InputFileChangeEventArgs e)
        {
            if (e.FileCount == 0)
                return;

            try
            {
                // Support maximum 5 MB per file
                var stream = e.File.OpenReadStream(5 * 1000 * 1000);

                // Copy the content to a MemoryStream
                using var reader = new StreamReader(stream);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                if (e.File.Name.EndsWith(".css"))
                {
                    await ThemeRepo.AddFromCssFile(e.File.Name, ms);
                }
                else if (e.File.Name.EndsWith(".zip"))
                {
                    await ThemeRepo.AddFromZipArchive(ms);
                }
                else
                {
                    throw new NotSupportedException(Loc["UnsupportedThemeFormat"]);
                }

                await js.AlertSuccess(Loc["AllDone"], $"{Loc["ThemeSuccessfullyUploaded"]}: {e.File.Name}");
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().Name, ex.Message);
            }

            Nav.NavigateTo("/settings/openbullet", true);
        }

        private async Task PlayHitSound() => await js.InvokeVoidAsync("playHitSound");
    }
}
