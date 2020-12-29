using Blazored.Modal.Services;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Components;
using Microsoft.Scripting.Utils;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Settings;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using System;
using System.Collections.Generic;
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
                Nav.NavigateTo("/settings/openbullet");
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
    }
}
