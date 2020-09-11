using OpenBullet2.Models.Hits;
using OpenBullet2.Models.Proxies;
using RuriLib.Helpers;
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
                JobType.MultiRun => MakeMultiRun(),
                JobType.ProxyCheck => MakeProxyCheck(),
                _ => throw new NotImplementedException()
            };

            options.StartCondition = new RelativeTimeStartCondition();
            return options;
        }

        private MultiRunJobOptions MakeMultiRun()
        {
            return new MultiRunJobOptions
            {
                HitOutputs = new List<HitOutputOptions> { new DatabaseHitOutputOptions() },
                ProxySources = new List<ProxySourceOptions> { new GroupProxySourceOptions() { GroupId = -1 } }
            };
        }

        private ProxyCheckJobOptions MakeProxyCheck()
        {
            return new ProxyCheckJobOptions
            {
                CheckOutput = new DatabaseProxyCheckOutputOptions()
            };
        }

        public JobOptions CloneExistant(JobOptions options)
        {
            return options switch
            {
                MultiRunJobOptions x => CloneMultiRun(x),
                ProxyCheckJobOptions x => Cloner.Clone(x),
                _ => throw new NotImplementedException()
            };
        }

        private MultiRunJobOptions CloneMultiRun(MultiRunJobOptions options)
        {
            var newOptions = Cloner.Clone(options);
            newOptions.Skip = 0;
            return newOptions;
        }
    }
}
