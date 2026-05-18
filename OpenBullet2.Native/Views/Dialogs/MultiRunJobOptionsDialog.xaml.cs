using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.Utils;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Configs;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Models.Proxies;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OpenBullet2.Native.Views.Dialogs;

/// <summary>
/// Interaction logic for MultiRunJobOptionsDialog.xaml
/// </summary>
public partial class MultiRunJobOptionsDialog : Page
{
    private readonly IUiFactory uiFactory;
    private readonly OpenBulletSettingsService obSettingsService;
    private readonly Func<JobOptions, Task>? onAccept;
    private readonly MultiRunJobOptionsViewModel vm;

    public MultiRunJobOptionsDialog(
        IUiFactory uiFactory,
        OpenBulletSettingsService obSettingsService,
        MultiRunJobOptionsViewModel vm,
        Func<JobOptions, Task>? onAccept)
        : this(uiFactory, obSettingsService, vm, null, onAccept)
    {
    }

    public MultiRunJobOptionsDialog(
        IUiFactory uiFactory,
        OpenBulletSettingsService obSettingsService,
        MultiRunJobOptionsViewModel vm,
        MultiRunJobOptions? options,
        Func<JobOptions, Task>? onAccept)
    {
        this.uiFactory = uiFactory;
        this.obSettingsService = obSettingsService;
        this.onAccept = onAccept;
        this.vm = vm;
        vm.Initialize(options);
        DataContext = this.vm;

        this.vm.StartConditionModeChanged += mode => startConditionTabControl.SelectedIndex = GetStartConditionTabIndex(mode);

        InitializeComponent();

        startConditionTabControl.SelectedIndex = GetStartConditionTabIndex(this.vm.StartConditionMode);
    }

    private static int GetStartConditionTabIndex(StartConditionMode mode) => mode switch
    {
        StartConditionMode.Immediate => 0,
        StartConditionMode.Relative => 1,
        StartConditionMode.Absolute => 2,
        _ => throw new NotImplementedException()
    };

    private void AddGroupProxySource(object sender, RoutedEventArgs e) => vm.AddGroupProxySource();
    private void AddFileProxySource(object sender, RoutedEventArgs e) => vm.AddFileProxySource();
    private void AddRemoteProxySource(object sender, RoutedEventArgs e) => vm.AddRemoteProxySource();

