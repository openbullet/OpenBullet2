using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies.Sources;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Tests.Extensions;
using RuriLib.Logging;
using RuriLib.Models.Configs;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Services;
using System.Text.Json;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class JobIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    /// <summary>
    /// Admin can get all jobs, even ones of guests, ordered by id.
    /// </summary>
    [Fact]
    public async Task GetAll_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 2;
        var pcJob = CreateProxyCheckJob();
        pcJob.Name = "Test PCJ";
        pcJob.Id = 1;
        pcJob.OwnerId = 1;
        jobManager.AddJob(mrJob);
        jobManager.AddJob(pcJob);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<JobOverviewDto>>(
            client, "/api/v1/job/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Collection(result.Value,
            j =>
            {
                Assert.Equal(pcJob.Id, j.Id);
                Assert.Equal(pcJob.Name, j.Name);
                Assert.Equal(1, j.OwnerId);
                Assert.Equal(JobType.ProxyCheck, j.Type);
            },
            j =>
            {
                Assert.Equal(mrJob.Id, j.Id);
                Assert.Equal(mrJob.Name, j.Name);
                Assert.Equal(0, j.OwnerId);
                Assert.Equal(JobType.MultiRun, j.Type);
            });
    }
    
    /// <summary>
    /// Guest can get all their jobs, ordered by id.
    /// </summary>
    [Fact]
    public async Task GetAll_Guest_OnlyOwnJobs()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest" };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 2;
        mrJob.OwnerId = guest.Id;
        var pcJob = CreateProxyCheckJob();
        pcJob.Name = "Test PCJ";
        pcJob.Id = 1;
        jobManager.AddJob(mrJob);
        jobManager.AddJob(pcJob);
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<JobOverviewDto>>(
            client, "/api/v1/job/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Collection(result.Value,
            j =>
            {
                Assert.Equal(mrJob.Id, j.Id);
                Assert.Equal(mrJob.Name, j.Name);
                Assert.Equal(guest.Id, j.OwnerId);
                Assert.Equal(JobType.MultiRun, j.Type);
            });
    }
    
    /// <summary>
    /// Admin can get all multi run jobs, even ones of guests, ordered by id.
    /// </summary>
    [Fact]
    public async Task GetAllMultiRunJobs_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 3;
        mrJob.Config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" }
        };
        mrJob.DataPool = new CombinationsDataPool("abc", 3);
        mrJob.Bots = 10;
        mrJob.ProxyMode = JobProxyMode.On;
        var mrJob2 = CreateMultiRunJob();
        mrJob2.Name = "Test MRJ2";
        mrJob2.Id = 2;
        mrJob2.OwnerId = 1;
        var pcJob = CreateProxyCheckJob();
        pcJob.Name = "Test PCJ";
        pcJob.Id = 1;
        jobManager.AddJob(mrJob);
        jobManager.AddJob(mrJob2);
        jobManager.AddJob(pcJob);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<MultiRunJobOverviewDto>>(
            client, "/api/v1/job/multi-run/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Collection(result.Value,
            j =>
            {
                Assert.Equal(mrJob2.Id, j.Id);
                Assert.Equal(mrJob2.Name, j.Name);
                Assert.Equal(1, j.OwnerId);
                Assert.Equal(JobType.MultiRun, j.Type);
            },
            j =>
            {
                Assert.Equal(mrJob.Id, j.Id);
                Assert.Equal(mrJob.Name, j.Name);
                Assert.Equal(0, j.OwnerId);
                Assert.Equal(JobType.MultiRun, j.Type);
                Assert.Equal(mrJob.Config.Metadata.Name, j.ConfigName);
                Assert.Contains("Combinations", j.DataPoolInfo);
                Assert.Equal(mrJob.Bots, j.Bots);
                Assert.Equal(mrJob.ProxyMode, j.ProxyMode);
            });
    }
    
    /// <summary>
    /// Guest can get all their multi run jobs, ordered by id.
    /// </summary>
    [Fact]
    public async Task GetAllMultiRunJobs_Guest_OnlyOwnJobs()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest" };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 3;
        mrJob.OwnerId = guest.Id;
        var mrJob2 = CreateMultiRunJob();
        mrJob2.Name = "Test MRJ2";
        mrJob2.Id = 2;
        var pcJob = CreateProxyCheckJob();
        pcJob.Name = "Test PCJ";
        pcJob.Id = 1;
        pcJob.OwnerId = guest.Id;
        jobManager.AddJob(mrJob);
        jobManager.AddJob(mrJob2);
        jobManager.AddJob(pcJob);
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<MultiRunJobOverviewDto>>(
            client, "/api/v1/job/multi-run/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Collection(result.Value,
            j => Assert.Equal(mrJob.Name, j.Name));
    }
    
    /// <summary>
    /// Admin can get all proxy check jobs, even ones of guests, ordered by id.
    /// </summary>
    [Fact]
    public async Task GetAllProxyCheckJobs_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 3;
        var pcJob = CreateProxyCheckJob();
        pcJob.Name = "Test PCJ";
        pcJob.Id = 2;
        pcJob.Bots = 10;
        var pcJob2 = CreateProxyCheckJob();
        pcJob2.Name = "Test PCJ2";
        pcJob2.Id = 1;
        pcJob2.OwnerId = 1;
        jobManager.AddJob(mrJob);
        jobManager.AddJob(pcJob);
        jobManager.AddJob(pcJob2);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<ProxyCheckJobOverviewDto>>(
            client, "/api/v1/job/proxy-check/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Collection(result.Value,
            j =>
            {
                Assert.Equal(pcJob2.Id, j.Id);
                Assert.Equal(pcJob2.Name, j.Name);
                Assert.Equal(1, j.OwnerId);
                Assert.Equal(JobType.ProxyCheck, j.Type);
            },
            j =>
            {
                Assert.Equal(pcJob.Id, j.Id);
                Assert.Equal(pcJob.Name, j.Name);
                Assert.Equal(0, j.OwnerId);
                Assert.Equal(JobType.ProxyCheck, j.Type);
                Assert.Equal(pcJob.Bots, j.Bots);
            });
    }
    
    /// <summary>
    /// Guest can get all their proxy check jobs, ordered by id.
    /// </summary>
    [Fact]
    public async Task GetAllProxyCheckJobs_Guest_OnlyOwnJobs()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest" };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 3;
        var pcJob = CreateProxyCheckJob();
        pcJob.Name = "Test PCJ";
        pcJob.Id = 2;
        var pcJob2 = CreateProxyCheckJob();
        pcJob2.Name = "Test PCJ2";
        pcJob2.Id = 1;
        pcJob2.OwnerId = guest.Id;
        jobManager.AddJob(mrJob);
        jobManager.AddJob(pcJob);
        jobManager.AddJob(pcJob2);
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<ProxyCheckJobOverviewDto>>(
            client, "/api/v1/job/proxy-check/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Collection(result.Value,
            j => Assert.Equal(pcJob2.Name, j.Name));
    }
    
    /// <summary>
    /// Admin can get the details of a multi run job.
    /// </summary>
    [Fact]
    public async Task GetMultiRunJob_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var proxyReloadService = GetRequiredService<ProxyReloadService>();
        var hitStorageService = GetRequiredService<HitStorageService>();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 2;
        mrJob.Skip = 5;
        mrJob.ProxySources = [new GroupProxySource(-1, proxyReloadService)];
        mrJob.HitOutputs = [new DatabaseHitOutput(hitStorageService)];
        mrJob.StartCondition = new RelativeTimeStartCondition
        {
            StartAfter = TimeSpan.FromSeconds(1)
        };
        mrJob.Config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" }
        };
        mrJob.DataPool = new CombinationsDataPool("abc", 3);
        mrJob.Bots = 10;
        mrJob.ProxyMode = JobProxyMode.On;
        jobManager.AddJob(mrJob);
        
        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var result = await GetJsonAsync<MultiRunJobDto>(
            client, "/api/v1/job/multi-run".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(mrJob.Id, result.Value.Id);
        Assert.Equal(mrJob.Name, result.Value.Name);
        Assert.Equal(mrJob.Skip, result.Value.Skip);
        Assert.Single(result.Value.ProxySources);
        Assert.Single(result.Value.HitOutputs);
        // TODO: Check the start condition
        Assert.Equal(mrJob.Config.Metadata.Name, result.Value.Config!.Name);
        Assert.Contains("Combinations", result.Value.DataPoolInfo);
        Assert.Equal(mrJob.Bots, result.Value.Bots);
        Assert.Equal(mrJob.ProxyMode, result.Value.ProxyMode);
        Assert.NotNull(result.Value.DataStats);
        Assert.NotNull(result.Value.ProxyStats);
    }

    /// <summary>
    /// Guest cannot get the details of a multi run job not owned by them.
    /// </summary>
    [Fact]
    public async Task GetMultiRunJob_Guest_NotOwned_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest" };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 2;
        jobManager.AddJob(mrJob);
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var result = await GetJsonAsync<MultiRunJobDto>(
            client, "/api/v1/job/multi-run".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    // Guest can get the details of their multi run job
    
    // Admin can get the details of a proxy check job
    
    // Guest cannot get the details of a proxy check job not owned by them
    
    // Guest can get the details of their proxy check job
    
    // Admin can get the options of a multi run job
    
    // Admin can get the default options of a multi run job
    
    // Guest cannot get the options of a multi run job not owned by them
    
    // Guest can get the options of their multi run job
    
    // Admin can get the options of a proxy check job
    
    // Admin can get the default options of a proxy check job
    
    // Guest cannot get the options of a proxy check job not owned by them
    
    // Guest can get the options of their proxy check job
    
    // Admin can create a multi run job
    
    // Guest can create a multi run job
    
    // Admin can create a proxy check job
    
    // Guest can create a proxy check job
    
    // Admin can update a multi run job
    
    // Admin cannot update a multi run job that is not idle
    
    // Guest cannot update a multi run job not owned by them
    
    // Guest can update their multi run job
    
    // Admin can update a proxy check job
    
    // Admin cannot update a proxy check job that is not idle
    
    // Guest cannot update a proxy check job not owned by them
    
    // Guest can update their proxy check job
    
    // Admin can get the custom inputs for a multi run job
    
    // Guest cannot get the custom inputs for a multi run job not owned by them
    
    // Guest can get the custom inputs for their multi run job
    
    // Admin can set the custom inputs for a multi run job
    
    // Guest cannot set the custom inputs for a multi run job not owned by them
    
    // Guest can set the custom inputs for their multi run job
    
    // Admin can delete a job
    
    // Guest cannot delete a job not owned by them
    
    // Guest can delete their job
    
    // Admin can delete all jobs
    
    // Admin cannot delete all jobs if there are running jobs
    
    // Guest can delete all their jobs
    
    // Admin can get the hit log for a hit in a multi run job
    
    // Guest cannot get the hit log for a hit in a multi run job not owned by them
    
    // Guest can get the hit log for a hit in their multi run job
    
    // Admin can start a job
    
    // TODO: Test all actions for various job states
    
    // Guest cannot start a job not owned by them
    
    // Guest can start their job
    
    // Admin can stop a job
    
    // Guest cannot stop a job not owned by them
    
    // Guest can stop their job
    
    // Admin can pause a job
    
    // Guest cannot pause a job not owned by them
    
    // Guest can pause their job
    
    // Admin can resume a job
    
    // Guest cannot resume a job not owned by them
    
    // Guest can resume their job
    
    // Admin can abort a job
    
    // Guest cannot abort a job not owned by them
    
    // Guest can abort their job
    
    // Admin can skip a job's waiting time
    
    // Guest cannot skip a job's waiting time not owned by them
    
    // Guest can skip their job's waiting time
    
    // Admin can change the number of bots in a job
    
    // Guest cannot change the number of bots in a job not owned by them
    
    // Guest can change the number of bots in their job

    private MultiRunJob CreateMultiRunJob()
    {
        var logger = GetRequiredService<IJobLogger>();
        var ruriLibSettingsService = GetRequiredService<RuriLibSettingsService>();
        var pluginRepository = GetRequiredService<PluginRepository>();
        var job = new MultiRunJob(ruriLibSettingsService, pluginRepository, logger)
        {
            DataPool = new InfiniteDataPool()
        };
        return job;
    }
    
    private ProxyCheckJob CreateProxyCheckJob()
    {
        var logger = GetRequiredService<IJobLogger>();
        var ruriLibSettingsService = GetRequiredService<RuriLibSettingsService>();
        var pluginRepository = GetRequiredService<PluginRepository>();
        return new ProxyCheckJob(ruriLibSettingsService, pluginRepository, logger);
    }
    
    private JobEntity CreateJobEntity(Job job, JobOptions? options = null,
        GuestEntity? owner = null)
    {
        var jsonOptions = options is null
            ? "{}"
            : JsonSerializer.Serialize(options, JsonSerializerOptions);
        
        return new JobEntity
        {
            CreationDate = DateTime.Now,
            JobType = job switch
            {
                MultiRunJob _ => JobType.MultiRun,
                ProxyCheckJob _ => JobType.ProxyCheck,
                _ => throw new NotImplementedException()    
            },
            JobOptions = jsonOptions,
            Owner = owner
        };
    }
}
