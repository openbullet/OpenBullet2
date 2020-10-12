using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs.Monitor
{
    // TODO: Add some log output to see errors or just activities that have been performed, like the cron jobs log
    public class TriggeredAction
    {
        public bool IsActive { get; set; } = true;
        public bool IsExecuting { get; set; } = false;
        public bool IsRepeatable { get; set; } = false;
        public int Executions { get; private set; } = 0;

        // The job this triggered action refers to
        public int JobId { get; set; }

        // All triggers must be verified at the same time
        public List<Trigger> Triggers { get; set; } = new List<Trigger>();

        // Actions are executed sequentially, so stop - delay - start is possible
        public List<Action> Actions { get; set; } = new List<Action>();

        // Fire and forget
        public async Task CheckAndExecute(IEnumerable<Job> jobs)
        {
            var job = jobs.FirstOrDefault(j => j.Id == JobId);

            if (job == null)
                return;

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
                            await action.Execute(JobId, jobs);
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
