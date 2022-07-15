using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
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

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for MultiRunJobOptionsDialog.xaml
    /// </summary>
    public partial class MultiRunJobOptionsDialog : Page
    {
        private readonly Action<JobOptions> onAccept;
        private readonly MultiRunJobOptionsViewModel vm;

        public MultiRunJobOptionsDialog(MultiRunJobOptions options = null, Action<JobOptions> onAccept = null)
        {
            this.onAccept = onAccept;
            vm = new MultiRunJobOptionsViewModel(options);
            DataContext = vm;

            vm.StartConditionModeChanged += mode => startConditionTabControl.SelectedIndex = (int)mode;
            
            InitializeComponent();

            startConditionTabControl.SelectedIndex = (int)vm.StartConditionMode;
        }

        private void AddGroupProxySource(object sender, RoutedEventArgs e) => vm.AddGroupProxySource();
        private void AddFileProxySource(object sender, RoutedEventArgs e) => vm.AddFileProxySource();
        private void AddRemoteProxySource(object sender, RoutedEventArgs e) => vm.AddRemoteProxySource();

        private void RemoveProxySource(object sender, RoutedEventArgs e)
            => vm.RemoveProxySource((ProxySourceOptionsViewModel)(sender as Button).Tag);

        private void AddDatabaseHitOutput(object sender, RoutedEventArgs e) => vm.AddDatabaseHitOutput();
        private void AddFileSystemHitOutput(object sender, RoutedEventArgs e) => vm.AddFileSystemHitOutput();
        private void AddDiscordWebhookHitOutput(object sender, RoutedEventArgs e) => vm.AddDiscordWebhookHitOutput();
        private void AddTelegramBotHitOutput(object sender, RoutedEventArgs e) => vm.AddTelegramBotHitOutput();
        private void AddCustomWebhookHitOutput(object sender, RoutedEventArgs e) => vm.AddCustomWebhookHitOutput();

        private void RemoveHitOutput(object sender, RoutedEventArgs e)
            => vm.RemoveHitOutput((HitOutputOptions)(sender as Button).Tag);

        public async void SelectConfig(ConfigViewModel config)
        {
            vm.SelectConfig(config);
            await vm.TrySetRecord();
        }

        public async void SelectWordlist(WordlistEntity entity)
        {
            (vm.DataPoolOptions as WordlistDataPoolOptionsViewModel).SelectWordlist(entity);
            await vm.TrySetRecord();
        }

        private void AddWordlist(object sender, RoutedEventArgs e)
            => new MainDialog(new AddWordlistDialog(this), "Add a wordlist").ShowDialog();

        public async void AddWordlist(WordlistEntity entity) => await vm.AddWordlist(entity);

        private void SelectConfig(object sender, RoutedEventArgs e)
            => new MainDialog(new SelectConfigDialog(this), "Select a config").ShowDialog();

        private void SelectWordlist(object sender, RoutedEventArgs e)
            => new MainDialog(new SelectWordlistDialog(this), "Select a wordlist").ShowDialog();

        private void Accept(object sender, RoutedEventArgs e)
        {
            if (!vm.IsConfigSelected)
            {
                Alert.Error("No config selected", "Please select a config before proceeding");
                return;
            }

            if (vm.SelectedConfig.HasCSharpCode())
            {
                Alert.Warning("Potentially dangerous config", "The Config you selected might have some C# code in it" +
                    " (or blocks that call external programs). Although C# can be helpful for config makers who want to" +
                    " use functionalities that are not implemented through blocks, it can also be used to harm your computer" +
                    " or steal information. It's STRONGLY advised that you review the code of the config and make sure nothing" +
                    " fishy is going on. Please review the config and make sure it is completely safe to run!");
            }

            onAccept?.Invoke(vm.Options);
            ((MainDialog)Parent).Close();
        }

        private void SelectFileForProxySource(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Proxy files or Shell scripts echoing proxies one by one | *.txt;*.bat;*.ps1;*.sh",
                FilterIndex = 1
            };

            ofd.ShowDialog();
            ((FileProxySourceOptionsViewModel)(sender as Button).Tag).FileName = ofd.FileName;
        }

        private void SelectFileForDataPool(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Wordlist files | *.txt",
                FilterIndex = 1
            };

            ofd.ShowDialog();
            (vm.DataPoolOptions as FileDataPoolOptionsViewModel).FileName = ofd.FileName;
        }
    }

    public class MultiRunJobOptionsViewModel : ViewModelBase
    {
        private readonly IRecordRepository recordRepo;
        private readonly IWordlistRepository wordlistRepo;
        private readonly RuriLibSettingsService rlSettingsService;
        private readonly ConfigService configService;
        private readonly JobFactoryService jobFactory;
        private readonly IProxyGroupRepository proxyGroupRepo;
        public MultiRunJobOptions Options { get; init; }

        #region Start Condition
        public event Action<StartConditionMode> StartConditionModeChanged;

        public StartConditionMode StartConditionMode
        {
            get => Options.StartCondition switch
            {
                RelativeTimeStartCondition => StartConditionMode.Relative,
                AbsoluteTimeStartCondition => StartConditionMode.Absolute,
                _ => throw new NotImplementedException()
            };
            set
            {
                Options.StartCondition = value switch
                {
                    StartConditionMode.Relative => new RelativeTimeStartCondition(),
                    StartConditionMode.Absolute => new AbsoluteTimeStartCondition(),
                    _ => throw new NotImplementedException()
                };

                OnPropertyChanged();
                OnPropertyChanged(nameof(StartInMode));
                OnPropertyChanged(nameof(StartAtMode));
                StartConditionModeChanged?.Invoke(StartConditionMode);
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
        private BitmapImage configIcon;
        public BitmapImage ConfigIcon
        {
            get => configIcon;
            private set
            {
                configIcon = value;
                OnPropertyChanged();
            }
        }

        private string configNameAndAuthor;
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
        public Config SelectedConfig { get; private set; }

        public void SelectConfig(ConfigViewModel vm)
        {
            Options.ConfigId = vm.Config.Id;
            Bots = vm.Config.Settings.GeneralSettings.SuggestedBots;
            SetConfigData();
        }

        private void SetConfigData()
        {
            SelectedConfig = configService.Configs.FirstOrDefault(c => c.Id == Options.ConfigId);

            if (SelectedConfig is not null)
            {
                ConfigIcon = Images.Base64ToBitmapImage(SelectedConfig.Metadata.Base64Image);
                ConfigNameAndAuthor = $"{SelectedConfig.Metadata.Name} by {SelectedConfig.Metadata.Author}";
                OnPropertyChanged(nameof(IsConfigSelected));
            }
        }

        public async Task TrySetRecord()
        {
            if (Options.DataPool is WordlistDataPoolOptions wdpo)
            {
                var record = await recordRepo.GetAll()
                    .FirstOrDefaultAsync(r => r.ConfigId == Options.ConfigId && r.WordlistId == wdpo.WordlistId);

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

        public MultiRunJobOptionsViewModel(MultiRunJobOptions options)
        {
            Options = options ?? JobOptionsFactory.CreateNew(JobType.MultiRun) as MultiRunJobOptions;
            recordRepo = SP.GetService<IRecordRepository>();
            wordlistRepo = SP.GetService<IWordlistRepository>();
            rlSettingsService = SP.GetService<RuriLibSettingsService>();
            configService = SP.GetService<ConfigService>();
            jobFactory = SP.GetService<JobFactoryService>();
            proxyGroupRepo = SP.GetService<IProxyGroupRepository>();

            SetConfigData();

            DataPoolOptions = Options.DataPool switch
            {
                WordlistDataPoolOptions w => new WordlistDataPoolOptionsViewModel(w),
                FileDataPoolOptions f => new FileDataPoolOptionsViewModel(f),
                RangeDataPoolOptions r => new RangeDataPoolOptionsViewModel(r),
                CombinationsDataPoolOptions c => new CombinationsDataPoolOptionsViewModel(c),
                InfiniteDataPoolOptions i => new InfiniteDataPoolOptionsViewModel(i),
                _ => throw new NotImplementedException()
            };

            proxyGroups = proxyGroupRepo.GetAll().ToList();
            PopulateProxySources();

            HitOutputsCollection = new ObservableCollection<HitOutputOptions>(Options.HitOutputs);
        }

        #region Hit Outputs
        private ObservableCollection<HitOutputOptions> hitOutputsCollection;
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
        private readonly IEnumerable<ProxyGroupEntity> proxyGroups;
        public IEnumerable<string> ProxyGroupNames => new string[] { "All" }.Concat(proxyGroups.Select(g => g.Name));
        public IEnumerable<ProxyType> ProxyTypes => Enum.GetValues(typeof(ProxyType)).Cast<ProxyType>();

        private ObservableCollection<ProxySourceOptionsViewModel> proxySourcesCollection;
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
            ProxySourcesCollection = new ObservableCollection<ProxySourceOptionsViewModel>();

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
        private DataPoolOptionsViewModel dataPoolOptions;
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
                    DataPoolOptions = new WordlistDataPoolOptionsViewModel(new WordlistDataPoolOptions());
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

        public Task AddWordlist(WordlistEntity entity) => wordlistRepo.Add(entity);
    }

    public enum StartConditionMode
    {
        Relative,
        Absolute
    }

    #region Data Pool ViewModels
    public class DataPoolOptionsViewModel : ViewModelBase
    {
        public DataPoolOptions Options { get; init; }

        public DataPoolOptionsViewModel(DataPoolOptions options)
        {
            Options = options;
        }
    }

    public class WordlistDataPoolOptionsViewModel : DataPoolOptionsViewModel
    {
        private readonly IWordlistRepository wordlistRepo;
        private WordlistEntity wordlist;
        private WordlistDataPoolOptions WordlistOptions => Options as WordlistDataPoolOptions;

        public string Info => WordlistOptions.WordlistId == -1 ? "No wordlist selected" : $"{wordlist.Name} ({wordlist.Total} lines)";

        public WordlistDataPoolOptionsViewModel(WordlistDataPoolOptions options) : base(options)
        {
            wordlistRepo = SP.GetService<IWordlistRepository>();

            if (options.WordlistId != -1)
            {
                wordlist = wordlistRepo.Get(options.WordlistId).Result;
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

    public class FileDataPoolOptionsViewModel : DataPoolOptionsViewModel
    {
        private FileDataPoolOptions FileOptions => Options as FileDataPoolOptions;

        public string FileName
        {
            get => FileOptions.FileName;
            set
            {
                FileOptions.FileName = value;
                OnPropertyChanged();
            }
        }

        public string WordlistType
        {
            get => FileOptions.WordlistType;
            set
            {
                FileOptions.WordlistType = value;
                OnPropertyChanged();
            }
        }

        public FileDataPoolOptionsViewModel(FileDataPoolOptions options) : base(options)
        {

        }
    }

    public class RangeDataPoolOptionsViewModel : DataPoolOptionsViewModel
    {
        private RangeDataPoolOptions RangeOptions => Options as RangeDataPoolOptions;

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
            get => RangeOptions.WordlistType;
            set
            {
                RangeOptions.WordlistType = value;
                OnPropertyChanged();
            }
        }

        public RangeDataPoolOptionsViewModel(RangeDataPoolOptions options) : base(options)
        {

        }
    }

    public class CombinationsDataPoolOptionsViewModel : DataPoolOptionsViewModel
    {
        private CombinationsDataPoolOptions CombinationsOptions => Options as CombinationsDataPoolOptions;

        public string CharSet
        {
            get => CombinationsOptions.CharSet;
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
            get => CombinationsOptions.WordlistType;
            set
            {
                CombinationsOptions.WordlistType = value;
                OnPropertyChanged();
            }
        }

        public string GeneratedAmountText => $"{(long)Math.Pow(CharSet.Length, Length)} combinations will be generated";

        public CombinationsDataPoolOptionsViewModel(CombinationsDataPoolOptions options) : base(options)
        {

        }
    }

    public class InfiniteDataPoolOptionsViewModel : DataPoolOptionsViewModel
    {
        private InfiniteDataPoolOptions InfiniteOptions => Options as InfiniteDataPoolOptions;

        public string WordlistType
        {
            get => InfiniteOptions.WordlistType;
            set
            {
                InfiniteOptions.WordlistType = value;
                OnPropertyChanged();
            }
        }

        public InfiniteDataPoolOptionsViewModel(InfiniteDataPoolOptions options) : base(options)
        {

        }
    }
    #endregion

    #region Proxy Sources ViewModels
    public class ProxySourceOptionsViewModel : ViewModelBase
    {
        public ProxySourceOptions Options { get; init; }

        public ProxySourceOptionsViewModel(ProxySourceOptions options)
        {
            Options = options;
        }
    }

    public class GroupProxySourceOptionsViewModel : ProxySourceOptionsViewModel
    {
        private GroupProxySourceOptions GroupOptions => Options as GroupProxySourceOptions;

        private readonly IEnumerable<ProxyGroupEntity> proxyGroups;

        public string GroupName
        {
            get => GroupOptions.GroupId == -1 ? "All" : proxyGroups.First(g => g.Id == GroupOptions.GroupId).Name;
            set
            {
                GroupOptions.GroupId = value == "All" ? -1 : proxyGroups.First(g => g.Name == value).Id;
                OnPropertyChanged();
            }
        }

        public GroupProxySourceOptionsViewModel(GroupProxySourceOptions options,
            IEnumerable<ProxyGroupEntity> proxyGroups) : base(options)
        {
            this.proxyGroups = proxyGroups;
        }
    }

    public class FileProxySourceOptionsViewModel : ProxySourceOptionsViewModel
    {
        private FileProxySourceOptions FileOptions => Options as FileProxySourceOptions;

        public string FileName
        {
            get => FileOptions.FileName;
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

        public FileProxySourceOptionsViewModel(FileProxySourceOptions options) : base(options)
        {

        }
    }

    public class RemoteProxySourceOptionsViewModel : ProxySourceOptionsViewModel
    {
        private RemoteProxySourceOptions RemoteOptions => Options as RemoteProxySourceOptions;

        public string Url
        {
            get => RemoteOptions.Url;
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

        public RemoteProxySourceOptionsViewModel(RemoteProxySourceOptions options) : base(options)
        {

        }
    }
    #endregion
}