    private void RemoveProxySource(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ProxySourceOptionsViewModel proxySource })
        {
            vm.RemoveProxySource(proxySource);
        }
    }

    private void AddDatabaseHitOutput(object sender, RoutedEventArgs e) => vm.AddDatabaseHitOutput();
    private void AddFileSystemHitOutput(object sender, RoutedEventArgs e) => vm.AddFileSystemHitOutput();
    private void AddDiscordWebhookHitOutput(object sender, RoutedEventArgs e) => vm.AddDiscordWebhookHitOutput();
    private void AddTelegramBotHitOutput(object sender, RoutedEventArgs e) => vm.AddTelegramBotHitOutput();
    private void AddCustomWebhookHitOutput(object sender, RoutedEventArgs e) => vm.AddCustomWebhookHitOutput();

    private void RemoveHitOutput(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: HitOutputOptions hitOutput })
        {
            vm.RemoveHitOutput(hitOutput);
        }
    }

    public async Task SelectConfigAsync(ConfigViewModel config)
    {
        vm.SelectConfig(config);
        await vm.TrySetRecordAsync();
    }

    public async Task SelectWordlistAsync(WordlistEntity entity)
    {
        if (vm.DataPoolOptions is WordlistDataPoolOptionsViewModel wordlistOptions)
        {
            wordlistOptions.SelectWordlist(entity);
        }

        await vm.TrySetRecordAsync();
    }

    private void AddWordlist(object sender, RoutedEventArgs e)
        => new MainDialog(uiFactory.Create<AddWordlistDialog>(this), "Add a wordlist").ShowDialog();

    public Task AddWordlistAsync(WordlistEntity entity) => vm.AddWordlist(entity);

    private void SelectConfig(object sender, RoutedEventArgs e)
        => new MainDialog(uiFactory.Create<SelectConfigDialog>(this), "Select a config").ShowDialog();

    private void SelectWordlist(object sender, RoutedEventArgs e)
        => new MainDialog(uiFactory.Create<SelectWordlistDialog>(this), "Select a wordlist").ShowDialog();

    private async void Accept(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!vm.IsConfigSelected)
            {
                Alert.Error("No config selected", "Please select a config before proceeding");
                return;
            }

            if (obSettingsService.Settings.GeneralSettings.WarnDangerousConfig
                && vm.SelectedConfig is not null
                && vm.SelectedConfig.HasCSharpCode())
            {
                Alert.Warning("Potentially dangerous config", "The Config you selected might have some C# code in it" +
                    " (or blocks that call external programs). Although C# can be helpful for config makers who want to" +
                    " use functionalities that are not implemented through blocks, it can also be used to harm your computer" +
                    " or steal information. It's STRONGLY advised that you review the code of the config and make sure nothing" +
                    " fishy is going on. Please review the config and make sure it is completely safe to run!");
            }

            if (onAccept is not null)
            {
                await onAccept(vm.Options);
            }

            ((MainDialog)Parent).Close();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private void SelectFileForProxySource(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Filter = "Proxy files or Shell scripts echoing proxies one by one | *.txt;*.bat;*.ps1;*.sh",
            FilterIndex = 1
        };

        ofd.ShowDialog();

        if (sender is Button { Tag: FileProxySourceOptionsViewModel fileProxySource })
        {
            fileProxySource.FileName = ofd.FileName;
        }
    }

    private void SelectFileForDataPool(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Filter = "Wordlist files | *.txt",
            FilterIndex = 1
        };

        ofd.ShowDialog();

        if (vm.DataPoolOptions is FileDataPoolOptionsViewModel fileDataPoolOptions)
        {
            fileDataPoolOptions.FileName = ofd.FileName;
        }
    }
}

public class MultiRunJobOptionsViewModel : ViewModelBase
{
    private readonly RuriLibSettingsService rlSettingsService;
    private readonly ConfigService configService;
    private readonly JobFactoryService jobFactory;
    private readonly IServiceScopeFactory scopeFactory;
    private StartConditionMode startConditionMode;
    public MultiRunJobOptions Options { get; private set; }

    #region Start Condition
    public event Action<StartConditionMode>? StartConditionModeChanged;

    public StartConditionMode StartConditionMode
    {
        get => startConditionMode;
        set
        {
            startConditionMode = value;
            Options.StartCondition = value switch
            {
                StartConditionMode.Immediate => new RelativeTimeStartCondition
                {
                    StartAfter = TimeSpan.Zero
                },
                StartConditionMode.Relative => new RelativeTimeStartCondition(),
                StartConditionMode.Absolute => new AbsoluteTimeStartCondition(),
                _ => throw new NotImplementedException()
            };

            OnPropertyChanged();
            OnPropertyChanged(nameof(StartImmediatelyMode));
            OnPropertyChanged(nameof(StartInMode));
            OnPropertyChanged(nameof(StartAtMode));
            StartConditionModeChanged?.Invoke(value);
        }
    }

    public bool StartImmediatelyMode
    {
        get => StartConditionMode is StartConditionMode.Immediate;
        set
        {
            if (value)
            {
                StartConditionMode = StartConditionMode.Immediate;
            }

            OnPropertyChanged();
        }
    }

    public bool StartInMode
    {
        get => StartConditionMode is StartConditionMode.Relative;
        set
        {
            if (value)
            {
                StartConditionMode = StartConditionMode.Relative;
            }

            OnPropertyChanged();
        }
    }

    public bool StartAtMode
    {
        get => StartConditionMode is StartConditionMode.Absolute;
        set
        {
            if (value)
            {
                StartConditionMode = StartConditionMode.Absolute;
            }

            OnPropertyChanged();
        }
    }

