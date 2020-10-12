using System;

namespace RuriLib.Models.Jobs.StartConditions
{
    public class AbsoluteTimeStartCondition : StartCondition
    {
        public DateTime StartAt { get; set; } = DateTime.Now + TimeSpan.FromMinutes(1);

        public override bool Verify(Job job)
            => StartAt < DateTime.Now;
    }
}
