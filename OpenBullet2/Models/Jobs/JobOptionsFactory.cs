using RuriLib.Models.Jobs;
using System;

namespace OpenBullet2.Models.Jobs
{
    public class JobOptionsFactory
    {
        public JobOptions GetOptions(Job job)
        {
            return job switch
            {
                SingleRunJob x => BuildSingleRunOptions(x),
                _ => throw new NotImplementedException()
            };
        }

        private SingleRunJobOptions BuildSingleRunOptions(SingleRunJob job)
        {
            return new SingleRunJobOptions
            {
                ConfigId = job.Config?.Id,
                Data = job.Data,
                Proxy = job.Proxy,
                ProxyMode = job.ProxyMode,
                ProxyType = job.ProxyType,
                HitOutputs = job.HitOutputs,
                StartCondition = job.StartCondition,
                WordlistType = job.WordlistType
            };
        }
    }
}
