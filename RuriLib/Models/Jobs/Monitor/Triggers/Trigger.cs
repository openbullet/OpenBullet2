using RuriLib.Models.Conditions.Comparisons;
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
    
    public class JobFinishedTrigger : Trigger
    {
        public override bool CheckStatus(Job job)
            => job.Status == JobStatus.Idle && job.Progress == 1f;
    }
    
    public class ProgressTrigger : Trigger
    {
        public NumComparison Comparison { get; set; }
        public float Amount { get; set; }

        public override bool CheckStatus(Job job)
            => Functions.Conditions.Conditions.Check(job.Progress * 100, Comparison, Amount);
    }

    public class TimeElapsedTrigger : Trigger
    {
        public NumComparison Comparison { get; set; }
        public int Seconds { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Hours { get; set; } = 0;
        public int Days { get; set; } = 0;

        public override bool CheckStatus(Job job)
            => Functions.Conditions.Conditions.Check(job.Elapsed, Comparison, new TimeSpan(Days, Hours, Minutes, Seconds));
    }

    public class TimeRemainingTrigger : Trigger
    {
        public NumComparison Comparison { get; set; }
        public int Seconds { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Hours { get; set; } = 0;
        public int Days { get; set; } = 0;

        public override bool CheckStatus(Job job)
            => Functions.Conditions.Conditions.Check(job.Remaining, Comparison, new TimeSpan(Days, Hours, Minutes, Seconds));
    }
}
