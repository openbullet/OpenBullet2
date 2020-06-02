using OpenBullet2.Models.Hits;
using OpenBullet2.Models.Proxies;
using RuriLib.Models.Jobs.StartConditions;
using System;
using System.Collections.Generic;

namespace OpenBullet2.Models.Jobs
{
    public class JobOptionsFactory
    {
        public JobOptions CreateNew(JobType type)
        {
            JobOptions options = type switch
            {
                JobType.SingleRun => new SingleRunJobOptions() 
                    { HitOutputs = new List<HitOutputOptions> { new DatabaseHitOutputOptions() } },
                JobType.MultiRun => new MultiRunJobOptions()
                    { HitOutputs = new List<HitOutputOptions> { new DatabaseHitOutputOptions() } },
                JobType.ProxyCheck => new ProxyCheckJobOptions()
                    { CheckOutput = new DatabaseProxyCheckOutputOptions() },
                _ => throw new NotImplementedException()
            };

            options.StartCondition = new RelativeTimeStartCondition();
            return options;
        }
    }
}
