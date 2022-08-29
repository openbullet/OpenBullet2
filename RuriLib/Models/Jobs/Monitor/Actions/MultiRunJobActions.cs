using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs.Monitor.Actions
{
    public class MultiRunJobAction : Action
    {
        public int TargetJobId { get; set; }

        public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
            => Execute(jobs.First(j => j.Id == TargetJobId) as MultiRunJob);

        public virtual Task Execute(MultiRunJob job)
            => throw new NotImplementedException();
    }

    public class SetBotsAction : MultiRunJobAction
    {
        public int Amount { get; set; }

        public override Task Execute(MultiRunJob job)
        {
            if (Amount is > 0 and <= 200)
                job.Bots = Amount;

            return Task.CompletedTask;
        }
    }

    public class ReloadProxiesAction : MultiRunJobAction
    {
        public override Task Execute(MultiRunJob job) => job.FetchProxiesFromSources();
    }
}
