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
using System.Threading.Tasks;

namespace OpenBullet2.Native.ViewModels;

public class ConfigSettingsViewModel : ViewModelBase
{
    private readonly RuriLibSettingsService rlSettingsService;
    private readonly ConfigService configService;
    private Config Config => configService.SelectedConfig
        ?? throw new InvalidOperationException("No config selected");
    private GeneralSettings General => Config.Settings.GeneralSettings;
    private ProxySettings Proxy => Config.Settings.ProxySettings;
    private DataSettings Data => Config.Settings.DataSettings;
    private InputSettings Input => Config.Settings.InputSettings;
    private BrowserSettings Browser => Config.Settings.BrowserSettings;
    private BrowserGhostCursorSettings GhostCursor => Browser.GhostCursor;

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

    private ObservableCollection<string> continueStatuses = [];
    public ObservableCollection<string> ContinueStatuses
    {
        get => continueStatuses;
        set
        {
            continueStatuses = value;
            General.ContinueStatuses = [.. continueStatuses];
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

    private ObservableCollection<string> proxyBanStatuses = [];
    public ObservableCollection<string> ProxyBanStatuses
    {
        get => proxyBanStatuses;
        set
        {
            proxyBanStatuses = value;
            Proxy.BanProxyStatuses = [.. proxyBanStatuses];
            OnPropertyChanged();
        }
    }

    private ObservableCollection<string> allowedProxyTypes = [];
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

    private ObservableCollection<string> allowedWordlistTypes = [];
    public ObservableCollection<string> AllowedWordlistTypes
    {
        get => allowedWordlistTypes;
        set
        {
            allowedWordlistTypes = value;
            Data.AllowedWordlistTypes = [.. allowedWordlistTypes];
            OnPropertyChanged();
            OnPropertyChanged(nameof(DataRuleSliceSuggestions));
        }
    }

    public IEnumerable<string> DataRuleSliceSuggestions => GetDataRuleWordlistTypes()
        .SelectMany(w => w.Slices.Concat(w.SlicesAlias))
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Distinct()
        .OrderBy(s => s);

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

    private ObservableCollection<DataRule> dataRulesCollection = [];
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

    private string testWordlistTypeForRules = string.Empty;
    public string TestWordlistTypeForRules
    {
        get => testWordlistTypeForRules;
        set
        {
            testWordlistTypeForRules = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<ConfigResourceOptions> resourcesCollection = [];
    public ObservableCollection<ConfigResourceOptions> ResourcesCollection
    {
        get => resourcesCollection;
        set
        {
            resourcesCollection = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<CustomInput> customInputsCollection = [];
    public ObservableCollection<CustomInput> CustomInputsCollection
    {
        get => customInputsCollection;
        set
        {
            customInputsCollection = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<string> quitBrowserStatuses = [];
    public ObservableCollection<string> QuitBrowserStatuses
    {
        get => quitBrowserStatuses;
        set
        {
            quitBrowserStatuses = value;
            Browser.QuitBrowserStatuses = [.. quitBrowserStatuses];
            OnPropertyChanged();
        }
    }

    public IEnumerable<BrowserAutomationEngine> BrowserAutomationEngines
        => Enum.GetValues(typeof(BrowserAutomationEngine)).Cast<BrowserAutomationEngine>();

    public BrowserAutomationEngine SelectedBrowserAutomationEngine
    {
        get => Browser.Engine;
        set
        {
            Browser.Engine = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<BrowserMouseAutomationMode> BrowserMouseAutomationModes
        => Enum.GetValues(typeof(BrowserMouseAutomationMode)).Cast<BrowserMouseAutomationMode>();

    public BrowserMouseAutomationMode SelectedBrowserMouseAutomationMode
    {
        get => Browser.MouseAutomationMode;
        set
        {
            Browser.MouseAutomationMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsGhostCursorMouseAutomationSelected));
        }
    }

    public bool IsGhostCursorMouseAutomationSelected
        => SelectedBrowserMouseAutomationMode == BrowserMouseAutomationMode.GhostCursor;

    public bool Headless
    {
        get => Browser.Headless;
        set
        {
            Browser.Headless = value;
            OnPropertyChanged();
        }
    }

    public bool IgnoreHttpsErrors
    {
        get => Browser.IgnoreHttpsErrors;
        set
        {
            Browser.IgnoreHttpsErrors = value;
            OnPropertyChanged();
        }
    }

    public bool LoadOnlyDocumentAndScript
    {
        get => Browser.LoadOnlyDocumentAndScript;
        set
        {
            Browser.LoadOnlyDocumentAndScript = value;
            OnPropertyChanged();
        }
    }

    public bool DismissDialogs
    {
        get => Browser.DismissDialogs;
        set
        {
            Browser.DismissDialogs = value;
            OnPropertyChanged();
        }
    }

    public string CommandLineArgs
    {
        get => Browser.CommandLineArgs;
        set
        {
            Browser.CommandLineArgs = value;
            OnPropertyChanged();
        }
    }

    public List<string> BlockedUrls
    {
        get => Browser.BlockedUrls;
        set
        {
            Browser.BlockedUrls = value;
            OnPropertyChanged();
        }
    }

    public double? GhostCursorMoveSpeed
    {
        get => GhostCursor.MoveSpeed;
        set
        {
            GhostCursor.MoveSpeed = value;
            OnPropertyChanged();
        }
    }

    public int? GhostCursorMoveDelay
    {
        get => GhostCursor.MoveDelay;
        set
        {
            GhostCursor.MoveDelay = value;
            OnPropertyChanged();
        }
    }

    public bool GhostCursorRandomizeMoveDelay
    {
        get => GhostCursor.RandomizeMoveDelay;
        set
        {
            GhostCursor.RandomizeMoveDelay = value;
            if (value)
            {
                GhostCursor.MoveDelay = null;
                OnPropertyChanged(nameof(GhostCursorMoveDelay));
            }
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsGhostCursorMoveDelayEnabled));
        }
    }

    public bool IsGhostCursorMoveDelayEnabled => !GhostCursorRandomizeMoveDelay;

    public int? GhostCursorDelayPerStep
    {
        get => GhostCursor.DelayPerStep;
        set
        {
            GhostCursor.DelayPerStep = value;
            OnPropertyChanged();
        }
    }

    public double? GhostCursorScrollSpeed
    {
        get => GhostCursor.ScrollSpeed;
        set
        {
            GhostCursor.ScrollSpeed = value;
            OnPropertyChanged();
        }
    }

    public int? GhostCursorScrollDelay
    {
        get => GhostCursor.ScrollDelay;
        set
        {
            GhostCursor.ScrollDelay = value;
            OnPropertyChanged();
        }
    }

    public int? GhostCursorHesitate
    {
        get => GhostCursor.Hesitate;
        set
        {
            GhostCursor.Hesitate = value;
            OnPropertyChanged();
        }
    }

    public int? GhostCursorWaitForClick
    {
        get => GhostCursor.WaitForClick;
        set
        {
            GhostCursor.WaitForClick = value;
            OnPropertyChanged();
        }
    }

    public int? GhostCursorMaxTries
    {
        get => GhostCursor.MaxTries;
        set
        {
            GhostCursor.MaxTries = value;
            OnPropertyChanged();
        }
    }

    public double? GhostCursorOvershootThreshold
    {
        get => GhostCursor.OvershootThreshold;
        set
        {
            GhostCursor.OvershootThreshold = value;
            OnPropertyChanged();
        }
    }

    public ConfigSettingsViewModel(ConfigService configService, RuriLibSettingsService rlSettingsService)
    {
        this.configService = configService;
        this.rlSettingsService = rlSettingsService;
        TestWordlistTypeForRules = WordlistTypes.FirstOrDefault() ?? string.Empty;
    }

    public override void UpdateViewModel()
    {
        CreateCollections();
        base.UpdateViewModel();
    }

    public void AddCustomInput()
    {
        CustomInputsCollection.Add(new CustomInput());
        Input.CustomInputs = [.. CustomInputsCollection];
    }

    public void RemoveCustomInput(CustomInput input)
    {
        CustomInputsCollection.Remove(input);
        Input.CustomInputs = [.. CustomInputsCollection];
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

    private void SaveResources() => Data.Resources = [.. ResourcesCollection];
    private void SaveDataRules() => Data.DataRules = [.. DataRulesCollection];

    private IEnumerable<RuriLib.Models.Environment.WordlistType> GetDataRuleWordlistTypes()
    {
        var allowedWordlistTypeNames = AllowedWordlistTypes
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToHashSet(StringComparer.Ordinal);

        var wordlistTypes = rlSettingsService.Environment.WordlistTypes.AsEnumerable();

        return allowedWordlistTypeNames.Count > 0
            ? wordlistTypes.Where(w => allowedWordlistTypeNames.Contains(w.Name))
            : wordlistTypes;
    }

    private void CreateCollections()
    {
        ContinueStatuses = new ObservableCollection<string>(General.ContinueStatuses);
        ProxyBanStatuses = new ObservableCollection<string>(Proxy.BanProxyStatuses);
        AllowedProxyTypes = new ObservableCollection<string>(Proxy.AllowedProxyTypes.Select(t => t.ToString()));
        AllowedWordlistTypes = new ObservableCollection<string>(Data.AllowedWordlistTypes);
        QuitBrowserStatuses = new ObservableCollection<string>(Browser.QuitBrowserStatuses);

        CustomInputsCollection = new ObservableCollection<CustomInput>(Input.CustomInputs);
        ResourcesCollection = new ObservableCollection<ConfigResourceOptions>(Data.Resources);
        DataRulesCollection = new ObservableCollection<DataRule>(Data.DataRules);
    }

    public Task Save() => configService.SaveSelectedConfigAsync();
}
