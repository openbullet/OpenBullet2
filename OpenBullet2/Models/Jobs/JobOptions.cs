using RuriLib.Models.Jobs.StartConditions;

namespace OpenBullet2.Models.Jobs
{
    public abstract class JobOptions
    {
        public StartCondition StartCondition { get; set; } = new RelativeTimeStartCondition();
    }
}
