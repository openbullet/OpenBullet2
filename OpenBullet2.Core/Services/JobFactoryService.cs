using Microsoft.Extensions.Configuration;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Repositories;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;
using System;
using System.Linq;

namespace OpenBullet2.Core.Services
{
    /// <summary>
    /// Factory that creates a <see cref="Job"/> from <see cref="JobOptions"/>.
    /// </summary>
    public class JobFactoryService
    {
        private readonly ConfigService configService;
        private readonly RuriLibSettingsService settingsService;
        private readonly HitStorageService hitStorage;
        private readonly ProxySourceFactoryService proxySourceFactory;
        private readonly DataPoolFactoryService dataPoolFactory;
        private readonly ProxyReloadService proxyReloadService;
        private readonly IRandomUAProvider randomUAProvider;
        private readonly IRNGProvider rngProvider;
        private readonly IJobLogger logger;
        private readonly IProxyRepository proxyRepo;
        private readonly PluginRepository pluginRepo;
        
        /// <summary>
        /// The maximum amount of bots that a job can use.
        /// </summary>
        public int BotLimit { get; init; } = 200;

        public JobFactoryService(ConfigService configService, RuriLibSettingsService settingsService, PluginRepository pluginRepo,
            HitStorageService hitStorage, ProxySourceFactoryService proxySourceFactory, DataPoolFactoryService dataPoolFactory,
            ProxyReloadService proxyReloadService, IRandomUAProvider randomUAProvider, IRNGProvider rngProvider, IJobLogger logger,
            IProxyRepository proxyRepo, IConfiguration config)
        {
            this.configService = configService;
            this.settingsService = settingsService;
            this.pluginRepo = pluginRepo;
            this.hitStorage = hitStorage;
            this.proxySourceFactory = proxySourceFactory;
            this.dataPoolFactory = dataPoolFactory;
            this.proxyReloadService = proxyReloadService;
            this.randomUAProvider = randomUAProvider;
            this.rngProvider = rngProvider;
            this.logger = logger;
            this.proxyRepo = proxyRepo;

            var botLimit = config.GetSection("Resources")["BotLimit"];

            if (botLimit is not null)
            {
                BotLimit = int.Parse(botLimit);
            }
        }

        /// <summary>
        /// Creates a <see cref="Job"/> with the provided <paramref name="id"/> and <paramref name="ownerId"/>
        /// from <see cref="JobOptions"/>.
        /// </summary>
        /// <param name="id">The ID of the newly created job, must be unique</param>
        /// <param name="ownerId">The ID of the user who owns the job. 0 for admin</param>
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
            var hitOutputsFactory = new HitOutputFactory(hitStorage);

            var job = new MultiRunJob(settingsService, pluginRepo, logger)
            {
                Config = configService.Configs.FirstOrDefault(c => c.Id == options.ConfigId),
                CreationTime = DateTime.Now,
                ProxyMode = options.ProxyMode,
                ShuffleProxies = options.ShuffleProxies,
                NoValidProxyBehaviour = options.NoValidProxyBehaviour,
                NeverBanProxies = options.NeverBanProxies,
                MarkAsToCheckOnAbort = options.MarkAsToCheckOnAbort,
                ProxyBanTime = TimeSpan.FromSeconds(options.ProxyBanTimeSeconds),
                ConcurrentProxyMode = options.ConcurrentProxyMode,
                PeriodicReloadInterval = TimeSpan.FromSeconds(options.PeriodicReloadIntervalSeconds),
                StartCondition = options.StartCondition,
                Bots = options.Bots,
                BotLimit = BotLimit,
                CurrentBotDatas = new BotData[BotLimit],
                Skip = options.Skip,
                HitOutputs = options.HitOutputs.Select(o => hitOutputsFactory.FromOptions(o)).ToList(),
                ProxySources = options.ProxySources.Select(s => proxySourceFactory.FromOptions(s).Result).ToList(),
                Providers = new(settingsService)
                {
                    RandomUA = settingsService.RuriLibSettings.GeneralSettings.UseCustomUserAgentsList
                        ? new DefaultRandomUAProvider(settingsService)
                        : randomUAProvider,
                    RNG = rngProvider
                }
            };

            job.DataPool = dataPoolFactory.FromOptions(options.DataPool).Result;
            return job;
        }

        private ProxyCheckJob MakeProxyCheckJob(ProxyCheckJobOptions options)
        {
            var job = new ProxyCheckJob(settingsService, pluginRepo, logger)
            {
                StartCondition = options.StartCondition,
                Bots = options.Bots,
                BotLimit = BotLimit,
                CheckOnlyUntested = options.CheckOnlyUntested,
                Url = options.Target.Url,
                SuccessKey = options.Target.SuccessKey,
                Timeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds)
            };

            job.GeoProvider = new DBIPProxyGeolocationProvider("dbip-country-lite.mmdb");
            job.Proxies = proxyReloadService.ReloadAsync(options.GroupId, job.OwnerId).Result;
            
            // Update the stats
            var proxies = options.CheckOnlyUntested
                ? job.Proxies.Where(p => p.WorkingStatus == ProxyWorkingStatus.Untested)
                : job.Proxies;

            job.Total = proxies.Count();
            job.Tested = proxies.Count(p => p.WorkingStatus != ProxyWorkingStatus.Untested);
            job.Working = proxies.Count(p => p.WorkingStatus == ProxyWorkingStatus.Working);
            job.NotWorking = proxies.Count(p => p.WorkingStatus == ProxyWorkingStatus.NotWorking);
            job.ProxyOutput = new DatabaseProxyCheckOutput(proxyRepo);

            return job;
        }
    }
}