    public DateTime StartAtTime
    {
        get => Options.StartCondition is AbsoluteTimeStartCondition abs ? abs.StartAt : DateTime.Now;
        set
        {
            if (Options.StartCondition is AbsoluteTimeStartCondition abs)
            {
                abs.StartAt = value;
            }

            OnPropertyChanged();
        }
    }

    public TimeSpan StartIn
    {
        get => Options.StartCondition is RelativeTimeStartCondition rel ? rel.StartAfter : TimeSpan.Zero;
        set
        {
            if (Options.StartCondition is RelativeTimeStartCondition rel)
            {
                rel.StartAfter = value;
            }

            OnPropertyChanged();
        }
    }
    #endregion

    #region Config and Proxy options
    private BitmapImage? configIcon;
    public BitmapImage? ConfigIcon
    {
        get => configIcon;
        private set
        {
            configIcon = value;
            OnPropertyChanged();
        }
    }

    private string configNameAndAuthor = string.Empty;
    public string ConfigNameAndAuthor
    {
        get => configNameAndAuthor;
        set
        {
            configNameAndAuthor = value;
            OnPropertyChanged();
        }
    }

    public bool IsConfigSelected => SelectedConfig is not null;
    public Config? SelectedConfig { get; private set; }

    public void SelectConfig(ConfigViewModel vm)
    {
        Options.ConfigId = vm.Config.Id;
        Bots = vm.Config.Settings.GeneralSettings.SuggestedBots;
        SetConfigData();
    }

    private void SetConfigData()
    {
        SelectedConfig = configService.Configs.FirstOrDefault(c => c.Id == Options.ConfigId);
        ConfigIcon = null;
        ConfigNameAndAuthor = string.Empty;
        OnPropertyChanged(nameof(IsConfigSelected));

        if (SelectedConfig is not null)
        {
            ConfigIcon = Images.Base64ToBitmapImage(SelectedConfig.Metadata.Base64Image);
            ConfigNameAndAuthor = $"{SelectedConfig.Metadata.Name} by {SelectedConfig.Metadata.Author}";
        }
    }

    public async Task TrySetRecordAsync()
    {
        if (Options.DataPool is WordlistDataPoolOptions wdpo)
        {
            var record = await WithRecordRepositoryAsync(repo => repo.GetAll()
                .FirstOrDefaultAsync(r => r.ConfigId == Options.ConfigId && r.WordlistId == wdpo.WordlistId));

            Skip = record?.Checkpoint ?? 0;
        }
    }

    public int Bots
    {
        get => Options.Bots;
        set
        {
            Options.Bots = value;
            OnPropertyChanged();
        }
    }

    public int BotLimit => jobFactory.BotLimit;

