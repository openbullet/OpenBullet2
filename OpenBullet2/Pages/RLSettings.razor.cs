using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using RuriLib.Functions.Captchas;
using RuriLib.Models.Settings;
using RuriLib.Services;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class RLSettings
    {
        [Inject] private RuriLibSettingsService RuriLibSettings { get; set; }
        [Inject] private NavigationManager Nav { get; set; }

        private GlobalSettings settings;

        protected override void OnInitialized()
        {
            settings = RuriLibSettings.RuriLibSettings;
        }

        async Task RestoreDefaults()
        {
            if (await js.Confirm(Loc["AreYouSure"], Loc["RestoreDefaultSettingsConfirmation"], Loc["Cancel"]))
            {
                RuriLibSettings.RuriLibSettings = new GlobalSettings();
                Nav.NavigateTo("/settings/rurilib", true);
            }
        }

        async Task Save()
        {
            try
            {
                await RuriLibSettings.Save();
                await js.AlertSuccess(Loc["Saved"], Loc["SettingsSaved"]);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        async Task CheckBalance()
        {
            var service = CaptchaServiceFactory.GetService(settings.CaptchaSettings);
            try
            {
                var balance = await service.GetBalanceAsync();
                await js.AlertSuccess(Loc["Success"], $"{Loc["Balance"]}: {balance}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
