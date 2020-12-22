using Microsoft.EntityFrameworkCore;
using OpenBullet2.Models.Data;
using OpenBullet2.Models.Hits;
using OpenBullet2.Models.Proxies;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using RuriLib.Models.UserAgents;
using RuriLib.Services;
using System;
using System.Linq;

namespace OpenBullet2.Models.Jobs
{
    public class JobFactoryService
    {
        private readonly ConfigService configService;
        private readonly RuriLibSettingsService settingsService;
        private readonly IHitRepository hitRepo;
        private readonly ProxySourceFactoryService proxySourceFactory;
        private readonly DataPoolFactoryService dataPoolFactory;
        private readonly ProxyReloadService proxyReloadService;
        private readonly IRandomUAProvider randomUAProvider;
        private readonly PluginRepository pluginRepo;

        public JobFactoryService(ConfigService configService, RuriLibSettingsService settingsService, PluginRepository pluginRepo,
            IHitRepository hitRepo, ProxySourceFactoryService proxySourceFactory, DataPoolFactoryService dataPoolFactory,
            ProxyReloadService proxyReloadService, IRandomUAProvider randomUAProvider)
        {
            this.configService = configService;
            this.settingsService = settingsService;
            this.pluginRepo = pluginRepo;
            this.hitRepo = hitRepo;
            this.proxySourceFactory = proxySourceFactory;
            this.dataPoolFactory = dataPoolFactory;
            this.proxyReloadService = proxyReloadService;
            this.randomUAProvider = randomUAProvider;
        }

        public Job FromOptions(int id, int ownerId, JobOptions options)
        {
            Job job = options switch
            {
                MultiRunJobOptions x => MakeMultiRunJob(x),
                ProxyCheckJobOptions x => MakeProxyCheckJob(x),
                _ => throw new NotImplementedException()
            };

            job.Id = id;
            job.OwnerId = ownerId;
            return job;
        }

        private MultiRunJob MakeMultiRunJob(MultiRunJobOptions options)
        {
            if (string.IsNullOrEmpty(options.ConfigId))
                throw new ArgumentException("No config specified");

            if (options.DataPool is WordlistDataPoolOptions dataPool && dataPool.WordlistId == -1)
                throw new ArgumentException("No wordlist specified");

            var hitOutputsFactory = new HitOutputFactory(hitRepo);

            var job = new MultiRunJob(settingsService, pluginRepo)
            {
                Config = configService.Configs.FirstOrDefault(c => c.Id == options.ConfigId),
                CreationTime = DateTime.Now,
                ProxyMode = options.ProxyMode,
                StartCondition = options.StartCondition,
                Bots = options.Bots,
                Skip = options.Skip,
                HitOutputs = options.HitOutputs.Select(o => hitOutputsFactory.FromOptions(o)).ToList(),
                ProxySources = options.ProxySources.Select(s => proxySourceFactory.FromOptions(s).Result).ToList(),
                RandomUAProvider = randomUAProvider
            };

            job.DataPool = dataPoolFactory.FromOptions(options.DataPool).Result;
            return job;
        }

        private ProxyCheckJob MakeProxyCheckJob(ProxyCheckJobOptions options)
        {
            var job = new ProxyCheckJob(settingsService, pluginRepo)
            {
                StartCondition = options.StartCondition,
                Bots = options.Bots,
                CheckOnlyUntested = options.CheckOnlyUntested,
                Url = options.Target.Url,
                SuccessKey = options.Target.SuccessKey,
                Timeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds)
            };

            job.GeoProvider = new DBIPProxyGeolocationProvider("dbip-country-lite.mmdb");

            var proxies = proxyReloadService.Reload(options.GroupId).Result;
            job.Proxies = options.CheckOnlyUntested
                ? proxies.Where(p => p.WorkingStatus == ProxyWorkingStatus.Untested)
                : proxies;

            job.Total = proxies.Count();
            job.Tested = proxies.Count(p => p.WorkingStatus != ProxyWorkingStatus.Untested);
            job.Working = proxies.Count(p => p.WorkingStatus == ProxyWorkingStatus.Working);
            job.NotWorking = proxies.Count(p => p.WorkingStatus == ProxyWorkingStatus.NotWorking);

            return job;
        }
    }
}
