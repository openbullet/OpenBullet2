using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Settings;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class OBSettings
    {
        [Inject] public PersistentSettingsService PersistentSettings { get; set; }
        [Inject] public NavigationManager Nav { get; set; }
        [Inject] public IModalService Modal { get; set; }

        OpenBulletSettings settings;

        protected override void OnInitialized()
        {
            settings = PersistentSettings.OpenBulletSettings;
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
