using OpenBullet2.Core.Services;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Proxies;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenBullet2.Native.ViewModels
{
    public class ConfigSettingsViewModel : ViewModelBase
    {
        private readonly RuriLibSettingsService rlSettingsService;
        private readonly ConfigService configService;
        private Config Config => configService.SelectedConfig;
        private GeneralSettings General => Config.Settings.GeneralSettings;
        private ProxySettings Proxy => Config.Settings.ProxySettings;
        private DataSettings Data => Config.Settings.DataSettings;
        private InputSettings Input => Config.Settings.InputSettings;
        private PuppeteerSettings Puppeteer => Config.Settings.PuppeteerSettings;

        public int LogBufferSize
        {
            get => General.SuggestedBots;
            set
            {
                General.SuggestedBots = value;
                OnPropertyChanged();
            }
        }

        public int MaximumCPM
        {
            get => General.MaximumCPM;
            set
            {
                General.MaximumCPM = value;
                OnPropertyChanged();
            }
        }

        public bool SaveEmptyCaptures
        {
            get => General.SaveEmptyCaptures;
            set
            {
                General.SaveEmptyCaptures = value;
                OnPropertyChanged();
            }
        }

        private string continueStatuses;
        public string ContinueStatuses
        {
            get => continueStatuses;
            set
            {
                continueStatuses = value;
                General.ContinueStatuses = continueStatuses.Split(',', StringSplitOptions.RemoveEmptyEntries);
                OnPropertyChanged();
            }
        }

        public bool UseProxies
        {
            get => Proxy.UseProxies;
            set
            {
                Proxy.UseProxies = value;
                OnPropertyChanged();
            }
        }

        public int MaxUsesPerProxy
        {
            get => Proxy.MaxUsesPerProxy;
            set
            {
                Proxy.MaxUsesPerProxy = value;
                OnPropertyChanged();
            }
        }

        public int BanLoopEvasion
        {
            get => Proxy.BanLoopEvasion;
            set
            {
                Proxy.BanLoopEvasion = value;
                OnPropertyChanged();
            }
        }

        private string proxyBanStatuses;
        public string ProxyBanStatuses
        {
            get => proxyBanStatuses;
            set
            {
                proxyBanStatuses = value;
                Proxy.BanProxyStatuses = proxyBanStatuses.Split(',', StringSplitOptions.RemoveEmptyEntries);
                OnPropertyChanged();
            }
        }

        private string allowedProxyTypes;
        public string AllowedProxyTypes
        {
            get => allowedProxyTypes;
            set
            {
                allowedProxyTypes = value;
                Proxy.AllowedProxyTypes = allowedProxyTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(t => Enum.TryParse(typeof(ProxyType), t, true, out var _))
                    .Select(t => (ProxyType)Enum.Parse(typeof(ProxyType), t, true)).ToArray();
                OnPropertyChanged();
            }
        }

        private string allowedWordlistTypes;
        public string AllowedWordlistTypes
        {
            get => allowedWordlistTypes;
            set
            {
                allowedWordlistTypes = value;
                Data.AllowedWordlistTypes = allowedWordlistTypes.Split(',', StringSplitOptions.RemoveEmptyEntries);
                OnPropertyChanged();
            }
        }

        public bool UrlEncodeDataAfterSlicing
        {
            get => Data.UrlEncodeDataAfterSlicing;
            set
            {
                Data.UrlEncodeDataAfterSlicing = value;
                OnPropertyChanged();
            }
        }

        private string quitBrowserStatuses;
        public string QuitBrowserStatuses
        {
            get => quitBrowserStatuses;
            set
            {
                quitBrowserStatuses = value;
                Puppeteer.QuitBrowserStatuses = quitBrowserStatuses.Split(',', StringSplitOptions.RemoveEmptyEntries);
                OnPropertyChanged();
            }
        }

        public bool Headless
        {
            get => Puppeteer.Headless;
            set
            {
                Puppeteer.Headless = value;
                OnPropertyChanged();
            }
        }

        public bool LoadOnlyDocumentAndScript
        {
            get => Puppeteer.LoadOnlyDocumentAndScript;
            set
            {
                Puppeteer.LoadOnlyDocumentAndScript = value;
                OnPropertyChanged();
            }
        }

        public bool DismissDialogs
        {
            get => Puppeteer.DismissDialogs;
            set
            {
                Puppeteer.DismissDialogs = value;
                OnPropertyChanged();
            }
        }

        public string CommandLineArgs
        {
            get => Puppeteer.CommandLineArgs;
            set
            {
                Puppeteer.CommandLineArgs = value;
                OnPropertyChanged();
            }
        }

        public List<string> BlockedUrls
        {
            get => Puppeteer.BlockedUrls;
            set
            {
                Puppeteer.BlockedUrls = value;
                OnPropertyChanged();
            }
        }

        public ConfigSettingsViewModel()
        {
            configService = SP.GetService<ConfigService>();
            rlSettingsService = SP.GetService<RuriLibSettingsService>();
        }

        public override void UpdateViewModel()
        {
            CreateCollections();
            base.UpdateViewModel();
        }

        private void CreateCollections()
        {
            ContinueStatuses = string.Join(',', General.ContinueStatuses);
            ProxyBanStatuses = string.Join(',', Proxy.BanProxyStatuses);
            AllowedProxyTypes = string.Join(',', Proxy.AllowedProxyTypes);
            AllowedWordlistTypes = string.Join(',', Data.AllowedWordlistTypes);
            QuitBrowserStatuses = string.Join(',', Puppeteer.QuitBrowserStatuses);
        }
    }
}
