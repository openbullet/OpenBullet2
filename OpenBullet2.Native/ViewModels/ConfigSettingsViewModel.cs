using OpenBullet2.Core.Services;
using RuriLib.Models;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Data.Rules;
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

        public int SuggestedBots
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

        public IEnumerable<StringRule> StringRules => Enum.GetValues(typeof(StringRule)).Cast<StringRule>();

        private ObservableCollection<SimpleDataRule> simpleDataRulesCollection;
        public ObservableCollection<SimpleDataRule> SimpleDataRulesCollection
        {
            get => simpleDataRulesCollection;
            set
            {
                simpleDataRulesCollection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<RegexDataRule> regexDataRulesCollection;
        public ObservableCollection<RegexDataRule> RegexDataRulesCollection
        {
            get => regexDataRulesCollection;
            set
            {
                regexDataRulesCollection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<LinesFromFileResourceOptions> linesFromFileResourcesCollection;
        public ObservableCollection<LinesFromFileResourceOptions> LinesFromFileResourcesCollection
        {
            get => linesFromFileResourcesCollection;
            set
            {
                linesFromFileResourcesCollection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<RandomLinesFromFileResourceOptions> randomLinesFromFileResourcesCollection;
        public ObservableCollection<RandomLinesFromFileResourceOptions> RandomLinesFromFileResourcesCollection
        {
            get => randomLinesFromFileResourcesCollection;
            set
            {
                randomLinesFromFileResourcesCollection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CustomInput> customInputsCollection;
        public ObservableCollection<CustomInput> CustomInputsCollection
        {
            get => customInputsCollection;
            set
            {
                customInputsCollection = value;
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

        public bool IgnoreHttpsErrors
        {
            get => Puppeteer.IgnoreHttpsErrors;
            set
            {
                Puppeteer.IgnoreHttpsErrors = value;
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

        public void AddCustomInput()
        {
            CustomInputsCollection.Add(new CustomInput());
            Input.CustomInputs = CustomInputsCollection.ToList();
        }

        public void RemoveCustomInput(CustomInput input)
        {
            CustomInputsCollection.Remove(input);
            Input.CustomInputs = CustomInputsCollection.ToList();
        }

        public void AddLinesFromFileResource()
        {
            LinesFromFileResourcesCollection.Add(new LinesFromFileResourceOptions());
            SaveResources();
        }

        public void AddRandomLinesFromFileResource()
        {
            RandomLinesFromFileResourcesCollection.Add(new RandomLinesFromFileResourceOptions());
            SaveResources();
        }

        public void RemoveResource(ConfigResourceOptions resource)
        {
            if (resource is LinesFromFileResourceOptions lff)
            {
                LinesFromFileResourcesCollection.Remove(lff);
            }
            else if (resource is RandomLinesFromFileResourceOptions rlff)
            {
                RandomLinesFromFileResourcesCollection.Remove(rlff);
            }

            SaveResources();
        }

        public void AddSimpleDataRule()
        {
            SimpleDataRulesCollection.Add(new SimpleDataRule());
            SaveDataRules();
        }

        public void AddRegexDataRule()
        {
            RegexDataRulesCollection.Add(new RegexDataRule());
            SaveDataRules();
        }

        public void RemoveDataRule(DataRule rule)
        {
            if (rule is SimpleDataRule sdr)
            {
                SimpleDataRulesCollection.Remove(sdr);
            }
            else if (rule is RegexDataRule rdr)
            {
                RegexDataRulesCollection.Remove(rdr);
            }

            SaveDataRules();
        }

        private void SaveResources() => Data.Resources = LinesFromFileResourcesCollection.ToList().Cast<ConfigResourceOptions>()
            .Concat(RandomLinesFromFileResourcesCollection.ToList().Cast<ConfigResourceOptions>()).ToList();

        private void SaveDataRules() => Data.DataRules = SimpleDataRulesCollection.ToList().Cast<DataRule>()
            .Concat(RegexDataRulesCollection.ToList().Cast<DataRule>()).ToList();

        private void CreateCollections()
        {
            ContinueStatuses = string.Join(',', General.ContinueStatuses);
            ProxyBanStatuses = string.Join(',', Proxy.BanProxyStatuses);
            AllowedProxyTypes = string.Join(',', Proxy.AllowedProxyTypes);
            AllowedWordlistTypes = string.Join(',', Data.AllowedWordlistTypes);
            QuitBrowserStatuses = string.Join(',', Puppeteer.QuitBrowserStatuses);

            CustomInputsCollection = new ObservableCollection<CustomInput>(Input.CustomInputs);
            LinesFromFileResourcesCollection = new ObservableCollection<LinesFromFileResourceOptions>(
                Data.Resources.Where(r => r is LinesFromFileResourceOptions)
                .Cast<LinesFromFileResourceOptions>());
            RandomLinesFromFileResourcesCollection = new ObservableCollection<RandomLinesFromFileResourceOptions>(
                Data.Resources.Where(r => r is RandomLinesFromFileResourceOptions)
                .Cast<RandomLinesFromFileResourceOptions>());
            SimpleDataRulesCollection = new ObservableCollection<SimpleDataRule>(
                Data.DataRules.Where(r => r is SimpleDataRule)
                .Cast<SimpleDataRule>());
            RegexDataRulesCollection = new ObservableCollection<RegexDataRule>(
                Data.DataRules.Where(r => r is RegexDataRule)
                .Cast<RegexDataRule>());
        }
    }
}
