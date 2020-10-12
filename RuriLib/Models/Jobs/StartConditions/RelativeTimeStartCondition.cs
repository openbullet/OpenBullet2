using System;

namespace RuriLib.Models.Jobs.StartConditions
{
    public class RelativeTimeStartCondition : StartCondition
    {
        public TimeSpan StartAfter { get; set; } = TimeSpan.Zero;

        public override bool Verify(Job job)
            => StartAfter < DateTime.Now - job.StartTime;
    }
}