    public int Skip
    {
        get => Options.Skip;
        set
        {
            Options.Skip = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<JobProxyMode> ProxyModes => Enum.GetValues(typeof(JobProxyMode)).Cast<JobProxyMode>();

    public JobProxyMode ProxyMode
    {
        get => Options.ProxyMode;
        set
        {
            Options.ProxyMode = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<NoValidProxyBehaviour> NoValidProxyBehaviours => Enum.GetValues(typeof(NoValidProxyBehaviour)).Cast<NoValidProxyBehaviour>();

    public NoValidProxyBehaviour NoValidProxyBehaviour
    {
        get => Options.NoValidProxyBehaviour;
        set
        {
            Options.NoValidProxyBehaviour = value;
            OnPropertyChanged();
        }
    }

    public bool ShuffleProxies
    {
        get => Options.ShuffleProxies;
        set
        {
            Options.ShuffleProxies = value;
            OnPropertyChanged();
        }
    }

    public bool MarkAsToCheckOnAbort
    {
        get => Options.MarkAsToCheckOnAbort;
        set
        {
            Options.MarkAsToCheckOnAbort = value;
            OnPropertyChanged();
        }
    }

    public bool NeverBanProxies
    {
        get => Options.NeverBanProxies;
        set
        {
            Options.NeverBanProxies = value;
            OnPropertyChanged();
        }
    }

    public bool NeverMarkProxiesAsBad
    {
        get => Options.NeverMarkProxiesAsBad;
        set
        {
            Options.NeverMarkProxiesAsBad = value;
            OnPropertyChanged();
        }
    }

    public bool ConcurrentProxyMode
    {
        get => Options.ConcurrentProxyMode;
        set
        {
            Options.ConcurrentProxyMode = value;
            OnPropertyChanged();
        }
    }

    public int PeriodicReloadIntervalSeconds
    {
        get => Options.PeriodicReloadIntervalSeconds;
        set
        {
            Options.PeriodicReloadIntervalSeconds = value;
            OnPropertyChanged();
        }
    }

    public int ProxyBanTimeSeconds
    {
        get => Options.ProxyBanTimeSeconds;
        set
        {
            Options.ProxyBanTimeSeconds = value;
            OnPropertyChanged();
        }
    }
    #endregion

    public MultiRunJobOptionsViewModel(
        RuriLibSettingsService rlSettingsService,
        ConfigService configService,
        JobFactoryService jobFactory,
        IServiceScopeFactory scopeFactory)
    {
        this.rlSettingsService = rlSettingsService;
        this.configService = configService;
        this.jobFactory = jobFactory;
        this.scopeFactory = scopeFactory;
        Options = null!;
    }

    public void Initialize(MultiRunJobOptions? options)
    {
        Options = options ?? (JobOptionsFactory.CreateNew(JobType.MultiRun) as MultiRunJobOptions
            ?? throw new InvalidOperationException("Failed to create multi run job options"));
        startConditionMode = Options.StartCondition switch
        {
            RelativeTimeStartCondition rel when rel.StartAfter == TimeSpan.Zero => StartConditionMode.Immediate,
            RelativeTimeStartCondition => StartConditionMode.Relative,
            AbsoluteTimeStartCondition => StartConditionMode.Absolute,
            _ => throw new NotImplementedException()
        };

        SetConfigData();

        DataPoolOptions = Options.DataPool switch
        {
            WordlistDataPoolOptions w => new WordlistDataPoolOptionsViewModel(w, scopeFactory),
            FileDataPoolOptions f => new FileDataPoolOptionsViewModel(f),
            RangeDataPoolOptions r => new RangeDataPoolOptionsViewModel(r),
            CombinationsDataPoolOptions c => new CombinationsDataPoolOptionsViewModel(c),
            InfiniteDataPoolOptions i => new InfiniteDataPoolOptionsViewModel(i),
            _ => throw new NotImplementedException()
        };

        using (var scope = scopeFactory.CreateScope())
        {
            proxyGroups = scope.ServiceProvider.GetRequiredService<IProxyGroupRepository>().GetAll().ToList();
        }
        PopulateProxySources();

        HitOutputsCollection = new ObservableCollection<HitOutputOptions>(Options.HitOutputs);
    }

    #region Hit Outputs
    private ObservableCollection<HitOutputOptions> hitOutputsCollection = [];
    public ObservableCollection<HitOutputOptions> HitOutputsCollection
    {
        get => hitOutputsCollection;
        set
        {
            hitOutputsCollection = value;
            OnPropertyChanged();
        }
    }

    public void AddDatabaseHitOutput()
    {
        if (!Options.HitOutputs.Any(o => o is DatabaseHitOutputOptions))
        {
            AddHitOutput(new DatabaseHitOutputOptions());
        }
    }

    public void AddFileSystemHitOutput() => AddHitOutput(new FileSystemHitOutputOptions());
    public void AddDiscordWebhookHitOutput() => AddHitOutput(new DiscordWebhookHitOutputOptions());
    public void AddTelegramBotHitOutput() => AddHitOutput(new TelegramBotHitOutputOptions());
    public void AddCustomWebhookHitOutput() => AddHitOutput(new CustomWebhookHitOutputOptions());

    private void AddHitOutput(HitOutputOptions options)
    {
        Options.HitOutputs.Add(options);
        HitOutputsCollection.Add(options);
    }

    public void RemoveHitOutput(HitOutputOptions hitOutput)
    {
        HitOutputsCollection.Remove(hitOutput);
        Options.HitOutputs.Remove(hitOutput);
    }
    #endregion

    #region Proxy Sources
    private IEnumerable<ProxyGroupEntity> proxyGroups = [];
    public IEnumerable<string> ProxyGroupNames => new[] { "All" }.Concat(proxyGroups.Select(g => g.Name ?? string.Empty));
    public IEnumerable<ProxyType> ProxyTypes => Enum.GetValues(typeof(ProxyType)).Cast<ProxyType>();

    private ObservableCollection<ProxySourceOptionsViewModel> proxySourcesCollection = [];
    public ObservableCollection<ProxySourceOptionsViewModel> ProxySourcesCollection
    {
        get => proxySourcesCollection;
        set
        {
            proxySourcesCollection = value;
            OnPropertyChanged();
        }
    }

    public void AddGroupProxySource()
    {
        var options = new GroupProxySourceOptions();
        Options.ProxySources.Add(options);
        var vm = new GroupProxySourceOptionsViewModel(options, proxyGroups);
        ProxySourcesCollection.Add(vm);
    }

    public void AddFileProxySource()
    {
        var options = new FileProxySourceOptions();
        Options.ProxySources.Add(options);
        var vm = new FileProxySourceOptionsViewModel(options);
        ProxySourcesCollection.Add(vm);
    }

    public void AddRemoteProxySource()
    {
        var options = new RemoteProxySourceOptions();
        Options.ProxySources.Add(options);
        var vm = new RemoteProxySourceOptionsViewModel(options);
        ProxySourcesCollection.Add(vm);
    }

    public void RemoveProxySource(ProxySourceOptionsViewModel vm)
    {
        ProxySourcesCollection.Remove(vm);
        Options.ProxySources.Remove(vm.Options);
    }

    private void PopulateProxySources()
    {
        ProxySourcesCollection = [];

        foreach (var source in Options.ProxySources)
        {
            switch (source)
            {
                case GroupProxySourceOptions group:
                    ProxySourcesCollection.Add(new GroupProxySourceOptionsViewModel(group, proxyGroups));
                    break;

                case FileProxySourceOptions file:
                    ProxySourcesCollection.Add(new FileProxySourceOptionsViewModel(file));
                    break;

                case RemoteProxySourceOptions remote:
                    ProxySourcesCollection.Add(new RemoteProxySourceOptionsViewModel(remote));
                    break;
            }
        }
    }
    #endregion

    #region Data Pool
    private DataPoolOptionsViewModel dataPoolOptions = new FileDataPoolOptionsViewModel(new FileDataPoolOptions());
    public DataPoolOptionsViewModel DataPoolOptions
    {
        get => dataPoolOptions;
        set
        {
            dataPoolOptions = value;
            Options.DataPool = dataPoolOptions.Options;
            OnPropertyChanged();
        }
    }

    public bool WordlistDataPoolMode
    {
        get => DataPoolOptions is WordlistDataPoolOptionsViewModel;
        set
        {
            if (value)
            {
                DataPoolOptions = new WordlistDataPoolOptionsViewModel(new WordlistDataPoolOptions(), scopeFactory);
            }

            OnPropertyChanged();
        }
    }

    public bool FileDataPoolMode
    {
        get => DataPoolOptions is FileDataPoolOptionsViewModel;
        set
        {
            if (value)
            {
                DataPoolOptions = new FileDataPoolOptionsViewModel(new FileDataPoolOptions());
            }

            OnPropertyChanged();
        }
    }

    public bool RangeDataPoolMode
    {
        get => DataPoolOptions is RangeDataPoolOptionsViewModel;
        set
        {
            if (value)
            {
                DataPoolOptions = new RangeDataPoolOptionsViewModel(new RangeDataPoolOptions());
            }

            OnPropertyChanged();
        }
    }

    public bool CombinationsDataPoolMode
    {
        get => DataPoolOptions is CombinationsDataPoolOptionsViewModel;
        set
        {
            if (value)
            {
                DataPoolOptions = new CombinationsDataPoolOptionsViewModel(new CombinationsDataPoolOptions());
            }

            OnPropertyChanged();
        }
    }

    public bool InfiniteDataPoolMode
    {
        get => DataPoolOptions is InfiniteDataPoolOptionsViewModel;
        set
        {
            if (value)
            {
                DataPoolOptions = new InfiniteDataPoolOptionsViewModel(new InfiniteDataPoolOptions());
            }

            OnPropertyChanged();
        }
    }

    public IEnumerable<string> WordlistTypes => rlSettingsService.Environment.WordlistTypes.Select(t => t.Name);
    #endregion

    public Task AddWordlist(WordlistEntity entity) => WithWordlistRepositoryAsync(repo => repo.AddAsync(entity));

    private async Task WithWordlistRepositoryAsync(Func<IWordlistRepository, Task> action)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IWordlistRepository>();
        await action(repo);
    }

    private async Task<T> WithRecordRepositoryAsync<T>(Func<IRecordRepository, Task<T>> action)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRecordRepository>();
        return await action(repo);
    }
}

public enum StartConditionMode
{
    Relative,
    Absolute,
    Immediate
}

#region Data Pool ViewModels
public class DataPoolOptionsViewModel(DataPoolOptions options) : ViewModelBase
{
    public DataPoolOptions Options { get; init; } = options;
}

public class WordlistDataPoolOptionsViewModel : DataPoolOptionsViewModel
{
    private WordlistEntity? wordlist;
    private readonly IServiceScopeFactory scopeFactory;
    private WordlistDataPoolOptions WordlistOptions => Options as WordlistDataPoolOptions
        ?? throw new InvalidOperationException("Invalid wordlist data pool options");

    public string Info => WordlistOptions.WordlistId == -1 || wordlist is null
        ? "No wordlist selected"
        : $"{wordlist.Name} ({wordlist.Total} lines)";

    public WordlistDataPoolOptionsViewModel(WordlistDataPoolOptions options, IServiceScopeFactory scopeFactory) : base(options)
    {
        this.scopeFactory = scopeFactory;

        if (options.WordlistId != -1)
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IWordlistRepository>();
            wordlist = repo.GetAsync(options.WordlistId).GetAwaiter().GetResult();
        }

        // If the wordlist was not found (e.g. deleted)
        if (wordlist is null)
        {
            options.WordlistId = -1;
        }
    }

    public void SelectWordlist(WordlistEntity wordlist)
    {
        this.wordlist = wordlist;
        WordlistOptions.WordlistId = wordlist.Id;
        OnPropertyChanged(nameof(Info));
    }
}

public class FileDataPoolOptionsViewModel(FileDataPoolOptions options) : DataPoolOptionsViewModel(options)
{
    private FileDataPoolOptions FileOptions => Options as FileDataPoolOptions
        ?? throw new InvalidOperationException("Invalid file data pool options");

