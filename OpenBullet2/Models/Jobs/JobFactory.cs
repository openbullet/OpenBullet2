using OpenBullet2.Models.Data;
using OpenBullet2.Models.Hits;
using OpenBullet2.Models.Proxies;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Models.Jobs;
using RuriLib.Services;
using System;
using System.Linq;

namespace OpenBullet2.Models.Jobs
{
    public class JobFactory
    {
        private readonly ConfigService configService;
        private readonly RuriLibSettingsService settingsService;
        private readonly IHitRepository hitRepo;
        private readonly IProxyRepository proxyRepo;
        private readonly IProxyGroupRepository proxyGroupsRepo;
        private readonly IWordlistRepository wordlistRepo;

        public JobFactory(ConfigService configService, RuriLibSettingsService settingsService,
            IHitRepository hitRepo, IProxyRepository proxyRepo, IProxyGroupRepository proxyGroupsRepo,
            IWordlistRepository wordlistRepo)
        {
            this.configService = configService;
            this.settingsService = settingsService;
            this.hitRepo = hitRepo;
            this.proxyRepo = proxyRepo;
            this.proxyGroupsRepo = proxyGroupsRepo;
            this.wordlistRepo = wordlistRepo;
        }

        public Job FromOptions(int id, JobOptions options)
        {
            Job job = options switch
            {
                SingleRunJobOptions x => MakeSingleRunJob(x),
                MultiRunJobOptions x => MakeMultiRunJob(x),
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

        private MultiRunJob MakeMultiRunJob(MultiRunJobOptions options)
        {
            var job = new MultiRunJob(settingsService)
            {
                Config = configService.Configs.FirstOrDefault(c => c.Id == options.ConfigId),
                CreationTime = DateTime.Now,
                HitOutputs = options.HitOutputs,
                ProxyMode = options.ProxyMode,
                StartCondition = options.StartCondition
            };

            job.ProxySource = new ProxySourceFactory(proxyGroupsRepo, proxyRepo).FromOptions(options.ProxySource).Result;
            job.DataPool = new DataPoolFactory(wordlistRepo, settingsService).FromOptions(options.DataPool).Result;

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
