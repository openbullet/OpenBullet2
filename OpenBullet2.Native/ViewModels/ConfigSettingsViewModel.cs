using OpenBullet2.Core.Services;
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
        private BrowserSettings Puppeteer => Config.Settings.BrowserSettings;

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

        public bool ReportLastCaptchaOnRetry
        {
            get => General.ReportLastCaptchaOnRetry;
            set
            {
                General.ReportLastCaptchaOnRetry = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> AllStatuses => rlSettingsService.GetStatuses();
        public IEnumerable<string> ProxyTypes => Enum.GetNames(typeof(ProxyType));
        public IEnumerable<string> WordlistTypes => rlSettingsService.Environment.WordlistTypes.Select(w => w.Name);

        private ObservableCollection<string> continueStatuses;
        public ObservableCollection<string> ContinueStatuses
        {
            get => continueStatuses;
            set
            {
                continueStatuses = value;
                General.ContinueStatuses = continueStatuses.ToArray();
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

        private ObservableCollection<string> proxyBanStatuses;
        public ObservableCollection<string> ProxyBanStatuses
        {
            get => proxyBanStatuses;
            set
            {
                proxyBanStatuses = value;
                Proxy.BanProxyStatuses = proxyBanStatuses.ToArray();
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> allowedProxyTypes;
        public ObservableCollection<string> AllowedProxyTypes
        {
            get => allowedProxyTypes;
            set
            {
                allowedProxyTypes = value;
                Proxy.AllowedProxyTypes = allowedProxyTypes
                    .Select(t => (ProxyType)Enum.Parse(typeof(ProxyType), t, true)).ToArray();
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> allowedWordlistTypes;
        public ObservableCollection<string> AllowedWordlistTypes
        {
            get => allowedWordlistTypes;
            set
            {
                allowedWordlistTypes = value;
                Data.AllowedWordlistTypes = allowedWordlistTypes.ToArray();
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

        private ObservableCollection<DataRule> dataRulesCollection;
        public ObservableCollection<DataRule> DataRulesCollection
        {
            get => dataRulesCollection;
            set
            {
                dataRulesCollection = value;
                OnPropertyChanged();
            }
        }

        private string testDataForRules = string.Empty;
        public string TestDataForRules
        {
            get => testDataForRules;
            set
            {
                testDataForRules = value;
                OnPropertyChanged();
            }
        }

        private string testWordlistTypeForRules;
        public string TestWordlistTypeForRules
        {
            get => testWordlistTypeForRules;
            set
            {
                testWordlistTypeForRules = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ConfigResourceOptions> resourcesCollection;
        public ObservableCollection<ConfigResourceOptions> ResourcesCollection
        {
            get => resourcesCollection;
            set
            {
                resourcesCollection = value;
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

        private ObservableCollection<string> quitBrowserStatuses;
        public ObservableCollection<string> QuitBrowserStatuses
        {
            get => quitBrowserStatuses;
            set
            {
                quitBrowserStatuses = value;
                Puppeteer.QuitBrowserStatuses = quitBrowserStatuses.ToArray();
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
            TestWordlistTypeForRules = WordlistTypes.First();
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
            ResourcesCollection.Add(new LinesFromFileResourceOptions());
            SaveResources();
        }

        public void AddRandomLinesFromFileResource()
        {
            ResourcesCollection.Add(new RandomLinesFromFileResourceOptions());
            SaveResources();
        }

        public void RemoveResource(ConfigResourceOptions resource)
        {
            ResourcesCollection.Remove(resource);
            SaveResources();
        }

        public void AddSimpleDataRule()
        {
            DataRulesCollection.Add(new SimpleDataRule());
            SaveDataRules();
        }

        public void AddRegexDataRule()
        {
            DataRulesCollection.Add(new RegexDataRule());
            SaveDataRules();
        }

        public void RemoveDataRule(DataRule rule)
        {
            DataRulesCollection.Remove(rule);
            SaveDataRules();
        }

        private void SaveResources() => Data.Resources = ResourcesCollection.ToList();
        private void SaveDataRules() => Data.DataRules = DataRulesCollection.ToList();

        private void CreateCollections()
        {
            ContinueStatuses = new ObservableCollection<string>(General.ContinueStatuses);
            ProxyBanStatuses = new ObservableCollection<string>(Proxy.BanProxyStatuses);
            AllowedProxyTypes = new ObservableCollection<string>(Proxy.AllowedProxyTypes.Select(t => t.ToString()));
            AllowedWordlistTypes = new ObservableCollection<string>(Data.AllowedWordlistTypes);
            QuitBrowserStatuses = new ObservableCollection<string>(Puppeteer.QuitBrowserStatuses);

            CustomInputsCollection = new ObservableCollection<CustomInput>(Input.CustomInputs);
            ResourcesCollection = new ObservableCollection<ConfigResourceOptions>(Data.Resources);
            DataRulesCollection = new ObservableCollection<DataRule>(Data.DataRules);
        }
    }
}
