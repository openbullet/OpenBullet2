using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Info;
using OpenBullet2.Web.Interfaces;
using RuriLib.Models.Configs;
using RuriLib.Services;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class InfoIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task GetServerInfo_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var result = await GetJsonAsync<ServerInfoDto>(client, "/api/v1/info/server");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var info = result.Value;
        Assert.True(info.StartTime < DateTime.UtcNow);
        Assert.Equal(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now), info.LocalUtcOffset);
    }
    
    [Fact]
    public async Task GetAnnouncement_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var announcementService = GetRequiredService<IAnnouncementService>();
        var announcement = await announcementService.FetchAnnouncementAsync();
        
        // Act
        var result = await GetJsonAsync<AnnouncementDto>(client, "/api/v1/info/announcement");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(announcement, result.Value.MarkdownText);
    }
    
    [Fact]
    public async Task GetChangelog_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var updateService = GetRequiredService<IUpdateService>();
        
        // Act
        var result = await GetJsonAsync<ChangelogDto>(client, "/api/v1/info/changelog");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.MarkdownText);

        var version = new Version(result.Value.Version);
        
        // The version should be identical except for the revision
        Assert.Equal(updateService.CurrentVersion.Major, version.Major);
        Assert.Equal(updateService.CurrentVersion.Minor, version.Minor);
        Assert.Equal(updateService.CurrentVersion.Build, version.Build);
    }
    
    [Fact]
    public async Task GetUpdateInfo_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var updateService = GetRequiredService<IUpdateService>();
        var currentVersion = new Version(await File.ReadAllTextAsync("version.txt"));
        
        // Act
        var result = await GetJsonAsync<UpdateInfoDto>(client, "/api/v1/info/update");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(currentVersion.ToString(), result.Value.CurrentVersion);
        Assert.Equal(updateService.RemoteVersion.ToString(), result.Value.RemoteVersion);
    }
    
    [Fact]
    public async Task GetCollectionInfo_Admin_SeeEverything()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        
        // Create a job
        var jobManager = GetRequiredService<JobManagerService>();
        var jobFactory = GetRequiredService<JobFactoryService>();
        var job = jobFactory.FromOptions(1, 0, new ProxyCheckJobOptions {
            Target = new ProxyCheckTarget()
        });
        jobManager.AddJob(job);
        
        // Create a proxy group and add proxies to it
        var proxyGroup = new ProxyGroupEntity { Name = "Test" };
        dbContext.ProxyGroups.Add(proxyGroup);
        foreach (var port in Enumerable.Range(1000, 1000))
            dbContext.Proxies.Add(new ProxyEntity
            {
                Host = "127.0.0.1",
                Port = port,
                Group = proxyGroup
            });
        
        // Create two wordlists
        dbContext.Wordlists.Add(new WordlistEntity { Total = 50 });
        dbContext.Wordlists.Add(new WordlistEntity { Total = 100 });
        
        // Create 3 hits (SUCCESS, CUSTOM and NONE)
        dbContext.Hits.Add(new HitEntity { Type = "SUCCESS" });
        dbContext.Hits.Add(new HitEntity { Type = "CUSTOM" });
        dbContext.Hits.Add(new HitEntity { Type = "NONE" });
        
        // Create a config
        var configService = GetRequiredService<ConfigService>();
        configService.Configs.Add(new Config { Id = "test" });
        
        // Add a guest
        dbContext.Guests.Add(new GuestEntity());
        
        // Load a plugin
        var pluginRepo = GetRequiredService<PluginRepository>();
        var file = new FileInfo("Resources/OB2TestPlugin.zip");
        await using var fs = file.OpenRead();
        pluginRepo.AddPlugin(fs);
        
        await dbContext.SaveChangesAsync();
        
        // Act
        var result = await GetJsonAsync<CollectionInfoDto>(client, "/api/v1/info/collection");
        
        // Assert
        Assert.True(result.IsSuccess);
        var info = result.Value;
        Assert.Equal(1, info.JobsCount);
        Assert.Equal(1000, info.ProxiesCount);
        Assert.Equal(2, info.WordlistsCount);
        Assert.Equal(150, info.WordlistsLines);
        Assert.Equal(3, info.HitsCount);
        Assert.Equal(1, info.ConfigsCount);
        Assert.Equal(1, info.GuestsCount);
        Assert.Equal(1, info.PluginsCount);
    }

    [Fact]
    public async Task GetCollectionInfo_Guest_SeeOnlyOwn()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();

        // Create an admin job and a guest job
        var jobManager = GetRequiredService<JobManagerService>();
        var jobFactory = GetRequiredService<JobFactoryService>();
        var adminJob = jobFactory.FromOptions(1, 0, new ProxyCheckJobOptions {
            Target = new ProxyCheckTarget()
        });
        var guestJob = jobFactory.FromOptions(1, guest.Id, new ProxyCheckJobOptions {
            Target = new ProxyCheckTarget()
        });
        guestJob.OwnerId = guest.Id;
        jobManager.AddJob(adminJob);
        jobManager.AddJob(guestJob);
        
        // Create an admin proxy group and a guest proxy group and add proxies to them
        var adminProxyGroup = new ProxyGroupEntity { Name = "Test" };
        var guestProxyGroup = new ProxyGroupEntity { Name = "Test", Owner = guest };
        dbContext.ProxyGroups.AddRange(adminProxyGroup, guestProxyGroup);
        foreach (var port in Enumerable.Range(1000, 1000))
        {
            dbContext.Proxies.Add(new ProxyEntity
            {
                Host = "127.0.0.1",
                Port = port,
                Group = adminProxyGroup
            });
            dbContext.Proxies.Add(new ProxyEntity
            {
                Host = "127.0.0.1",
                Port = port,
                Group = guestProxyGroup
            });
        }

        // Create two wordlists (one owned by the guest)
        dbContext.Wordlists.Add(new WordlistEntity { Total = 50 });
        dbContext.Wordlists.Add(new WordlistEntity { Total = 100, Owner = guest });

        // Create 3 hits (SUCCESS, CUSTOM and NONE) (one owned by the guest)
        dbContext.Hits.Add(new HitEntity { Type = "SUCCESS", OwnerId = guest.Id });
        dbContext.Hits.Add(new HitEntity { Type = "CUSTOM" });
        dbContext.Hits.Add(new HitEntity { Type = "NONE" });

        // Create a config
        var configService = GetRequiredService<ConfigService>();
        configService.Configs.Add(new Config { Id = "test" });

        // Add another guest
        dbContext.Guests.Add(new GuestEntity());

        // Load a plugin
        var pluginRepo = GetRequiredService<PluginRepository>();
        var file = new FileInfo("Resources/OB2TestPlugin.zip");
        await using var fs = file.OpenRead();
        pluginRepo.AddPlugin(fs);

        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var result = await GetJsonAsync<CollectionInfoDto>(client, "/api/v1/info/collection");

        // Assert
        Assert.True(result.IsSuccess);
        var info = result.Value;
        Assert.Equal(1, info.JobsCount);
        Assert.Equal(1000, info.ProxiesCount);
        Assert.Equal(1, info.WordlistsCount);
        Assert.Equal(100, info.WordlistsLines);
        Assert.Equal(1, info.HitsCount);
        Assert.Equal(1, info.ConfigsCount);
        Assert.Equal(1, info.GuestsCount);
        Assert.Equal(0, info.PluginsCount);
    }
}
