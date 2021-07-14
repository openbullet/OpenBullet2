using RuriLib.Models.Jobs.Monitor;
using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenBullet2.Services
{
    public class JobMonitorService : IDisposable
    {
        public bool Initialized { get; set; } = false;
        public List<TriggeredAction> TriggeredActions { get; set; } = new List<TriggeredAction>();
        private readonly Timer timer;
        private readonly JobManagerService jobManager;

        public JobMonitorService(JobManagerService jobManager)
        {
            this.jobManager = jobManager;
            timer = new Timer(new TimerCallback(_ => CheckAndExecute()), null, 1000, 1000);
        }

        private void CheckAndExecute()
        {
            for (var i = 0; i < TriggeredActions.Count; i++)
            {
                var action = TriggeredActions[i];
                
                if (action.IsActive && !action.IsExecuting && (action.IsRepeatable || action.Executions == 0))
                {
                    action.CheckAndExecute(jobManager.Jobs).ConfigureAwait(false);
                }
            }
        }

        public void Dispose() => timer?.Dispose();
    }
}
