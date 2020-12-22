using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs
{
    public abstract class Job
    {
        public int Id { get; set; }
        public int OwnerId { get; set; } = 0;
        public JobStatus Status { get; protected set; } = JobStatus.Idle;
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public StartCondition StartCondition { get; set; } = new RelativeTimeStartCondition();

        // Events
        public event EventHandler Finished;

        protected void OnFinished()
        {
            Finished.Invoke(this, EventArgs.Empty);
        }

        protected CancellationTokenSource cts;
        protected readonly RuriLibSettingsService settings;
        protected readonly PluginRepository pluginRepo;

        public Job(RuriLibSettingsService settings, PluginRepository pluginRepo)
        {
            this.settings = settings;
            this.pluginRepo = pluginRepo;
        }

        public virtual async Task Start()
        {
            StartTime = DateTime.Now;
            Status = JobStatus.Waiting;
            await StartCondition.WaitUntilVerified(this);
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
