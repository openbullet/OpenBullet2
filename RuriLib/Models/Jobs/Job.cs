using RuriLib.Logging;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs
{
    // Todo: Implement IDisposable and dispose the following when a job is deleted or edited
    // - GroupProxySource
    // - DatabaseHitOutput
    // - DatabaseProxyCheckOutput
    public abstract class Job
    {
        // Public properties
        public int Id { get; set; }
        public int OwnerId { get; set; } = 0;
        public JobStatus Status { get; protected set; } = JobStatus.Idle;
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public StartCondition StartCondition { get; set; } = new RelativeTimeStartCondition();

        // Virtual properties
        public virtual float Progress => throw new NotImplementedException();

        // Protected fields
        protected readonly RuriLibSettingsService settings;
        protected readonly PluginRepository pluginRepo;
        protected readonly IJobLogger logger;

        // Private fields
        private bool waitFinished = false;
        private CancellationTokenSource cts; // Cancellation token for cancelling the StartCondition wait

        public Job(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger logger = null)
        {
            this.settings = settings;
            this.pluginRepo = pluginRepo;
            this.logger = logger;
        }

        public virtual async Task Start(CancellationToken cancellationToken = default)
        {
            waitFinished = false;
            cts?.Dispose();
            cts = new CancellationTokenSource();

            StartTime = DateTime.Now;
            Status = JobStatus.Waiting;

            try
            {
                logger?.LogInfo(Id, "Waiting for the start condition to be verified...");
                await StartCondition.WaitUntilVerified(this, cts.Token);
                logger?.LogInfo(Id, "Finished waiting");
            }
            catch (TaskCanceledException)
            {
                // The token has been cancelled, skip the wait
                logger?.LogInfo(Id, "The wait has been manually skipped");
            }

            waitFinished = true;
        }

        public void SkipWait()
        {
            if (!waitFinished && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }

        public virtual Task Pause()
        {
            throw new NotImplementedException();
        }

        public virtual Task Resume()
        {
            throw new NotImplementedException();
        }

        public virtual Task Stop()
        {
            throw new NotImplementedException();
        }

        public virtual Task Abort()
        {
            throw new NotImplementedException();
        }
    }
}
