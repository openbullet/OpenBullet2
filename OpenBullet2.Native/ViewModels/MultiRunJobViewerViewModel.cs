using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Proxies.Sources;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Utils;
using RuriLib.Extensions;
using RuriLib.Models.Bots;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Hits;
using RuriLib.Models.Hits.HitOutputs;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Models.Proxies.ProxySources;
using RuriLib.Parallelization.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenBullet2.Native.ViewModels
{
    public class MultiRunJobViewerViewModel : ViewModelBase, IDisposable
    {
        private readonly OpenBulletSettingsService obSettingsService;
        private readonly List<ProxyGroupEntity> proxyGroups;
        private readonly Timer botsInfoTimer;
        private readonly Timer secondsTicker;
        private readonly SoundPlayer soundPlayer;
        private CancellationTokenSource startCTS;

        public event Action<object, string, Color> NewMessage;

        public MultiRunJobViewModel Job { get; set; }
        private MultiRunJob MultiRunJob => Job.Job as MultiRunJob;

        #region Properties that don't need to be updated during the run
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

        public string DataPoolInfo => MultiRunJob.DataPool switch
        {
            WordlistDataPool w => $"Wordlist ({w.Wordlist.Name})",
            FileDataPool f => $"File ({f.FileName})",
            InfiniteDataPool => "Infinite",
            RangeDataPool r => $"Range (start: {r.Start}, amount: {r.Amount}, step: {r.Step}, pad: {r.Pad})",
            CombinationsDataPool c => $"Combinations (charset: {c.CharSet}, length: {c.Length})",
            _ => throw new NotImplementedException()
        };

        private string proxySourcesInfo;
        public string ProxySourcesInfo
        {
            get => proxySourcesInfo;
            set
            {
                proxySourcesInfo = value;
                OnPropertyChanged();
            }
        }

        private string hitOutputsInfo;
        public string HitOutputsInfo
        {
            get => hitOutputsInfo;
            set
            {
                hitOutputsInfo = value;
                OnPropertyChanged();
            }
        }

        public string CustomInputsInfo => string.Join(", ", MultiRunJob.CustomInputsAnswers.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        public bool HasCustomInputs => MultiRunJob.Config != null && MultiRunJob.Config.Settings.InputSettings.CustomInputs.Any();

        public bool EnableJobLog => obSettingsService.Settings.GeneralSettings.EnableJobLogging;
        #endregion

        #region Properties that need to be updated every second
        public string RemainingWaitString => MultiRunJob.StartCondition switch
        {
            RelativeTimeStartCondition r => (MultiRunJob.StartTime + r.StartAfter - DateTime.Now).ToString(@"hh\:mm\:ss"),
            AbsoluteTimeStartCondition a => (a.StartAt - DateTime.Now).ToString(@"hh\:mm\:ss"),
            _ => throw new NotImplementedException()
        };

        public bool IsWaiting => MultiRunJob.Status is JobStatus.Waiting;
        #endregion

        #region Properties that need to be updated when the status changes
        public bool CanChangeOptions => MultiRunJob.Status is JobStatus.Idle;
        public bool CanStart => MultiRunJob.Status is JobStatus.Idle;
        public bool CanSkipWait => MultiRunJob.Status is JobStatus.Waiting;
        public bool CanPause => MultiRunJob.Status is JobStatus.Running;
        public bool CanResume => MultiRunJob.Status is JobStatus.Paused;
        public bool CanStop => MultiRunJob.Status is JobStatus.Running or JobStatus.Paused;
        public bool CanAbort => MultiRunJob.Status is JobStatus.Starting or JobStatus.Running or JobStatus.Paused or JobStatus.Pausing or JobStatus.Stopping;

        public bool IsStarting => MultiRunJob.Status is JobStatus.Starting;
        public bool IsStopping => MultiRunJob.Status is JobStatus.Stopping;
        public bool IsPausing => MultiRunJob.Status is JobStatus.Pausing;
        #endregion

        #region Properties that need to be updated when a new result comes in
        public double Progress => Math.Clamp(MultiRunJob.Progress * 100, 0, 100);
        #endregion

        #region Collections
        private ObservableCollection<BotViewModel> botsCollection;
        public ObservableCollection<BotViewModel> BotsCollection
        {
            get => botsCollection;
            set
            {
                botsCollection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<HitViewModel> hitsCollection;
        public ObservableCollection<HitViewModel> HitsCollection
        {
            get => hitsCollection;
            set
            {
                hitsCollection = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<HitsFilter> HitsFilters => Enum.GetValues(typeof(HitsFilter)).Cast<HitsFilter>();

        private HitsFilter hitsFilter = HitsFilter.Hits;
        public HitsFilter HitsFilter
        {
            get => hitsFilter;
            set
            {
                hitsFilter = value;
                OnPropertyChanged();
                UpdateHitsCollection();
            }
        }
        #endregion

        public MultiRunJobViewerViewModel(MultiRunJobViewModel jobVM)
        {
            obSettingsService = SP.GetService<OpenBulletSettingsService>();
            Job = jobVM;

            #region Setup
            if (MultiRunJob.Config is not null)
            {
                ConfigIcon = Images.Base64ToBitmapImage(MultiRunJob.Config.Metadata.Base64Image);
                ConfigNameAndAuthor = $"{MultiRunJob.Config.Metadata.Name} by {MultiRunJob.Config.Metadata.Author}";
            }

            var proxyGroupRepo = SP.GetService<IProxyGroupRepository>();
            proxyGroups = proxyGroupRepo.GetAll().ToList();

            var sb = new StringBuilder();
            for (var i = 0; i < MultiRunJob.ProxySources.Count; i++)
            {
                var info = MultiRunJob.ProxySources[i] switch
                {
                    GroupProxySource g => $"Group ({GetProxyGroupName(g.GroupId)})",
                    FileProxySource f => $"File ({f.FileName})",
                    RemoteProxySource r => $"Remote ({r.Url})",
                    _ => throw new NotImplementedException()
                };

                sb.Append(info);

                if (i < MultiRunJob.ProxySources.Count - 1)
                {
                    sb.Append(" | ");
                }
            }

            ProxySourcesInfo = sb.ToString();

            sb = new StringBuilder();
            for (var i = 0; i < MultiRunJob.HitOutputs.Count; i++)
            {
                var info = MultiRunJob.HitOutputs[i] switch
                {
                    DatabaseHitOutput => "Database",
                    FileSystemHitOutput fs => $"File System ({fs.BaseDir})",
                    DiscordWebhookHitOutput d => $"Discord ({d.Webhook.TruncatePretty(70)})",
                    TelegramBotHitOutput t => $"Telegram ({t.Token.Split(':')[0]})",
                    CustomWebhookHitOutput c => $"Custom Webhook ({c.Url.TruncatePretty(70)})",
                    _ => throw new NotImplementedException()
                };

                sb.Append(info);

                if (i < MultiRunJob.HitOutputs.Count - 1)
                {
                    sb.Append(" | ");
                }
            }
            
            HitOutputsInfo = sb.ToString();
            #endregion

            #region Bind events and timers
            MultiRunJob.OnCompleted += UpdateOnCompleted;
            MultiRunJob.OnResult += UpdateViewModel;
            MultiRunJob.OnStatusChanged += UpdateStatus;
            MultiRunJob.OnProgress += UpdateViewModel;

            MultiRunJob.OnResult += OnResult;
            MultiRunJob.OnResult += PlayHitSound;
            MultiRunJob.OnTaskError += OnTaskError;
            MultiRunJob.OnError += OnError;
            MultiRunJob.OnHit += OnHit;

            botsInfoTimer = new Timer(new TimerCallback(_ => RefreshBotsInfo()), null, 200, 200);
            secondsTicker = new Timer(new TimerCallback(_ => PeriodicUpdate()), null, 1000, 1000);
            soundPlayer = new SoundPlayer("Sounds/hit.wav");
            #endregion

            UpdateBots();
            UpdateHitsCollection();
        }

        #region Update methods
        // Updates the VM of all the current BotViewModel instances
        private void RefreshBotsInfo()
        {
            if (BotsCollection is not null)
            {
                foreach (var bot in BotsCollection)
                {
                    bot.UpdateViewModel();
                }
            }
        }

        // Periodic update for stuff that needs to be updated every second
        private void PeriodicUpdate()
        {
            OnPropertyChanged(nameof(IsWaiting));

            if (MultiRunJob.Status == JobStatus.Waiting)
            {
                OnPropertyChanged(nameof(RemainingWaitString));
            }

            Job.PeriodicUpdate();

            // Update the bots collection if the number of bots was changed
            if (BotsCollection is not null && BotsCollection.Count != MultiRunJob.Bots)
            {
                UpdateBots();
            }
        }

        // Updates everything (only when a job completes, just to be safe, not expensive)
        private void UpdateOnCompleted(object sender, EventArgs e) => UpdateViewModel();

        // Updates the stats after every successful check
        private void UpdateViewModel(object sender, ResultDetails<MultiRunInput, CheckResult> details)
        {
            OnPropertyChanged(nameof(Progress));
            Job.UpdateStats();
        }

        // Update the stuff related to a job's status change
        private void UpdateStatus(object sender, JobStatus status)
        {
            Job.UpdateStatus();

            OnPropertyChanged(nameof(CanChangeOptions));
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanSkipWait));
            OnPropertyChanged(nameof(CanResume));
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(CanAbort));

            OnPropertyChanged(nameof(IsStarting));
            OnPropertyChanged(nameof(IsStopping));
            OnPropertyChanged(nameof(IsPausing));
        }


        private void UpdateViewModel(object sender, float progress) => UpdateViewModel();

        private void OnHit(object sender, Hit hit)
        {
            if ((HitsFilter == HitsFilter.Hits && hit.Type == "SUCCESS") || (HitsFilter == HitsFilter.ToCheck && hit.Type == "NONE")
                || (HitsFilter == HitsFilter.Custom && hit.Type != "SUCCESS" && hit.Type != "NONE"))
            {
                Application.Current.Dispatcher.Invoke(() => HitsCollection?.Add(new HitViewModel(hit)));
            }
        }

        // Call this at the start and when bots are changed
        private void UpdateBots()
        {
            var bots = Enumerable.Range(0, MultiRunJob.Bots)
                .Select(i => new BotViewModel(i, MultiRunJob.CurrentBotDatas));

            BotsCollection = new ObservableCollection<BotViewModel>(bots);
        }

        private void UpdateHitsCollection()
        {
            var hits = HitsFilter switch
            {
                HitsFilter.Hits => MultiRunJob.Hits.Where(h => h.Type == "SUCCESS"),
                HitsFilter.ToCheck => MultiRunJob.Hits.Where(h => h.Type == "NONE"),
                HitsFilter.Custom => MultiRunJob.Hits.Where(h => h.Type != "SUCCESS" && h.Type != "NONE"),
                _ => throw new NotImplementedException()
            };

            HitsCollection = new ObservableCollection<HitViewModel>(hits.Select(h => new HitViewModel(h)));
        }
        #endregion

        #region Logging
        private void OnResult(object sender, ResultDetails<MultiRunInput, CheckResult> details)
        {
            var botData = details.Result.BotData;
            var data = botData.Line.Data;
            var proxy = botData.Proxy != null
                ? $"{botData.Proxy.Host}:{botData.Proxy.Port}"
                : string.Empty;

            var message = $"Line checked ({data})({proxy}) with status {botData.STATUS}";
            var color = botData.STATUS switch
            {
                "SUCCESS" => Colors.YellowGreen,
                "FAIL" => Colors.Tomato,
                "BAN" => Colors.Plum,
                "RETRY" => Colors.Yellow,
                "ERROR" => Colors.Red,
                "NONE" => Colors.SkyBlue,
                _ => Colors.Orange
            };

            NewMessage?.Invoke(this, message, color);
        }

        private void OnTaskError(object sender, ErrorDetails<MultiRunInput> details)
        {
            var botData = details.Item.BotData;
            var data = botData.Line.Data;
            var proxy = botData.Proxy != null
                ? $"{botData.Proxy.Host}:{botData.Proxy.Port}"
                : string.Empty;

            var message = $"Task error ({data})({proxy})! {details.Exception.Message}";
            NewMessage?.Invoke(this, message, Colors.Tomato);
        }

        private void OnError(object sender, Exception ex)
            => NewMessage?.Invoke(this, $"Job error: {ex.Message}", Colors.Tomato);
        #endregion

        private void PlayHitSound(object sender, ResultDetails<MultiRunInput, CheckResult> details)
        {
            if (obSettingsService.Settings.CustomizationSettings.PlaySoundOnHit && details.Result.BotData.STATUS == "SUCCESS")
            {
                try
                {
                    soundPlayer.Play();
                }
                catch
                {
                    
                }
            }
        }

        #region Controls
        public async Task Start()
        {
            try
            {
                startCTS = new CancellationTokenSource();
                HitsCollection = new();
                AskCustomInputs();
                OnPropertyChanged(nameof(CustomInputsInfo));
                await MultiRunJob.Start(startCTS.Token);
                UpdateBots();
            }
            finally
            {
                startCTS?.Dispose();
            }
        }

        public Task Stop() => MultiRunJob.Stop();

        public async Task Abort()
        {
            if (MultiRunJob.Status is JobStatus.Starting or JobStatus.Waiting)
            {
                startCTS.Cancel();
                return;
            }

            await MultiRunJob.Abort();
        }

        public Task Pause() => MultiRunJob.Pause();
        public Task Resume() => MultiRunJob.Resume();
        public void SkipWait() => MultiRunJob.SkipWait();
        
        public async Task ChangeBots(int newValue)
        {
            // TODO: Also edit the job options! So the number of bots is persisted

            await MultiRunJob.ChangeBots(newValue);
            MultiRunJob.Bots = newValue;
            Job.UpdateBots();
        }
        #endregion

        #region Utils
        private void AskCustomInputs()
        {
            MultiRunJob.CustomInputsAnswers.Clear();

            foreach (var input in MultiRunJob.Config.Settings.InputSettings.CustomInputs)
            {
                MultiRunJob.CustomInputsAnswers[input.VariableName] = Alert.CustomInput(input.Description, input.DefaultAnswer);
            }
        }

        private string GetProxyGroupName(int id)
        {
            try
            {
                if (id == -1)
                {
                    return "All";
                }

                return proxyGroups.First(g => g.Id == id).Name;
            }
            catch
            {
                return "Invalid";
            }
        }
        #endregion

        public void Dispose()
        {
            try
            {
                botsInfoTimer?.Dispose();
                secondsTicker?.Dispose();
                soundPlayer?.Dispose();

                MultiRunJob.OnCompleted -= UpdateOnCompleted;
                MultiRunJob.OnResult -= UpdateViewModel;
                MultiRunJob.OnStatusChanged -= UpdateStatus;
                MultiRunJob.OnProgress -= UpdateViewModel;

                MultiRunJob.OnResult -= OnResult;
                MultiRunJob.OnResult -= PlayHitSound;
                MultiRunJob.OnTaskError -= OnTaskError;
                MultiRunJob.OnError -= OnError;
                MultiRunJob.OnHit -= OnHit;
            }
            catch
            {

            }
        }
    }

    #region Other ViewModels
    public class BotViewModel : ViewModelBase
    {
        private readonly int index;
        private readonly BotData[] datas;

        private BotData BotData => datas.Length > index ? datas[index] : null;

        public int Id => index + 1;
        public string Data => BotData?.Line?.Data;
        public string Proxy => BotData?.Proxy?.ToString();
        public string Info => BotData?.ExecutionInfo;

        public BotViewModel(int index, BotData[] datas)
        {
            this.index = index;
            this.datas = datas;
        }

        public override void UpdateViewModel()
        {
            OnPropertyChanged(nameof(Data));
            OnPropertyChanged(nameof(Proxy));
            OnPropertyChanged(nameof(Info));
        }
    }

    public class HitViewModel : ViewModelBase
    {
        public Hit Hit { get; init; }

        public DateTime Time => Hit.Date;
        public string Data => Hit.Data.Data;
        public string Proxy => Hit.Proxy?.ToString();
        public string Type => Hit.Type;
        public string Capture => Hit.CapturedDataString;

        public HitViewModel(Hit hit)
        {
            Hit = hit;
        }
    }
    #endregion

    public enum HitsFilter
    {
        Hits,
        Custom,
        ToCheck
    }
}
