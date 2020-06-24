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
        [Inject] public RuriLibSettingsService RuriLibSettings { get; set; }
        [Inject] public NavigationManager Nav { get; set; }

        GlobalSettings settings;

        protected override void OnInitialized()
        {
            settings = RuriLibSettings.RuriLibSettings;
        }

        async Task RestoreDefaults()
        {
            if (await js.Confirm("Are you sure", "Do you want to restore the default settings?"))
            {
                RuriLibSettings.RuriLibSettings = new GlobalSettings();
                Nav.NavigateTo("/settings/rurilib");
            }
        }

        async Task Save()
        {
            try
            {
                await RuriLibSettings.Save();
                await js.AlertSuccess("Saved", "The settings were successfully saved.");
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
                await js.AlertSuccess("Success", $"The balance is {balance}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
