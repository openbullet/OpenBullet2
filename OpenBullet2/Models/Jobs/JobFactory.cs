using Microsoft.EntityFrameworkCore;
using OpenBullet2.Models.Data;
using OpenBullet2.Models.Hits;
using OpenBullet2.Models.Proxies;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
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
                ProxyCheckJobOptions x => MakeProxyCheckJob(x),
                _ => throw new NotImplementedException()
            };

            job.Id = id;
            return job;
        }

        private SingleRunJob MakeSingleRunJob(SingleRunJobOptions options)
        {
            return new SingleRunJob(settingsService)
            {
                Config = configService.Configs.FirstOrDefault(c => c.Id == options.ConfigId),
                CreationTime = DateTime.Now,
                Data = options.Data,
                Proxy = options.Proxy,
                ProxyMode = options.ProxyMode,
                ProxyType = options.ProxyType,
                WordlistType = options.WordlistType,
                StartCondition = options.StartCondition,
                HitOutputs = options.HitOutputs.Select(o => new HitOutputFactory(hitRepo).FromOptions(o)).ToList()
            };
        }

        private MultiRunJob MakeMultiRunJob(MultiRunJobOptions options)
        {
            var job = new MultiRunJob(settingsService)
            {
                Config = configService.Configs.FirstOrDefault(c => c.Id == options.ConfigId),
                CreationTime = DateTime.Now,
                ProxyMode = options.ProxyMode,
                StartCondition = options.StartCondition,
                Bots = options.Bots,
                Skip = options.Skip,
                HitOutputs = options.HitOutputs.Select(o => new HitOutputFactory(hitRepo).FromOptions(o)).ToList(),
                ProxySource = new ProxySourceFactory(proxyGroupsRepo, proxyRepo).FromOptions(options.ProxySource).Result
            };

            job.DataPool = new DataPoolFactory(wordlistRepo, settingsService).FromOptions(options.DataPool).Result;
            return job;
        }

        private ProxyCheckJob MakeProxyCheckJob(ProxyCheckJobOptions options)
        {
            var job = new ProxyCheckJob(settingsService)
            {
                StartCondition = options.StartCondition,
                Bots = options.Bots,
                CheckOnlyUntested = options.CheckOnlyUntested,
                Url = options.Target.Url,
                SuccessKey = options.Target.SuccessKey,
                Timeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds)
            };

            var factory = new ProxyFactory();
            var entities = options.GroupId == -1
                ? proxyRepo.GetAll().ToListAsync().Result
                : proxyRepo.GetAll().Where(p => p.GroupId == options.GroupId).ToListAsync().Result;

            if (!options.CheckOnlyUntested)
                entities.ForEach(e => e.Status = ProxyWorkingStatus.Untested);

            job.GeoProvider = new DBIPProxyGeolocationProvider("dbip-country-lite.mmdb");

            job.Proxies = entities.Select(e => factory.FromEntity(e));
            job.ProxyOutput = new ProxyCheckOutputFactory(proxyRepo).FromOptions(options.CheckOutput);

            job.Total = entities.Count();
            job.Tested = entities.Count(p => p.Status != ProxyWorkingStatus.Untested);
            job.Working = entities.Count(p => p.Status == ProxyWorkingStatus.Working);
            job.NotWorking = entities.Count(p => p.Status == ProxyWorkingStatus.NotWorking);

            return job;
        }
    }
}
