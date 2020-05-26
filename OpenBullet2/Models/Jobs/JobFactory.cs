using OpenBullet2.Models.Hits;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Models.Hits;
using RuriLib.Models.Jobs;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBullet2.Models.Jobs
{
    public class JobFactory
    {
        private readonly ConfigService configService;
        private readonly RuriLibSettingsService settingsService;
        private readonly SingletonDbHitRepository hitRepo;

        public JobFactory(ConfigService configService, RuriLibSettingsService settingsService,
            SingletonDbHitRepository hitRepo)
        {
            this.configService = configService;
            this.settingsService = settingsService;
            this.hitRepo = hitRepo;
        }

        public Job CreateNew(JobType type)
        {

            return type switch
            {
                JobType.SingleRun => CreateSingleRun(),
                JobType.MultiRun => new MultiRunJob(settingsService),
                JobType.Spider => new SpiderJob(settingsService),
                JobType.Ripper => new RipJob(settingsService),
                JobType.SeleniumUnitTest => new SeleniumUnitTestJob(settingsService),
                _ => throw new NotImplementedException()
            };
        }

        private SingleRunJob CreateSingleRun()
        {
            return new SingleRunJob(settingsService)
            {
                HitOutputs = new List<IHitOutput> { new DatabaseHitOutput(hitRepo) }
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
            var job = new SingleRunJob(settingsService)
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

            // We have to re-initialize the DatabaseHitOutput and give it the singleton repository
            // TODO: Move these to a HitsOutput factory!
            var dbOutput = job.HitOutputs.FirstOrDefault(o => o is DatabaseHitOutput);
            
            if (dbOutput != null)
            {
                job.HitOutputs.Remove(dbOutput);
                job.HitOutputs.Add(new DatabaseHitOutput(hitRepo));
            }

            return job;
        }
    }
}
