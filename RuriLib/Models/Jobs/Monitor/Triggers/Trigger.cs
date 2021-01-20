using System;

namespace RuriLib.Models.Jobs.Monitor.Triggers
{
    public abstract class Trigger
    {
        public virtual bool CheckStatus(Job job)
            => throw new NotImplementedException();
    }

    public class JobStatusTrigger : Trigger
    {
        public JobStatus Status { get; set; } = JobStatus.Idle;

        public override bool CheckStatus(Job job)
            => job.Status == Status;
    }
}