    public string FileName
    {
        get => FileOptions.FileName ?? string.Empty;
        set
        {
            FileOptions.FileName = value;
            OnPropertyChanged();
        }
    }

    public string WordlistType
    {
        get => FileOptions.WordlistType ?? string.Empty;
        set
        {
            FileOptions.WordlistType = value;
            OnPropertyChanged();
        }
    }
}

public class RangeDataPoolOptionsViewModel(RangeDataPoolOptions options) : DataPoolOptionsViewModel(options)
{
    private RangeDataPoolOptions RangeOptions => (RangeDataPoolOptions)Options;

    public long Start
    {
        get => RangeOptions.Start;
        set
        {
            RangeOptions.Start = value;
            OnPropertyChanged();
        }
    }

    public int Amount
    {
        get => RangeOptions.Amount;
        set
        {
            RangeOptions.Amount = value;
            OnPropertyChanged();
        }
    }

    public int Step
    {
        get => RangeOptions.Step;
        set
        {
            RangeOptions.Step = value;
            OnPropertyChanged();
        }
    }

    public bool Pad
    {
        get => RangeOptions.Pad;
        set
        {
            RangeOptions.Pad = value;
            OnPropertyChanged();
        }
    }

    public string WordlistType
    {
        get => RangeOptions.WordlistType ?? string.Empty;
        set
        {
            RangeOptions.WordlistType = value;
            OnPropertyChanged();
        }
    }
}

public class CombinationsDataPoolOptionsViewModel(CombinationsDataPoolOptions options) : DataPoolOptionsViewModel(options)
{
    private CombinationsDataPoolOptions CombinationsOptions => (CombinationsDataPoolOptions)Options;

