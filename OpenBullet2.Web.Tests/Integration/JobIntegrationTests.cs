using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Models.Proxies.Sources;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Hit;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Dtos.Job.MultiRun;
using OpenBullet2.Web.Dtos.Job.ProxyCheck;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Tests.Extensions;
using RuriLib.Logging;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Hits;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Services;
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
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
                Assert.Equal(mrJob.ShouldUseProxies(), j.UseProxies);
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
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
    public async Task GetMultiRunJob_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
        Assert.Equal(ErrorCode.JobNotFound, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Guest can get the details of their multi run job.
    /// </summary>
    [Fact]
    public async Task GetMultiRunJob_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 2;
        mrJob.OwnerId = guest.Id;
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
        Assert.True(result.IsSuccess);
        Assert.Equal(mrJob.Id, result.Value.Id);
        Assert.Equal(mrJob.Name, result.Value.Name);
    }
    
    /// <summary>
    /// Admin can get the details of a proxy check job.
    /// </summary>
    [Fact]
    public async Task GetProxyCheckJob_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var proxyCheckOutputFactory = GetRequiredService<ProxyCheckOutputFactory>();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var pcJob = CreateProxyCheckJob();
        pcJob.Id = 1;
        pcJob.Name = "Test PCJ";
        pcJob.Bots = 10;
        pcJob.StartCondition = new RelativeTimeStartCondition
        {
            StartAfter = TimeSpan.FromSeconds(1)
        };
        pcJob.Url = "https://example.com";
        pcJob.SuccessKey = "<title>Example</title>";
        pcJob.Timeout = TimeSpan.FromSeconds(10);
        pcJob.ProxyOutput = proxyCheckOutputFactory.FromOptions(new DatabaseProxyCheckOutputOptions());
        var jobEntity = CreateProxyCheckJobEntity(pcJob);
        jobEntity.Id = pcJob.Id;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(pcJob);
        await dbContext.SaveChangesAsync();
        
        // Act
        var queryParams = new
        {
            id = pcJob.Id
        };
        var result = await GetJsonAsync<ProxyCheckJobDto>(
            client, "/api/v1/job/proxy-check".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(pcJob.Id, result.Value.Id);
        Assert.Equal(pcJob.Name, result.Value.Name);
        Assert.Equal(pcJob.Bots, result.Value.Bots);
        Assert.Equal(pcJob.Url, result.Value.Target!.Url);
        Assert.Equal(pcJob.SuccessKey, result.Value.Target!.SuccessKey);
        Assert.Equal(pcJob.Timeout.TotalMilliseconds, result.Value.TimeoutMilliseconds);
        Assert.Contains("database", result.Value.CheckOutput.ToLower());
    }
    
    /// <summary>
    /// Guest cannot get the details of a proxy check job not owned by them.
    /// </summary>
    [Fact]
    public async Task GetProxyCheckJob_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var pcJob = CreateProxyCheckJob();
        pcJob.Id = 1;
        pcJob.Name = "Test PCJ";
        var jobEntity = CreateProxyCheckJobEntity(pcJob);
        jobEntity.Id = pcJob.Id;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(pcJob);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = pcJob.Id
        };
        var result = await GetJsonAsync<ProxyCheckJobDto>(
            client, "/api/v1/job/proxy-check".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.JobNotFound, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Guest can get the details of their proxy check job.
    /// </summary>
    [Fact]
    public async Task GetProxyCheckJob_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var proxyCheckOutputFactory = GetRequiredService<ProxyCheckOutputFactory>();
        var guest = new GuestEntity { Id = 1, Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var pcJob = CreateProxyCheckJob();
        pcJob.Id = 1;
        pcJob.Name = "Test PCJ";
        pcJob.OwnerId = guest.Id;
        pcJob.Bots = 10;
        pcJob.StartCondition = new RelativeTimeStartCondition
        {
            StartAfter = TimeSpan.FromSeconds(1)
        };
        pcJob.Url = "https://example.com";
        pcJob.SuccessKey = "<title>Example</title>";
        pcJob.Timeout = TimeSpan.FromSeconds(10);
        pcJob.ProxyOutput = proxyCheckOutputFactory.FromOptions(new DatabaseProxyCheckOutputOptions());
        var jobEntity = CreateProxyCheckJobEntity(pcJob, owner: guest);
        jobEntity.Id = pcJob.Id;
        jobEntity.Owner = guest;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(pcJob);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = pcJob.Id
        };
        var result = await GetJsonAsync<ProxyCheckJobDto>(
            client, "/api/v1/job/proxy-check".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(pcJob.Id, result.Value.Id);
        Assert.Equal(pcJob.Name, result.Value.Name);
        Assert.Equal(pcJob.Bots, result.Value.Bots);
        Assert.Equal(pcJob.Url, result.Value.Target!.Url);
        Assert.Equal(pcJob.SuccessKey, result.Value.Target!.SuccessKey);
        Assert.Equal(pcJob.Timeout.TotalMilliseconds, result.Value.TimeoutMilliseconds);
        Assert.Contains("database", result.Value.CheckOutput.ToLower());
    }
    
    /// <summary>
    /// Admin can get the options of a multi run job.
    /// </summary>
    [Fact]
    public async Task GetMultiRunJobOptions_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        var db = GetRequiredService<ApplicationDbContext>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" }
        };
        await configRepository.SaveAsync(config);
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        jobManager.AddJob(mrJob);
        var jobOptions = new MultiRunJobOptions {
            Name = "Test MRJ",
            ConfigId = config.Id,
            Bots = 10,
            Skip = 5,
            ProxyMode = JobProxyMode.On,
            DataPool = new CombinationsDataPoolOptions { CharSet = "abc", Length = 2 },
            HitOutputs = [new DatabaseHitOutputOptions()],
            ProxySources = [new GroupProxySourceOptions { GroupId = -1 }],
            StartCondition = new RelativeTimeStartCondition { StartAfter = TimeSpan.FromSeconds(1) }
        };
        var jobEntity = CreateMultiRunJobEntity(mrJob, jobOptions);
        jobEntity.Id = mrJob.Id;
        db.Jobs.Add(jobEntity);
        await db.SaveChangesAsync();
        
        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var result = await GetJsonAsync<MultiRunJobOptionsDto>(
            client, "/api/v1/job/multi-run/options".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(jobOptions.ConfigId, result.Value.ConfigId);
        Assert.Equal(jobOptions.Bots, result.Value.Bots);
        Assert.Equal(jobOptions.Skip, result.Value.Skip);
        Assert.Equal(jobOptions.ProxyMode, result.Value.ProxyMode);
        Assert.Single(result.Value.HitOutputs);
        Assert.Single(result.Value.ProxySources);
    }
    
    /// <summary>
    /// Admin can get the default options of a multi run job.
    /// </summary>
    [Fact]
    public async Task GetMultiRunJobDefaultOptions_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var queryParams = new
        {
            id = -1
        };
        var result = await GetJsonAsync<MultiRunJobOptionsDto>(
            client, "/api/v1/job/multi-run/options".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
    }
    
    /// <summary>
    /// Guest cannot get the options of a multi run job not owned by them.
    /// </summary>
    [Fact]
    public async Task GetMultiRunJobOptions_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        var jobOptions = new MultiRunJobOptions {
            Name = "Test MRJ",
            ConfigId = Guid.NewGuid().ToString(),
            Bots = 10,
            Skip = 5,
            ProxyMode = JobProxyMode.On,
            DataPool = new CombinationsDataPoolOptions { CharSet = "abc", Length = 2 },
            HitOutputs = [new DatabaseHitOutputOptions()],
            ProxySources = [new GroupProxySourceOptions { GroupId = -1 }],
            StartCondition = new RelativeTimeStartCondition { StartAfter = TimeSpan.FromSeconds(1) }
        };
        var jobEntity = CreateMultiRunJobEntity(mrJob, jobOptions);
        jobEntity.Id = mrJob.Id;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var result = await GetJsonAsync<MultiRunJobOptionsDto>(
            client, "/api/v1/job/multi-run/options".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.JobNotFound, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Guest can get the options of their multi run job.
    /// </summary>
    [Fact]
    public async Task GetMultiRunJobOptions_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Id = 1, Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        mrJob.OwnerId = guest.Id;
        var jobOptions = new MultiRunJobOptions {
            Name = "Test MRJ",
            ConfigId = Guid.NewGuid().ToString(),
            Bots = 10,
            Skip = 5,
            ProxyMode = JobProxyMode.On,
            DataPool = new CombinationsDataPoolOptions { CharSet = "abc", Length = 2 },
            HitOutputs = [new DatabaseHitOutputOptions()],
            ProxySources = [new GroupProxySourceOptions { GroupId = -1 }],
            StartCondition = new RelativeTimeStartCondition { StartAfter = TimeSpan.FromSeconds(1) }
        };
        var jobEntity = CreateMultiRunJobEntity(mrJob, jobOptions);
        jobEntity.Id = mrJob.Id;
        jobEntity.Owner = guest;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var result = await GetJsonAsync<MultiRunJobOptionsDto>(
            client, "/api/v1/job/multi-run/options".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(jobOptions.ConfigId, result.Value.ConfigId);
        Assert.Equal(jobOptions.Bots, result.Value.Bots);
        Assert.Equal(jobOptions.Skip, result.Value.Skip);
        Assert.Equal(jobOptions.ProxyMode, result.Value.ProxyMode);
        Assert.Single(result.Value.HitOutputs);
        Assert.Single(result.Value.ProxySources);
    }
    
    /// <summary>
    /// Admin can get the options of a proxy check job.
    /// </summary>
    [Fact]
    public async Task GetProxyCheckJobOptions_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var pcJob = CreateProxyCheckJob();
        pcJob.Id = 1;
        pcJob.Name = "Test PCJ";
        var jobOptions = new ProxyCheckJobOptions {
            Bots = 10,
            GroupId = -1,
            CheckOnlyUntested = true,
            Target = new ProxyCheckTarget {
                Url = "https://example.com",
                SuccessKey = "<title>Example</title>"
            },
            TimeoutMilliseconds = 10000
        };
        var jobEntity = CreateProxyCheckJobEntity(pcJob, jobOptions);
        jobEntity.Id = pcJob.Id;
        jobEntity.Owner = null;
        var db = GetRequiredService<ApplicationDbContext>();
        db.Jobs.Add(jobEntity);
        jobManager.AddJob(pcJob);
        await db.SaveChangesAsync();
        
        // Act
        var queryParams = new
        {
            id = pcJob.Id
        };
        var result = await GetJsonAsync<ProxyCheckJobOptionsDto>(
            client, "/api/v1/job/proxy-check/options".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(jobOptions.Bots, result.Value.Bots);
        Assert.Equal(jobOptions.GroupId, result.Value.GroupId);
        Assert.Equal(jobOptions.CheckOnlyUntested, result.Value.CheckOnlyUntested);
        Assert.Equal(jobOptions.Target.Url, result.Value.Target!.Url);
        Assert.Equal(jobOptions.Target.SuccessKey, result.Value.Target!.SuccessKey);
        Assert.Equal(jobOptions.TimeoutMilliseconds, result.Value.TimeoutMilliseconds);
    }
    
    /// <summary>
    /// Admin can get the default options of a proxy check job.
    /// </summary>
    [Fact]
    public async Task GetProxyCheckJobDefaultOptions_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var queryParams = new
        {
            id = -1
        };
        var result = await GetJsonAsync<ProxyCheckJobOptionsDto>(
            client, "/api/v1/job/proxy-check/options".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
    }
    
    /// <summary>
    /// Guest cannot get the options of a proxy check job not owned by them.
    /// </summary>
    [Fact]
    public async Task GetProxyCheckJobOptions_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var pcJob = CreateProxyCheckJob();
        pcJob.Id = 1;
        pcJob.Name = "Test PCJ";
        var jobOptions = new ProxyCheckJobOptions {
            Bots = 10,
            GroupId = -1,
            CheckOnlyUntested = true,
            Target = new ProxyCheckTarget {
                Url = "https://example.com",
                SuccessKey = "<title>Example</title>"
            },
            TimeoutMilliseconds = 10000
        };
        var jobEntity = CreateProxyCheckJobEntity(pcJob, jobOptions);
        jobEntity.Id = pcJob.Id;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(pcJob);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = pcJob.Id
        };
        var result = await GetJsonAsync<ProxyCheckJobOptionsDto>(
            client, "/api/v1/job/proxy-check/options".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.JobNotFound, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Guest can get the options of their proxy check job.
    /// </summary>
    [Fact]
    public async Task GetProxyCheckJobOptions_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Id = 1, Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var pcJob = CreateProxyCheckJob();
        pcJob.Id = 1;
        pcJob.Name = "Test PCJ";
        pcJob.OwnerId = guest.Id;
        var jobOptions = new ProxyCheckJobOptions {
            Bots = 10,
            GroupId = -1,
            CheckOnlyUntested = true,
            Target = new ProxyCheckTarget {
                Url = "https://example.com",
                SuccessKey = "<title>Example</title>"
            },
            TimeoutMilliseconds = 10000
        };
        var jobEntity = CreateProxyCheckJobEntity(pcJob, jobOptions);
        jobEntity.Id = pcJob.Id;
        jobEntity.Owner = guest;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(pcJob);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = pcJob.Id
        };
        var result = await GetJsonAsync<ProxyCheckJobOptionsDto>(
            client, "/api/v1/job/proxy-check/options".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(jobOptions.Bots, result.Value.Bots);
        Assert.Equal(jobOptions.GroupId, result.Value.GroupId);
        Assert.Equal(jobOptions.CheckOnlyUntested, result.Value.CheckOnlyUntested);
        Assert.Equal(jobOptions.Target.Url, result.Value.Target!.Url);
        Assert.Equal(jobOptions.Target.SuccessKey, result.Value.Target!.SuccessKey);
        Assert.Equal(jobOptions.TimeoutMilliseconds, result.Value.TimeoutMilliseconds);
    }
    
    // Admin can create a multi run job
    [Fact]
    public async Task CreateMultiRunJob_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" }
        };
        await configRepository.SaveAsync(config);
        
        // Act
        var dto = new CreateMultiRunJobDto
        {
            Name = "Test MRJ",
            ConfigId = config.Id,
            Bots = 10,
            Skip = 5,
            ProxyMode = JobProxyMode.On,
            DataPool = JsonSerializer.SerializeToElement(new InfiniteDataPoolOptionsDto
            {
                PolyTypeName = "infiniteDataPool"
            }, JsonSerializerOptions),
            HitOutputs = [JsonSerializer.SerializeToElement(new DatabaseHitOutputOptionsDto
            {
                PolyTypeName = "databaseHitOutput"
            }, JsonSerializerOptions)],
            ProxySources = [JsonSerializer.SerializeToElement(new GroupProxySourceOptionsDto
            {
                GroupId = -1,
                PolyTypeName = "groupProxySource"
            }, JsonSerializerOptions)],
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var result = await PostJsonAsync<MultiRunJobDto>(
            client, "/api/v1/job/multi-run", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        var resultJob = result.Value;
        Assert.Equal(dto.Name, resultJob.Name);
    }
    
    // Guest can create a multi run job
    [Fact]
    public async Task CreateMultiRunJob_Guest_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" }
        };
        await configRepository.SaveAsync(config);
        
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var dto = new CreateMultiRunJobDto
        {
            Name = "Test MRJ",
            ConfigId = config.Id,
            Bots = 10,
            Skip = 5,
            ProxyMode = JobProxyMode.On,
            DataPool = JsonSerializer.SerializeToElement(new InfiniteDataPoolOptionsDto
            {
                PolyTypeName = "infiniteDataPool"
            }, JsonSerializerOptions),
            HitOutputs = [JsonSerializer.SerializeToElement(new DatabaseHitOutputOptionsDto
            {
                PolyTypeName = "databaseHitOutput"
            }, JsonSerializerOptions)],
            ProxySources = [JsonSerializer.SerializeToElement(new GroupProxySourceOptionsDto
            {
                GroupId = -1,
                PolyTypeName = "groupProxySource"
            }, JsonSerializerOptions)],
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var result = await PostJsonAsync<MultiRunJobDto>(
            client, "/api/v1/job/multi-run", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        var resultJob = result.Value;
        Assert.Equal(dto.Name, resultJob.Name);
        Assert.Equal(guest.Id, resultJob.OwnerId);
    }
    
    // Admin can create a proxy check job
    [Fact]
    public async Task CreateProxyCheckJob_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var dto = new CreateProxyCheckJobDto
        {
            Name = "Test PCJ",
            Bots = 10,
            GroupId = -1,
            CheckOnlyUntested = true,
            Target = new ProxyCheckTargetDto
            {
                Url = "https://example.com",
                SuccessKey = "<title>Example</title>"
            },
            CheckOutput = JsonSerializer.SerializeToElement(new DatabaseProxyCheckOutputOptionsDto
            {
                PolyTypeName = "databaseProxyCheckOutput"
            }, JsonSerializerOptions),
            TimeoutMilliseconds = 10000,
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var result = await PostJsonAsync<ProxyCheckJobDto>(
            client, "/api/v1/job/proxy-check", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        var resultJob = result.Value;
        Assert.Equal(dto.Name, resultJob.Name);
    }
    
    // Guest can create a proxy check job
    [Fact]
    public async Task CreateProxyCheckJob_Guest_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var dto = new CreateProxyCheckJobDto
        {
            Name = "Test PCJ",
            Bots = 10,
            GroupId = -1,
            CheckOnlyUntested = true,
            Target = new ProxyCheckTargetDto
            {
                Url = "https://example.com",
                SuccessKey = "<title>Example</title>"
            },
            CheckOutput = JsonSerializer.SerializeToElement(new DatabaseProxyCheckOutputOptionsDto
            {
                PolyTypeName = "databaseProxyCheckOutput"
            }, JsonSerializerOptions),
            TimeoutMilliseconds = 10000,
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var result = await PostJsonAsync<ProxyCheckJobDto>(
            client, "/api/v1/job/proxy-check", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        var resultJob = result.Value;
        Assert.Equal(dto.Name, resultJob.Name);
        Assert.Equal(guest.Id, resultJob.OwnerId);
    }
    
    // Admin can update a multi run job
    [Fact]
    public async Task UpdateMultiRunJob_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        jobManager.AddJob(mrJob);
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Jobs.Add(jobEntity);
        await dbContext.SaveChangesAsync();
        
        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" }
        };
        await configRepository.SaveAsync(config);
        
        // Act
        var dto = new UpdateMultiRunJobDto
        {
            Id = mrJob.Id,
            Name = "Test MRJ2",
            ConfigId = config.Id,
            Bots = 10,
            Skip = 5,
            ProxyMode = JobProxyMode.On,
            DataPool = JsonSerializer.SerializeToElement(new InfiniteDataPoolOptionsDto
            {
                PolyTypeName = "infiniteDataPool"
            }, JsonSerializerOptions),
            HitOutputs = [JsonSerializer.SerializeToElement(new DatabaseHitOutputOptionsDto
            {
                PolyTypeName = "databaseHitOutput"
            }, JsonSerializerOptions)],
            ProxySources = [JsonSerializer.SerializeToElement(new GroupProxySourceOptionsDto
            {
                GroupId = -1,
                PolyTypeName = "groupProxySource"
            }, JsonSerializerOptions)],
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var response = await PutJsonAsync<MultiRunJobDto>(
            client, "/api/v1/job/multi-run", dto);
        
        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(dto.Name, response.Value.Name);
        
        mrJob = jobManager.Jobs.OfType<MultiRunJob>().Single();
        Assert.Equal(dto.Name, mrJob.Name);
    }
    
    // Admin cannot update a multi run job that is not idle
    [Fact]
    public async Task UpdateMultiRunJob_Admin_NotIdle_BadRequest()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        mrJob.GetType().GetProperty("Status")!.SetValue(mrJob, JobStatus.Running);
        jobManager.AddJob(mrJob);
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Jobs.Add(jobEntity);
        await dbContext.SaveChangesAsync();
        
        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" }
        };
        await configRepository.SaveAsync(config);
        
        // Act
        var dto = new UpdateMultiRunJobDto
        {
            Id = mrJob.Id,
            Name = "Test MRJ2",
            ConfigId = config.Id,
            Bots = 10,
            Skip = 5,
            ProxyMode = JobProxyMode.On,
            DataPool = JsonSerializer.SerializeToElement(new InfiniteDataPoolOptionsDto
            {
                PolyTypeName = "infiniteDataPool"
            }, JsonSerializerOptions),
            HitOutputs = [JsonSerializer.SerializeToElement(new DatabaseHitOutputOptionsDto
            {
                PolyTypeName = "databaseHitOutput"
            }, JsonSerializerOptions)],
            ProxySources = [JsonSerializer.SerializeToElement(new GroupProxySourceOptionsDto
            {
                GroupId = -1,
                PolyTypeName = "groupProxySource"
            }, JsonSerializerOptions)],
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var response = await PutJsonAsync<MultiRunJobDto>(
            client, "/api/v1/job/multi-run", dto);
        
        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.Error);
        Assert.NotNull(response.Error.Content);
        Assert.Equal(ErrorCode.JobNotIdle, response.Error.Content.ErrorCode);
    }
    
    // Guest cannot update a multi run job not owned by them
    [Fact]
    public async Task UpdateMultiRunJob_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();

        RequireLogin();
        ImpersonateGuest(client, guest);

        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" }
        };
        await configRepository.SaveAsync(config);

        // Act
        var dto = new UpdateMultiRunJobDto
        {
            Id = mrJob.Id,
            Name = "Test MRJ2",
            ConfigId = config.Id,
            Bots = 10,
            Skip = 5,
            ProxyMode = JobProxyMode.On,
            DataPool = JsonSerializer.SerializeToElement(new InfiniteDataPoolOptionsDto
            {
                PolyTypeName = "infiniteDataPool"
            }, JsonSerializerOptions),
            HitOutputs =
            [
                JsonSerializer.SerializeToElement(new DatabaseHitOutputOptionsDto
                {
                    PolyTypeName = "databaseHitOutput"
                }, JsonSerializerOptions)
            ],
            ProxySources =
            [
                JsonSerializer.SerializeToElement(new GroupProxySourceOptionsDto
                {
                    GroupId = -1,
                    PolyTypeName = "groupProxySource"
                }, JsonSerializerOptions)
            ],
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var response = await PutJsonAsync<MultiRunJobDto>(
            client, "/api/v1/job/multi-run", dto);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.Error);
        Assert.NotNull(response.Error.Content);
        Assert.Equal(ErrorCode.JobNotFound, response.Error.Content.ErrorCode);
    }

    // Guest can update their multi run job
    [Fact]
    public async Task UpdateMultiRunJob_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Id = 1, Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        mrJob.OwnerId = guest.Id;
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        jobEntity.Owner = guest;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();

        RequireLogin();
        ImpersonateGuest(client, guest);

        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" }
        };
        await configRepository.SaveAsync(config);

        // Act
        var dto = new UpdateMultiRunJobDto
        {
            Id = mrJob.Id,
            Name = "Test MRJ2",
            ConfigId = config.Id,
            Bots = 10,
            Skip = 5,
            ProxyMode = JobProxyMode.On,
            DataPool = JsonSerializer.SerializeToElement(new InfiniteDataPoolOptionsDto
            {
                PolyTypeName = "infiniteDataPool"
            }, JsonSerializerOptions),
            HitOutputs =
            [
                JsonSerializer.SerializeToElement(new DatabaseHitOutputOptionsDto
                {
                    PolyTypeName = "databaseHitOutput"
                }, JsonSerializerOptions)
            ],
            ProxySources =
            [
                JsonSerializer.SerializeToElement(new GroupProxySourceOptionsDto
                {
                    GroupId = -1,
                    PolyTypeName = "groupProxySource"
                }, JsonSerializerOptions)
            ],
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var response = await PutJsonAsync<MultiRunJobDto>(
            client, "/api/v1/job/multi-run", dto);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(dto.Name, response.Value.Name);
        
        mrJob = jobManager.Jobs.OfType<MultiRunJob>().Single();
        Assert.Equal(dto.Name, mrJob.Name);
    }

    // Admin can update a proxy check job
    [Fact]
    public async Task UpdateProxyCheckJob_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var pcJob = CreateProxyCheckJob();
        pcJob.Id = 1;
        pcJob.Name = "Test PCJ";
        var jobEntity = CreateProxyCheckJobEntity(pcJob);
        jobEntity.Id = pcJob.Id;
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Jobs.Add(jobEntity);
        var jobManager = GetRequiredService<JobManagerService>();
        jobManager.AddJob(pcJob);
        await dbContext.SaveChangesAsync();
        
        // Act
        var dto = new UpdateProxyCheckJobDto
        {
            Id = pcJob.Id,
            Name = "Test PCJ2",
            Bots = 10,
            GroupId = -1,
            CheckOnlyUntested = true,
            Target = new ProxyCheckTargetDto
            {
                Url = "https://example.com",
                SuccessKey = "<title>Example</title>"
            },
            CheckOutput = JsonSerializer.SerializeToElement(new DatabaseProxyCheckOutputOptionsDto
            {
                PolyTypeName = "databaseProxyCheckOutput"
            }, JsonSerializerOptions),
            TimeoutMilliseconds = 10000,
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var response = await PutJsonAsync<ProxyCheckJobDto>(
            client, "/api/v1/job/proxy-check", dto);
        
        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(dto.Name, response.Value.Name);
        
        pcJob = jobManager.Jobs.OfType<ProxyCheckJob>().Single();
        Assert.Equal(dto.Name, pcJob.Name);
    }
    
    // Admin cannot update a proxy check job that is not idle
    [Fact]
    public async Task UpdateProxyCheckJob_Admin_NotIdle_BadRequest()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var pcJob = CreateProxyCheckJob();
        pcJob.Id = 1;
        pcJob.Name = "Test PCJ";
        pcJob.GetType().GetProperty("Status")!.SetValue(pcJob, JobStatus.Running);
        var jobEntity = CreateProxyCheckJobEntity(pcJob);
        jobEntity.Id = pcJob.Id;
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Jobs.Add(jobEntity);
        var jobManager = GetRequiredService<JobManagerService>();
        jobManager.AddJob(pcJob);
        await dbContext.SaveChangesAsync();
        
        // Act
        var dto = new UpdateProxyCheckJobDto
        {
            Id = pcJob.Id,
            Name = "Test PCJ2",
            Bots = 10,
            GroupId = -1,
            CheckOnlyUntested = true,
            Target = new ProxyCheckTargetDto
            {
                Url = "https://example.com",
                SuccessKey = "<title>Example</title>"
            },
            CheckOutput = JsonSerializer.SerializeToElement(new DatabaseProxyCheckOutputOptionsDto
            {
                PolyTypeName = "databaseProxyCheckOutput"
            }, JsonSerializerOptions),
            TimeoutMilliseconds = 10000,
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var response = await PutJsonAsync<ProxyCheckJobDto>(
            client, "/api/v1/job/proxy-check", dto);
        
        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.Error);
        Assert.NotNull(response.Error.Content);
        Assert.Equal(ErrorCode.JobNotIdle, response.Error.Content.ErrorCode);
    }
    
    // Guest cannot update a proxy check job not owned by them
    [Fact]
    public async Task UpdateProxyCheckJob_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var pcJob = CreateProxyCheckJob();
        pcJob.Id = 1;
        pcJob.Name = "Test PCJ";
        var jobEntity = CreateProxyCheckJobEntity(pcJob);
        jobEntity.Id = pcJob.Id;
        dbContext.Jobs.Add(jobEntity);
        var jobManager = GetRequiredService<JobManagerService>();
        jobManager.AddJob(pcJob);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var dto = new UpdateProxyCheckJobDto
        {
            Id = pcJob.Id,
            Name = "Test PCJ2",
            Bots = 10,
            GroupId = -1,
            CheckOnlyUntested = true,
            Target = new ProxyCheckTargetDto
            {
                Url = "https://example.com",
                SuccessKey = "<title>Example</title>"
            },
            CheckOutput = JsonSerializer.SerializeToElement(new DatabaseProxyCheckOutputOptionsDto
            {
                PolyTypeName = "databaseProxyCheckOutput"
            }, JsonSerializerOptions),
            TimeoutMilliseconds = 10000,
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var response = await PutJsonAsync<ProxyCheckJobDto>(
            client, "/api/v1/job/proxy-check", dto);
        
        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.Error);
        Assert.NotNull(response.Error.Content);
        Assert.Equal(ErrorCode.JobNotFound, response.Error.Content.ErrorCode);
    }
    
    // Guest can update their proxy check job
    [Fact]
    public async Task UpdateProxyCheckJob_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Id = 1, Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var pcJob = CreateProxyCheckJob();
        pcJob.Id = 1;
        pcJob.Name = "Test PCJ";
        pcJob.OwnerId = guest.Id;
        var jobEntity = CreateProxyCheckJobEntity(pcJob);
        jobEntity.Id = pcJob.Id;
        jobEntity.Owner = guest;
        dbContext.Jobs.Add(jobEntity);
        var jobManager = GetRequiredService<JobManagerService>();
        jobManager.AddJob(pcJob);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var dto = new UpdateProxyCheckJobDto
        {
            Id = pcJob.Id,
            Name = "Test PCJ2",
            Bots = 10,
            GroupId = -1,
            CheckOnlyUntested = true,
            Target = new ProxyCheckTargetDto
            {
                Url = "https://example.com",
                SuccessKey = "<title>Example</title>"
            },
            CheckOutput = JsonSerializer.SerializeToElement(new DatabaseProxyCheckOutputOptionsDto
            {
                PolyTypeName = "databaseProxyCheckOutput"
            }, JsonSerializerOptions),
            TimeoutMilliseconds = 10000,
            StartCondition = JsonSerializer.SerializeToElement(new RelativeTimeStartConditionDto
            {
                StartAfter = TimeSpan.FromSeconds(1),
                PolyTypeName = "relativeTimeStartCondition"
            }, JsonSerializerOptions)
        };
        var response = await PutJsonAsync<ProxyCheckJobDto>(
            client, "/api/v1/job/proxy-check", dto);
        
        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(dto.Name, response.Value.Name);
        
        pcJob = jobManager.Jobs.OfType<ProxyCheckJob>().Single();
        Assert.Equal(dto.Name, pcJob.Name);
    }
    
    // Admin can get the custom inputs for a multi run job
    [Fact]
    public async Task GetMultiRunJobCustomInputs_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();

        var customInputs = new List<CustomInput>()
        {
            new()
            {
                Description = "Test input",
                VariableName = "TEST",
                DefaultAnswer = "test"
            },

            new()
            {
                Description = "Test input 2",
                VariableName = "TEST2",
                DefaultAnswer = "test2"
            }
        };
        
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" },
            Settings = new ConfigSettings
            {
                InputSettings = new InputSettings
                {
                    CustomInputs = customInputs
                }
            }
        };
        var configRepository = GetRequiredService<IConfigRepository>();
        await configRepository.SaveAsync(config);
        mrJob.Config = config;
        mrJob.CustomInputsAnswers = new Dictionary<string, string>
        {
            { "TEST", "modified" }
        };
        
        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var response = await GetJsonAsync<IEnumerable<CustomInputQuestionDto>>(
            client, "/api/v1/job/multi-run/custom-inputs".ToUri(queryParams));
        
        // Assert
        Assert.True(response.IsSuccess);
        var result = response.Value.ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal(customInputs[0].Description, result[0].Description);
        Assert.Equal(customInputs[0].VariableName, result[0].VariableName);
        Assert.Equal(customInputs[0].DefaultAnswer, result[0].DefaultAnswer);
        Assert.Equal(mrJob.CustomInputsAnswers[customInputs[0].VariableName], result[0].CurrentAnswer);
        Assert.Equal(customInputs[1].Description, result[1].Description);
        Assert.Equal(customInputs[1].VariableName, result[1].VariableName);
        Assert.Equal(customInputs[1].DefaultAnswer, result[1].DefaultAnswer);
        Assert.Null(result[1].CurrentAnswer);
    }
    
    // Guest cannot get the custom inputs for a multi run job not owned by them
    [Fact]
    public async Task GetMultiRunJobCustomInputs_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var response = await GetJsonAsync<IEnumerable<CustomInputQuestionDto>>(
            client, "/api/v1/job/multi-run/custom-inputs".ToUri(queryParams));
        
        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.Error);
        Assert.NotNull(response.Error.Content);
        Assert.Equal(ErrorCode.JobNotFound, response.Error.Content.ErrorCode);
    }
    
    // Guest can get the custom inputs for their multi run job
    [Fact]
    public async Task GetMultiRunJobCustomInputs_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Id = 1, Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        mrJob.OwnerId = guest.Id;
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        jobEntity.Owner = guest;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();

        RequireLogin();
        ImpersonateGuest(client, guest);

        var customInputs = new List<CustomInput>()
        {
            new()
            {
                Description = "Test input",
                VariableName = "TEST",
                DefaultAnswer = "test"
            },

            new()
            {
                Description = "Test input 2",
                VariableName = "TEST2",
                DefaultAnswer = "test2"
            }
        };

        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata { Name = "Test Config" },
            Settings = new ConfigSettings
            {
                InputSettings = new InputSettings
                {
                    CustomInputs = customInputs
                }
            }
        };
        var configRepository = GetRequiredService<IConfigRepository>();
        await configRepository.SaveAsync(config);
        mrJob.Config = config;
        mrJob.CustomInputsAnswers = new Dictionary<string, string>
        {
            { "TEST", "modified" }
        };

        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var response = await GetJsonAsync<IEnumerable<CustomInputQuestionDto>>(
            client, "/api/v1/job/multi-run/custom-inputs".ToUri(queryParams));

        // Assert
        Assert.True(response.IsSuccess);
        var result = response.Value.ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal(customInputs[0].Description, result[0].Description);
        Assert.Equal(customInputs[0].VariableName, result[0].VariableName);
        Assert.Equal(customInputs[0].DefaultAnswer, result[0].DefaultAnswer);
        Assert.Equal(mrJob.CustomInputsAnswers[customInputs[0].VariableName], result[0].CurrentAnswer);
        Assert.Equal(customInputs[1].Description, result[1].Description);
        Assert.Equal(customInputs[1].VariableName, result[1].VariableName);
        Assert.Equal(customInputs[1].DefaultAnswer, result[1].DefaultAnswer);
        Assert.Null(result[1].CurrentAnswer);
    }

    // Admin can set the custom inputs for a multi run job
    [Fact]
    public async Task SetMultiRunJobCustomInputs_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();

        // Act
        var dto = new CustomInputsDto
        {
            JobId = mrJob.Id,
            Answers = [
                new CustomInputAnswerDto
                {
                    VariableName = "TEST",
                    Answer = "modified"
                }
            ]
        };
        var error = await PatchAsync(
            client, "/api/v1/job/multi-run/custom-inputs", dto);

        // Assert
        Assert.Null(error);
        Assert.Equal(dto.Answers.First().Answer, mrJob.CustomInputsAnswers["TEST"]);
    }
    
    // Guest cannot set the custom inputs for a multi run job not owned by them
    [Fact]
    public async Task SetMultiRunJobCustomInputs_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var dto = new CustomInputsDto
        {
            JobId = mrJob.Id,
            Answers = [
                new CustomInputAnswerDto
                {
                    VariableName = "TEST",
                    Answer = "modified"
                }
            ]
        };
        var error = await PatchAsync(
            client, "/api/v1/job/multi-run/custom-inputs", dto);

        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.BadRequest, error.Response.StatusCode);
        Assert.Equal(ErrorCode.JobNotFound, error.Content!.ErrorCode);
    }
    
    // Guest can set the custom inputs for their multi run job
    [Fact]
    public async Task SetMultiRunJobCustomInputs_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Id = 1, Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        mrJob.OwnerId = guest.Id;
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        jobEntity.Owner = guest;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var dto = new CustomInputsDto
        {
            JobId = mrJob.Id,
            Answers = [
                new CustomInputAnswerDto
                {
                    VariableName = "TEST",
                    Answer = "modified"
                }
            ]
        };
        var error = await PatchAsync(
            client, "/api/v1/job/multi-run/custom-inputs", dto);

        // Assert
        Assert.Null(error);
        Assert.Equal(dto.Answers.First().Answer, mrJob.CustomInputsAnswers["TEST"]);
    }
    
    // Admin can delete a job
    [Fact]
    public async Task DeleteJob_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        jobManager.AddJob(mrJob);
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Jobs.Add(jobEntity);
        await dbContext.SaveChangesAsync();
        
        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var error = await DeleteAsync(
            client, "/api/v1/job".ToUri(queryParams));
        
        // Assert
        Assert.Null(error);
        Assert.Empty(jobManager.Jobs);
        
        var jobEntities = await dbContext.Jobs.ToListAsync();
        Assert.Empty(jobEntities);
    }
    
    // Guest cannot delete a job not owned by them
    [Fact]
    public async Task DeleteJob_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var error = await DeleteAsync(
            client, "/api/v1/job".ToUri(queryParams));
        
        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.BadRequest, error.Response.StatusCode);
        Assert.Equal(ErrorCode.JobNotFound, error.Content!.ErrorCode);
        
        Assert.Single(jobManager.Jobs);
        
        var jobEntities = await dbContext.Jobs.ToListAsync();
        Assert.Single(jobEntities);
    }
    
    // Guest can delete their job
    [Fact]
    public async Task DeleteJob_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Id = 1, Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        mrJob.OwnerId = guest.Id;
        var jobEntity = CreateMultiRunJobEntity(mrJob);
        jobEntity.Id = mrJob.Id;
        jobEntity.Owner = guest;
        dbContext.Jobs.Add(jobEntity);
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var queryParams = new
        {
            id = mrJob.Id
        };
        var error = await DeleteAsync(
            client, "/api/v1/job".ToUri(queryParams));
        
        // Assert
        Assert.Null(error);
        Assert.Empty(jobManager.Jobs);
        
        var jobEntities = await dbContext.Jobs.ToListAsync();
        Assert.Empty(jobEntities);
    }
    
    // Admin can delete all jobs
    [Fact]
    public async Task DeleteAllJobs_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        
        var mrJob1 = CreateMultiRunJob();
        mrJob1.Name = "Test MRJ";
        mrJob1.Id = 1;
        jobManager.AddJob(mrJob1);
        var jobEntity1 = CreateMultiRunJobEntity(mrJob1);
        jobEntity1.Id = mrJob1.Id;
        
        var mrJob2 = CreateMultiRunJob();
        mrJob2.Name = "Test MRJ2";
        mrJob2.Id = 2;
        jobManager.AddJob(mrJob2);
        var jobEntity2 = CreateMultiRunJobEntity(mrJob2);
        jobEntity2.Id = mrJob2.Id;
        
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Jobs.AddRange(jobEntity1, jobEntity2);
        await dbContext.SaveChangesAsync();
        
        // Act
        var response = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/job/all");
        
        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(2, response.Value.Count);
        Assert.Empty(jobManager.Jobs);
        
        var jobEntities = await dbContext.Jobs.ToListAsync();
        Assert.Empty(jobEntities);
    }
    
    // Admin cannot delete all jobs if there are running jobs
    [Fact]
    public async Task DeleteAllJobs_Admin_NotIdle_BadRequest()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        
        var mrJob1 = CreateMultiRunJob();
        mrJob1.Name = "Test MRJ";
        mrJob1.Id = 1;
        mrJob1.GetType().GetProperty("Status")!.SetValue(mrJob1, JobStatus.Running);
        jobManager.AddJob(mrJob1);
        var jobEntity1 = CreateMultiRunJobEntity(mrJob1);
        jobEntity1.Id = mrJob1.Id;
        
        var mrJob2 = CreateMultiRunJob();
        mrJob2.Name = "Test MRJ2";
        mrJob2.Id = 2;
        jobManager.AddJob(mrJob2);
        var jobEntity2 = CreateMultiRunJobEntity(mrJob2);
        jobEntity2.Id = mrJob2.Id;
        
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Jobs.AddRange(jobEntity1, jobEntity2);
        await dbContext.SaveChangesAsync();
        
        // Act
        var response = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/job/all");
        
        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.Error);
        Assert.NotNull(response.Error.Content);
        Assert.Equal(ErrorCode.JobNotIdle, response.Error.Content.ErrorCode);
        
        Assert.Equal(2, jobManager.Jobs.Count());
        
        var jobEntities = await dbContext.Jobs.ToListAsync();
        Assert.Equal(2, jobEntities.Count);
    }
    
    // Guest can delete all their jobs (but not others)
    [Fact]
    public async Task DeleteAllJobs_Guest_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Id = 1, Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        
        var mrJob1 = CreateMultiRunJob();
        mrJob1.Name = "Test MRJ";
        mrJob1.Id = 1;
        mrJob1.OwnerId = guest.Id;
        jobManager.AddJob(mrJob1);
        var jobEntity1 = CreateMultiRunJobEntity(mrJob1);
        jobEntity1.Id = mrJob1.Id;
        jobEntity1.Owner = guest;
        
        var mrJob2 = CreateMultiRunJob();
        mrJob2.Name = "Test MRJ2";
        mrJob2.Id = 2;
        jobManager.AddJob(mrJob2);
        var jobEntity2 = CreateMultiRunJobEntity(mrJob2);
        jobEntity2.Id = mrJob2.Id;
        
        dbContext.Jobs.AddRange(jobEntity1, jobEntity2);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var response = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/job/all");
        
        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(1, response.Value.Count);
        Assert.Single(jobManager.Jobs);
        
        var jobEntities = await dbContext.Jobs.ToListAsync();
        Assert.Single(jobEntities);
    }
    
    // Admin can get the hit log for a hit in a multi run job
    [Fact]
    public async Task GetHitLog_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        jobManager.AddJob(mrJob);
        var botLogger = new BotLogger();
        botLogger.Log("Test message");
        botLogger.Log("Test message 2");
        var hit = new Hit
        {
            BotLogger = botLogger,
        };
        mrJob.Hits.Add(hit);
        
        // Act
        var queryParams = new
        {
            jobId = mrJob.Id,
            hitId = hit.Id
        };
        var response = await GetJsonAsync<MrjHitLogDto>(
            client, "/api/v1/job/multi-run/hit-log".ToUri(queryParams));
        
        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(2, response.Value.Log!.Count);
        Assert.Equal("Test message", response.Value.Log[0].Message);
        Assert.Equal("Test message 2", response.Value.Log[1].Message);
    }
    
    // Guest cannot get the hit log for a hit (admin only)
    [Fact]
    public async Task GetHitLog_Guest_NotAllowed_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        jobManager.AddJob(mrJob);
        var botLogger = new BotLogger();
        botLogger.Log("Test message");
        botLogger.Log("Test message 2");
        var hit = new Hit
        {
            BotLogger = botLogger,
        };
        mrJob.Hits.Add(hit);
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            jobId = mrJob.Id,
            hitId = hit.Id
        };
        var response = await GetJsonAsync<MrjHitLogDto>(
            client, "/api/v1/job/multi-run/hit-log".ToUri(queryParams));
        
        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.Error);
        Assert.NotNull(response.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, response.Error.Content.ErrorCode);
    }
    
    // TODO: Test all actions for various job states
    
    // Admin can change the number of bots in a job
    [Fact]
    public async Task ChangeBots_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        mrJob.Bots = 10;
        jobManager.AddJob(mrJob);
        
        // Act
        var dto = new ChangeBotsDto
        {
            JobId = mrJob.Id,
            Bots = 5
        };
        var error = await PostAsync(
            client, "/api/v1/job/change-bots", dto);
        
        // Assert
        Assert.Null(error);
        Assert.Equal(5, mrJob.Bots);
    }
    
    // Guest cannot change the number of bots in a job not owned by them
    [Fact]
    public async Task ChangeBots_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        mrJob.Bots = 10;
        jobManager.AddJob(mrJob);
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var dto = new ChangeBotsDto
        {
            JobId = mrJob.Id,
            Bots = 5
        };
        var error = await PostAsync(
            client, "/api/v1/job/change-bots", dto);
        
        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.BadRequest, error.Response.StatusCode);
        Assert.Equal(ErrorCode.JobNotFound, error.Content!.ErrorCode);
        Assert.Equal(10, mrJob.Bots);
    }
    
    // Guest can change the number of bots in their job
    [Fact]
    public async Task ChangeBots_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobManager = GetRequiredService<JobManagerService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Id = 1, Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var mrJob = CreateMultiRunJob();
        mrJob.Name = "Test MRJ";
        mrJob.Id = 1;
        mrJob.OwnerId = guest.Id;
        mrJob.Bots = 10;
        jobManager.AddJob(mrJob);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var dto = new ChangeBotsDto
        {
            JobId = mrJob.Id,
            Bots = 5
        };
        var error = await PostAsync(
            client, "/api/v1/job/change-bots", dto);
        
        // Assert
        Assert.Null(error);
        Assert.Equal(5, mrJob.Bots);
    }
    
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
    
    private JobEntity CreateMultiRunJobEntity(Job job, MultiRunJobOptions? options = null)
    {
        // We need to use Newtonsoft.Json here for the TypeNameHandling
        var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
        };
        
        var wrapper = new JobOptionsWrapper
        {
            Options = options ?? new MultiRunJobOptions()
        };
        
        return new JobEntity
        {
            CreationDate = DateTime.Now,
            JobType = job switch
            {
                MultiRunJob _ => JobType.MultiRun,
                ProxyCheckJob _ => JobType.ProxyCheck,
                _ => throw new NotImplementedException()    
            },
            JobOptions = Newtonsoft.Json.JsonConvert.SerializeObject(wrapper, jsonSettings)
        };
    }
    
    private JobEntity CreateProxyCheckJobEntity(Job job,
        ProxyCheckJobOptions? options = null, GuestEntity? owner = null)
    {
        // We need to use Newtonsoft.Json here for the TypeNameHandling
        var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
        };
        
        var wrapper = new JobOptionsWrapper
        {
            Options = options ?? new ProxyCheckJobOptions()
        };
        
        return new JobEntity
        {
            CreationDate = DateTime.Now,
            JobType = job switch
            {
                MultiRunJob _ => JobType.MultiRun,
                ProxyCheckJob _ => JobType.ProxyCheck,
                _ => throw new NotImplementedException()    
            },
            JobOptions = Newtonsoft.Json.JsonConvert.SerializeObject(wrapper, jsonSettings),
            Owner = owner
        };
    }
}
