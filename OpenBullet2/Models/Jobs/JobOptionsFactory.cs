using RuriLib.Models.Jobs.StartConditions;
using System;

namespace OpenBullet2.Models.Jobs
{
    public class JobOptionsFactory
    {
        public JobOptions CreateNew(JobType type)
        {
            JobOptions options = type switch
            {
                JobType.SingleRun => new SingleRunJobOptions(),
                JobType.MultiRun => new MultiRunJobOptions(),
                _ => throw new NotImplementedException()
            };

            options.StartCondition = new RelativeTimeStartCondition();
            return options;
        }
    }
}
