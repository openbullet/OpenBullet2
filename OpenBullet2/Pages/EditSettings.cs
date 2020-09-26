using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Services;
using RuriLib.Models.Configs;
using RuriLib.Models.Proxies;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class EditSettings
    {
        [Inject] public ConfigService ConfigService { get; set; }
        [Inject] public RuriLibSettingsService RuriLibSettings { get; set; }
        [Inject] public NavigationManager Nav { get; set; }

        ConfigSettings settings;
        List<string> continueStatuses;
        List<string> stopStatuses;
        List<string> proxyContinueStatuses;
        List<string> proxyBanStatuses;
        List<ProxyType> allowedProxyTypes;
        List<ProxyType> unallowedProxyTypes;
        List<string> allowedWordlistTypes;
        List<string> unallowedWordlistTypes;
        List<string> quitBrowserStatuses;
        List<string> dontQuitBrowserStatuses;

        protected override void OnInitialized()
        {
            settings = ConfigService.SelectedConfig.Settings;

            // HACK: I tried binding directly to lists but it would spit out double the amount of values when deserializing the json
            // for some reason so this approach is working but it's not the best
            continueStatuses = settings.GeneralSettings.ContinueStatuses.ToList();
            stopStatuses = RuriLibSettings.GetStatuses()
                .Where(s => !continueStatuses.Contains(s)).ToList();

            proxyBanStatuses = settings.ProxySettings.BanProxyStatuses.ToList();
            proxyContinueStatuses = RuriLibSettings.GetStatuses()
                .Where(s => !proxyBanStatuses.Contains(s)).ToList();

            allowedProxyTypes = settings.ProxySettings.AllowedProxyTypes.ToList();
            unallowedProxyTypes = Enum.GetValues(typeof(ProxyType))
                .Cast<ProxyType>().Where(t => !allowedProxyTypes.Contains(t)).ToList();

            allowedWordlistTypes = settings.DataSettings.AllowedWordlistTypes.ToList();
            unallowedWordlistTypes = RuriLibSettings.Environment.WordlistTypes
                .Select(w => w.Name)
                .Where(w => !allowedWordlistTypes.Contains(w)).ToList();

            quitBrowserStatuses = settings.PuppeteerSettings.QuitBrowserStatuses.ToList();
            dontQuitBrowserStatuses = RuriLibSettings.GetStatuses()
                .Where(s => !quitBrowserStatuses.Contains(s)).ToList();
        }

        void OnSelectionChanged(object value)
        {
            // HACK: Set all here since I don't want to make 1 method for each one
            settings.GeneralSettings.ContinueStatuses = continueStatuses.ToArray();
            settings.ProxySettings.BanProxyStatuses = proxyBanStatuses.ToArray();
            settings.ProxySettings.AllowedProxyTypes = allowedProxyTypes.ToArray();
            settings.DataSettings.AllowedWordlistTypes = allowedWordlistTypes.ToArray();
            settings.PuppeteerSettings.QuitBrowserStatuses = quitBrowserStatuses.ToArray();
        }

        async Task RestoreDefaults()
        {
            if (await js.Confirm(Loc["AreYouSure"], Loc["RestoreDefaultSettingsConfirmation"], Loc["Cancel"]))
            {
                ConfigService.SelectedConfig.Settings = new ConfigSettings();
                Nav.NavigateTo("/config/edit/settings");
            }
        }
    }
}
