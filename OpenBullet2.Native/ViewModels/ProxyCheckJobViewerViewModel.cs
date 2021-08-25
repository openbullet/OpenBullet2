using OpenBullet2.Core.Services;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Models.Proxies;
using RuriLib.Parallelization.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OpenBullet2.Native.ViewModels
{
    public class ProxyCheckJobViewerViewModel : ViewModelBase, IDisposable
    {
        private readonly Timer secondsTicker;

        public event Action<object, string, Color> NewMessage;

        public ProxyCheckJobViewModel Job { get; set; }
        private ProxyCheckJob ProxyCheckJob => Job.Job as ProxyCheckJob;

        #region Properties that need to be updated every second
        public string RemainingWaitString => ProxyCheckJob.StartCondition switch
        {
            RelativeTimeStartCondition r => (ProxyCheckJob.StartTime + r.StartAfter - DateTime.Now).ToString(@"hh\:mm\:ss"),
            AbsoluteTimeStartCondition a => (a.StartAt - DateTime.Now).ToString(@"hh\:mm\:ss"),
            _ => throw new NotImplementedException()
        };
        #endregion

        #region Properties that need to be updated when the status changes
        public bool CanStart => ProxyCheckJob.Status is JobStatus.Idle;
        public bool CanSkipWait => ProxyCheckJob.Status is JobStatus.Waiting;
        public bool CanPause => ProxyCheckJob.Status is JobStatus.Running;
        public bool CanResume => ProxyCheckJob.Status is JobStatus.Paused;
        public bool CanStop => ProxyCheckJob.Status is JobStatus.Running or JobStatus.Paused;
        public bool CanAbort => ProxyCheckJob.Status is JobStatus.Running or JobStatus.Paused or JobStatus.Pausing or JobStatus.Stopping;

        public bool IsStopping => ProxyCheckJob.Status is JobStatus.Stopping;
        public bool IsWaiting => ProxyCheckJob.Status is JobStatus.Waiting;
        public bool IsPausing => ProxyCheckJob.Status is JobStatus.Pausing;
        #endregion

        #region Properties that need to be updated when a new result comes in
        public double Progress => Math.Clamp(ProxyCheckJob.Progress * 100, 0, 100);
        #endregion

        public ProxyCheckJobViewerViewModel(ProxyCheckJobViewModel jobVM)
        {
            Job = jobVM;

            #region Bind events and timers
            ProxyCheckJob.OnCompleted += UpdateOnCompleted;
            ProxyCheckJob.OnResult += UpdateViewModel;
            ProxyCheckJob.OnStatusChanged += UpdateStatus;
            ProxyCheckJob.OnProgress += UpdateViewModel;

            ProxyCheckJob.OnResult += OnResult;
            ProxyCheckJob.OnTaskError += OnTaskError;
            ProxyCheckJob.OnError += OnError;

            secondsTicker = new Timer(new TimerCallback(_ => PeriodicUpdate()), null, 1000, 1000);
            #endregion
        }

        #region Update methods
        // Periodic update for stuff that needs to be updated every second
        private void PeriodicUpdate()
        {
            if (ProxyCheckJob.Status == JobStatus.Waiting)
            {
                OnPropertyChanged(nameof(RemainingWaitString));
            }

            Job.PeriodicUpdate();
        }

        // Updates everything (only when a job completes, just to be safe, not expensive)
        private void UpdateOnCompleted(object sender, EventArgs e) => UpdateViewModel();

        // Updates the stats after every successful check
        private void UpdateViewModel(object sender, ResultDetails<ProxyCheckInput, Proxy> details)
        {
            OnPropertyChanged(nameof(Progress));
            Job.UpdateStats();
        }

        // Update the stuff related to a job's status change
        private void UpdateStatus(object sender, JobStatus status)
        {
            Job.UpdateStatus();

            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanSkipWait));
            OnPropertyChanged(nameof(CanResume));
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(CanAbort));

            OnPropertyChanged(nameof(IsStopping));
            OnPropertyChanged(nameof(IsWaiting));
            OnPropertyChanged(nameof(IsPausing));
        }


        private void UpdateViewModel(object sender, float progress) => UpdateViewModel();
        #endregion

        #region Logging
        private void OnResult(object sender, ResultDetails<ProxyCheckInput, Proxy> details)
        {
            var proxy = details.Result;

            var message = $"Proxy checked ({proxy}) with ping {proxy.Ping} ms and country {proxy.Country}";
            var color = proxy.WorkingStatus == ProxyWorkingStatus.Working ? Colors.YellowGreen : Colors.Tomato;

            NewMessage?.Invoke(this, message, color);
        }

        private void OnTaskError(object sender, ErrorDetails<ProxyCheckInput> details)
        {
            var message = $"Task error ({details.Item.Proxy})! {details.Exception.Message}";
            NewMessage?.Invoke(this, message, Colors.Tomato);
        }

        private void OnError(object sender, Exception ex)
            => NewMessage?.Invoke(this, $"Job error: {ex.Message}", Colors.Tomato);
        #endregion

        #region Controls
        public Task Start() => ProxyCheckJob.Start();

        public Task Stop() => ProxyCheckJob.Stop();
        public Task Abort() => ProxyCheckJob.Abort();
        public Task Pause() => ProxyCheckJob.Pause();
        public Task Resume() => ProxyCheckJob.Resume();
        public void SkipWait() => ProxyCheckJob.SkipWait();

        public async Task ChangeBots(int newValue)
        {
            // TODO: Also edit the job options! So the number of bots is persisted

            await ProxyCheckJob.ChangeBots(newValue);
            ProxyCheckJob.Bots = newValue;
            Job.UpdateBots();
        }
        #endregion

        public void Dispose()
        {
            try
            {
                secondsTicker?.Dispose();

                ProxyCheckJob.OnCompleted -= UpdateOnCompleted;
                ProxyCheckJob.OnResult -= UpdateViewModel;
                ProxyCheckJob.OnStatusChanged -= UpdateStatus;
                ProxyCheckJob.OnProgress -= UpdateViewModel;

                ProxyCheckJob.OnResult -= OnResult;
                ProxyCheckJob.OnTaskError -= OnTaskError;
                ProxyCheckJob.OnError -= OnError;
            }
            catch
            {

            }
        }
    }
}