    public string CharSet
    {
        get => CombinationsOptions.CharSet ?? string.Empty;
        set
        {
            CombinationsOptions.CharSet = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GeneratedAmountText));
        }
    }

    public int Length
    {
        get => CombinationsOptions.Length;
        set
        {
            CombinationsOptions.Length = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GeneratedAmountText));
        }
    }

    public string WordlistType
    {
        get => CombinationsOptions.WordlistType ?? string.Empty;
        set
        {
            CombinationsOptions.WordlistType = value;
            OnPropertyChanged();
        }
    }

    public string GeneratedAmountText => $"{(long)Math.Pow(CharSet.Length, Length)} combinations will be generated";
}

public class InfiniteDataPoolOptionsViewModel(InfiniteDataPoolOptions options) : DataPoolOptionsViewModel(options)
{
    private InfiniteDataPoolOptions InfiniteOptions => (InfiniteDataPoolOptions)Options;

    public string WordlistType
    {
        get => InfiniteOptions.WordlistType ?? string.Empty;
        set
        {
            InfiniteOptions.WordlistType = value;
            OnPropertyChanged();
        }
    }
}
#endregion

#region Proxy Sources ViewModels
public class ProxySourceOptionsViewModel(ProxySourceOptions options) : ViewModelBase
{
    public ProxySourceOptions Options { get; init; } = options;
}

