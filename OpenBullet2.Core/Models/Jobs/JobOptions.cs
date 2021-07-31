using RuriLib.Models.Jobs.StartConditions;

namespace OpenBullet2.Core.Models.Jobs
{
    public abstract class JobOptions
    {
        public StartCondition StartCondition { get; set; } = new RelativeTimeStartCondition();
    }
}
