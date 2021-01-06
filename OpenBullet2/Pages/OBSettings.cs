using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Scripting.Utils;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Settings;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class OBSettings
    {
        [Inject] public PersistentSettingsService PersistentSettings { get; set; }
        [Inject] public NavigationManager Nav { get; set; }
        [Inject] public IModalService Modal { get; set; }

        OpenBulletSettings settings;
        string[] availableThemes = Array.Empty<string>();

        protected override void OnInitialized()
        {
            settings = PersistentSettings.OpenBulletSettings;

            try
            {
                availableThemes = Directory.GetFiles("wwwroot/css/themes")
                    .Where(f => f.EndsWith(".css"))
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not fetch themes: {ex.Message}");
            }
        }

        async Task RestoreDefaults()
        {
            if (await js.Confirm(Loc["AreYouSure"], Loc["RestoreDefaultSettingsConfirmation"], Loc["Cancel"]))
            {
                PersistentSettings.Recreate();
                Nav.NavigateTo("/settings/openbullet", true);
            }
        }

        async Task Save()
        {
            try
            {
                await PersistentSettings.Save();
                await js.AlertSuccess(Loc["Saved"], Loc["SettingsSaved"]);
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().Name, ex.Message);
            }
        }

        async Task ChangePassword()
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
            settings.AppearanceSettings.Theme = value;
            Nav.NavigateTo("/settings/openbullet", true);
        }

        private async Task ProcessUploadedTheme(InputFileChangeEventArgs e)
        {
            if (e.FileCount == 0)
                return;

            if (!e.File.Name.EndsWith(".css"))
            {
                await js.AlertError(Loc["NotACssFile"], Loc["NotACssFileText"]);
                return;
            }

            try
            {
                // Support maximum 5 MB per file
                var stream = e.File.OpenReadStream(5 * 1000 * 1000);

                // Copy the content to a FileStream
                using var reader = new StreamReader(stream);
                using var fs = new FileStream($"wwwroot/css/themes/{e.File.Name}", FileMode.Create);
                await stream.CopyToAsync(fs);

                await js.AlertSuccess(Loc["AllDone"], $"{Loc["ThemeSuccessfullyUploaded"]}: {e.File.Name}");
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().Name, ex.Message);
            }

            Nav.NavigateTo("/settings/openbullet", true);
        }
    }
}
