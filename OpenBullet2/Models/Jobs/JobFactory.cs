using OpenBullet2.Models.Hits;
using OpenBullet2.Services;
using RuriLib.Models.Hits;
using RuriLib.Models.Jobs;
using System;
using System.Collections.Generic;
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

        public Job CreateNew(JobType type)
        {
            return type switch
            {
                JobType.SingleRun => CreateSingleRun(),
                JobType.MultiRun => new MultiRunJob(),
                JobType.Spider => new SpiderJob(),
                JobType.Ripper => new RipJob(),
                JobType.SeleniumUnitTest => new SeleniumUnitTestJob(),
                _ => throw new NotImplementedException()
            };
        }

        private SingleRunJob CreateSingleRun()
        {
            return new SingleRunJob
            {
                HitOutputs = new List<IHitOutput> { new DatabaseHitOutput() }
            };
        }

        public Job FromOptions(int id, JobOptions options)
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
