using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Proxies.Sources;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenBullet2.Native.ViewModels
{
    public class MultiRunJobViewerViewModel : ViewModelBase, IDisposable
    {
        private readonly OpenBulletSettingsService obSettingsService;
        private readonly List<ProxyGroupEntity> proxyGroups;
        private readonly Timer timer;

        public event Action<object, string, Color> NewMessage;

        public MultiRunJobViewModel Job { get; set; }
        private MultiRunJob MultiRunJob => Job.Job as MultiRunJob;

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

        public string RemainingWaitString => MultiRunJob.StartCondition switch
        {
            RelativeTimeStartCondition r => (MultiRunJob.StartTime + r.StartAfter - DateTime.Now).ToString(@"hh\:mm\:ss"),
            AbsoluteTimeStartCondition a => (a.StartAt - DateTime.Now).ToString(@"hh\:mm\:ss"),
            _ => throw new NotImplementedException()
        };

        public bool CanStart => MultiRunJob.Status is JobStatus.Idle;
        public bool CanSkipWait => MultiRunJob.Status is JobStatus.Waiting;
        public bool CanPause => MultiRunJob.Status is JobStatus.Running;
        public bool CanResume => MultiRunJob.Status is JobStatus.Paused;
        public bool CanStop => MultiRunJob.Status is JobStatus.Running or JobStatus.Paused;
        public bool CanAbort => MultiRunJob.Status is JobStatus.Running or JobStatus.Paused or JobStatus.Pausing or JobStatus.Stopping;
        public bool IsStopping => MultiRunJob.Status is JobStatus.Stopping;
        public bool IsWaiting => MultiRunJob.Status is JobStatus.Waiting;
        public bool IsPausing => MultiRunJob.Status is JobStatus.Pausing;

        public double Progress => Math.Clamp(MultiRunJob.Progress * 100, 0, 100);

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

        public bool EnableJobLog => obSettingsService.Settings.GeneralSettings.EnableJobLogging;

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

        public MultiRunJobViewerViewModel(MultiRunJobViewModel jobVM)
        {
            obSettingsService = SP.GetService<OpenBulletSettingsService>();
            Job = jobVM;

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

            MultiRunJob.OnCompleted += UpdateViewModel;
            MultiRunJob.OnResult += UpdateViewModel;
            MultiRunJob.OnStatusChanged += UpdateViewModel;
            MultiRunJob.OnProgress += UpdateViewModel;

            MultiRunJob.OnResult += OnResult;
            MultiRunJob.OnTaskError += OnTaskError;
            MultiRunJob.OnError += OnError;
            MultiRunJob.OnHit += OnHit;

            timer = new Timer(new TimerCallback(_ => RefreshBotsInfo()), null, 100, 100);
            UpdateHitsCollection();
        }

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

        private void UpdateViewModel(object sender, EventArgs e) => UpdateViewModel();
        private void UpdateViewModel(object sender, ResultDetails<MultiRunInput, CheckResult> details) => UpdateViewModel();
        private void UpdateViewModel(object sender, JobStatus status) => UpdateViewModel();
        private void UpdateViewModel(object sender, float progress) => UpdateViewModel();

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

        private void OnHit(object sender, Hit hit)
        {
            if ((HitsFilter == HitsFilter.Hits && hit.Type == "SUCCESS") || (HitsFilter == HitsFilter.ToCheck && hit.Type == "NONE")
                || (HitsFilter == HitsFilter.Custom && hit.Type != "SUCCESS" && hit.Type != "NONE"))
            {
                // TODO: Add the hit to the observable (it was giving inconsistency errors when i tried)
                // For now we just update the entire thing
                UpdateHitsCollection();
            }
        }

        public async Task Start()
        {
            await MultiRunJob.Start();
            UpdateBotsCollection();
        }

        public Task Stop() => MultiRunJob.Stop();
        public Task Abort() => MultiRunJob.Abort();
        public Task Pause() => MultiRunJob.Pause();
        public Task Resume() => MultiRunJob.Resume();
        public void SkipWait() => MultiRunJob.SkipWait();

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

        // Call this at the start and when bots are changed
        private void UpdateBotsCollection()
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

        public void Dispose()
        {
            try
            {
                timer?.Dispose();

                MultiRunJob.OnCompleted -= UpdateViewModel;
                MultiRunJob.OnResult -= UpdateViewModel;
                MultiRunJob.OnStatusChanged -= UpdateViewModel;
                MultiRunJob.OnProgress -= UpdateViewModel;

                MultiRunJob.OnResult -= OnResult;
                MultiRunJob.OnTaskError -= OnTaskError;
                MultiRunJob.OnError -= OnError;
                MultiRunJob.OnHit -= OnHit;
            }
            catch
            {

            }
        }
    }

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
    }

    public class HitViewModel : ViewModelBase
    {
        private readonly Hit hit;

        public DateTime Time => hit.Date;
        public string Data => hit.Data.Data;
        public string Proxy => hit.Proxy?.ToString();
        public string Type => hit.Type;
        public string Capture => hit.CapturedDataString;

        public HitViewModel(Hit hit)
        {
            this.hit = hit;
        }
    }

    public enum HitsFilter
    {
        Hits,
        Custom,
        ToCheck
    }
}
