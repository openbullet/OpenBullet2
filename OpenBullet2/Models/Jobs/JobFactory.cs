using OpenBullet2.Services;
using RuriLib.Models.Jobs;
using System;
using System.Linq;

namespace OpenBullet2.Models.Jobs
{
    public class JobFactory
    {
        private readonly ConfigService configService;

        public JobFactory(ConfigService configService)
        {
            this.configService = configService;
        }

        public Job Create(int id, JobOptions options)
        {
            var job = options switch
            {
                SingleRunJobOptions x => MakeSingleRunJob(x),
                _ => throw new NotImplementedException()
            };

            job.Id = id;
            return job;
        }

        private SingleRunJob MakeSingleRunJob(SingleRunJobOptions options)
        {
            return new SingleRunJob()
            {
                Config = configService.Configs.FirstOrDefault(c => c.Id == options.ConfigId),
                CreationTime = DateTime.Now,
                Data = options.Data,
                HitOutputs = options.HitOutputs,
                Proxy = options.Proxy,
                ProxyMode = options.ProxyMode,
                ProxyType = options.ProxyType,
                WordlistType = options.WordlistType,
                StartCondition = options.StartCondition
            };
        }
    }
}
