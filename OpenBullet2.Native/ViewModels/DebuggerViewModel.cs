using OpenBullet2.Core.Services;
using RuriLib.Logging;
using RuriLib.Models.Debugger;
using RuriLib.Models.Proxies;
using RuriLib.Models.Variables;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Native.ViewModels
{
    public class DebuggerViewModel : ViewModelBase
    {
        private readonly RuriLibSettingsService rlSettingsService;
        private readonly OpenBulletSettingsService obSettingsService;
        private readonly ConfigService configService;
        private readonly IRandomUAProvider randomUAProvider;
        private readonly IRNGProvider rngProvider;
        private readonly PluginRepository pluginRepo;

        private DebuggerOptions options;
        private BotLogger logger;
        private ConfigDebugger debugger;

        public event EventHandler<BotLoggerEntry> NewLogEntry;
        public event EventHandler LogCleared;

        private string testData = string.Empty;
        public string TestData
        {
            get => testData;
            set
            {
                testData = value;
                OnPropertyChanged();
            }
        }

        private string wordlistType;
        public string WordlistType
        {
            get => wordlistType;
            set
            {
                wordlistType = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> WordlistTypes => rlSettingsService.Environment.WordlistTypes.Select(w => w.Name);

        private bool persistLog;
        public bool PersistLog
        {
            get => persistLog;
            set
            {
                persistLog = value;
                OnPropertyChanged();
            }
        }

        private bool useProxy;
        public bool UseProxy
        {
            get => useProxy;
            set
            {
                useProxy = value;
                OnPropertyChanged();
            }
        }

        private string testProxy = string.Empty;
        public string TestProxy
        {
            get => testProxy;
            set
            {
                testProxy = value;
                OnPropertyChanged();
            }
        }

        private ProxyType proxyType = ProxyType.Http;
        public ProxyType ProxyType
        {
            get => proxyType;
            set
            {
                proxyType = value;
                OnPropertyChanged();
            }
        }

        private bool stepByStep;
        public bool StepByStep
        {
            get => stepByStep;
            set
            {
                stepByStep = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<ProxyType> ProxyTypes => Enum.GetValues(typeof(ProxyType)).Cast<ProxyType>();

        private ConfigDebuggerStatus status;
        public ConfigDebuggerStatus Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanTakeStep));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public bool CanStart => status is ConfigDebuggerStatus.Idle;
        public bool CanTakeStep => status is ConfigDebuggerStatus.WaitingForStep;
        public bool CanStop => status is ConfigDebuggerStatus.Running or ConfigDebuggerStatus.WaitingForStep;

        public List<Variable> Variables => obSettingsService.Settings.GeneralSettings.GroupCapturesInDebugger
            ? options.Variables.OrderBy(v => v.MarkedForCapture).ToList()
            : options.Variables;

        private string searchString = string.Empty;
        public string SearchString
        {
            get => searchString;
            set
            {
                searchString = value;
                OnPropertyChanged();
            }
        }

        private int[] indices = Array.Empty<int>();
        public int[] Indices
        {
            get => indices;
            set
            {
                indices = value;
                CurrentMatchIndex = 0;
            }
        }

        private int currentMatchIndex;
        public int CurrentMatchIndex
        {
            get => currentMatchIndex;
            set
            {
                currentMatchIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MatchInfo));
            }
        }

        public string MatchInfo => $"{CurrentMatchIndex + 1} of {Indices.Length}";

        public DebuggerViewModel()
        {
            rlSettingsService = SP.GetService<RuriLibSettingsService>();
            obSettingsService = SP.GetService<OpenBulletSettingsService>();
            configService = SP.GetService<ConfigService>();
            randomUAProvider = SP.GetService<IRandomUAProvider>();
            rngProvider = SP.GetService<IRNGProvider>();
            pluginRepo = SP.GetService<PluginRepository>();

            WordlistType = WordlistTypes.First();
        }

        public async Task Run()
        {
            if (!PersistLog)
            {
                logger = new();
            }

            options = new DebuggerOptions
            {
                TestData = TestData,
                TestProxy = TestProxy,
                WordlistType = WordlistType,
                PersistLog = PersistLog,
                ProxyType = ProxyType,
                UseProxy = UseProxy,
                StepByStep = StepByStep
            };

            debugger = new ConfigDebugger(configService.SelectedConfig, options, logger)
            {
                PluginRepo = pluginRepo,
                RandomUAProvider = randomUAProvider,
                RNGProvider = rngProvider,
                RuriLibSettings = rlSettingsService
            };

            debugger.StatusChanged += OnStatusChanged;
            debugger.NewLogEntry += OnNewLogEntry;

            try
            {
                await debugger.Run();
            }
            finally
            {
                Status = ConfigDebuggerStatus.Idle;

                debugger.StatusChanged -= OnStatusChanged;
                debugger.NewLogEntry -= OnNewLogEntry;
            }
        }

        public void TakeStep() => debugger?.TryTakeStep();

        public void Stop() => debugger?.Stop();

        public void ClearLog()
        {
            logger?.Clear();
            LogCleared?.Invoke(this, EventArgs.Empty);
        }

        private void OnStatusChanged(object sender, ConfigDebuggerStatus status) => Status = status;
        private void OnNewLogEntry(object sender, BotLoggerEntry e) => NewLogEntry?.Invoke(this, e);
    }
}