public class GroupProxySourceOptionsViewModel(GroupProxySourceOptions options,
    IEnumerable<ProxyGroupEntity> proxyGroups) : ProxySourceOptionsViewModel(options)
{
    private GroupProxySourceOptions GroupOptions => (GroupProxySourceOptions)Options;

    private readonly IEnumerable<ProxyGroupEntity> proxyGroups = proxyGroups;

    public string GroupName
    {
        get => GroupOptions.GroupId == -1
            ? "All"
            : proxyGroups.First(g => g.Id == GroupOptions.GroupId).Name ?? string.Empty;
        set
        {
            GroupOptions.GroupId = value == "All" ? -1 : proxyGroups.First(g => g.Name == value).Id;
            OnPropertyChanged();
        }
    }
}

public class FileProxySourceOptionsViewModel(FileProxySourceOptions options) : ProxySourceOptionsViewModel(options)
{
    private FileProxySourceOptions FileOptions => (FileProxySourceOptions)Options;

    public string FileName
    {
        get => FileOptions.FileName ?? string.Empty;
        set
        {
            FileOptions.FileName = value;
            OnPropertyChanged();
        }
    }

    public ProxyType DefaultType
    {
        get => FileOptions.DefaultType;
        set
        {
            FileOptions.DefaultType = value;
            OnPropertyChanged();
        }
    }
}

public class RemoteProxySourceOptionsViewModel(RemoteProxySourceOptions options) : ProxySourceOptionsViewModel(options)
{
    private RemoteProxySourceOptions RemoteOptions => (RemoteProxySourceOptions)Options;

    public string Url
    {
        get => RemoteOptions.Url ?? string.Empty;
        set
        {
            RemoteOptions.Url = value;
            OnPropertyChanged();
        }
    }

    public ProxyType DefaultType
    {
        get => RemoteOptions.DefaultType;
        set
        {
            RemoteOptions.DefaultType = value;
            OnPropertyChanged();
        }
    }
}
#endregion
