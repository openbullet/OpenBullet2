using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Proxies.Sources;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Native.Utils;
using RuriLib.Extensions;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Hits.HitOutputs;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Models.Proxies.ProxySources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace OpenBullet2.Native.ViewModels
{
    public class MultiRunJobViewerViewModel : ViewModelBase, IDisposable
    {
        private List<ProxyGroupEntity> proxyGroups;

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
            _ => throw new System.NotImplementedException()
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

        public MultiRunJobViewerViewModel(MultiRunJobViewModel jobVM)
        {
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
                    _ => throw new System.NotImplementedException()
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
                    _ => throw new System.NotImplementedException()
                };

                sb.Append(info);

                if (i < MultiRunJob.HitOutputs.Count - 1)
                {
                    sb.Append(" | ");
                }
            }

            HitOutputsInfo = sb.ToString();

            // TODO: Hook events
            MultiRunJob.OnCompleted += (s, e) =>
            {

            };
        }

        public override void UpdateViewModel()
        {
            
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

        public void Dispose()
        {
            // TODO: Unhook events
        }
    }
}
