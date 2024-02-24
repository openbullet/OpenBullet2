using RuriLib.Models.Jobs.Monitor.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Action = RuriLib.Models.Jobs.Monitor.Actions.Action;

namespace RuriLib.Models.Jobs.Monitor
{
    // TODO: Add some log output to see errors or just activities that have been performed, like the cron jobs log
    public class TriggeredAction
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public string Name { get; init; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsExecuting { get; set; }
        public bool IsRepeatable { get; set; }
        public int Executions { get; set; }

        // The job this triggered action refers to
        public int JobId { get; set; }

        // All triggers must be verified at the same time
        public List<Trigger> Triggers { get; init; } = [];

        // Actions are executed sequentially, so stop - delay - start is possible
        public List<Action> Actions { get; init; } = [];

        // Fire and forget
        public async Task CheckAndExecute(IEnumerable<Job> jobs)
        {
            var jobsArray = jobs as Job[] ?? jobs.ToArray();
            var job = jobsArray.FirstOrDefault(j => j.Id == JobId);

            if (job == null)
            {
                return;
            }

            try
            {
                // Check the status of triggers on the current job
                if (Triggers.All(t => t.CheckStatus(job)))
                {
                    Executions++;
                    IsExecuting = true;

                    foreach (var action in Actions)
                    {
                        // Try to execute action on current job or any of the other jobs
                        try
                        {
                            await action.Execute(JobId, jobsArray);
                        }
                        catch
                        {
                            // Something went bad with actions
                        }
                    }
                }
            }
            catch
            {
                // Something went bad with triggers (maybe the job isn't there anymore)
            }
            finally
            {
                IsExecuting = false;
            }
        }

        public void Reset()
        {
            IsExecuting = false;
            Executions = 0;
        }
    }
}
