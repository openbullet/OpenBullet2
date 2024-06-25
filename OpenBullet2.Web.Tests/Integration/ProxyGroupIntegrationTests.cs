using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.ProxyGroup;
using OpenBullet2.Web.Exceptions;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class ProxyGroupIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    /// <summary>
    /// The admin can list all proxy groups, including those owned by guests.
    /// </summary>
    [Fact]
    public async Task GetAllGroups_Admin_ListAll()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        dbContext.ProxyGroups.Add(new ProxyGroupEntity { Name = "group1", Owner = guest });
        dbContext.ProxyGroups.Add(new ProxyGroupEntity { Name = "group2" });
        await dbContext.SaveChangesAsync();
        
        // Act
        var result = await GetJsonAsync<IEnumerable<ProxyGroupDto>>(client, "/api/v1/proxy-group/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
    }
    
    /// <summary>
    /// A guest can only list their own proxy groups.
    /// </summary>
    [Fact]
    public async Task GetAllGroups_Guest_OnlyOwn()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var guest2 = new GuestEntity { Username = "guest2" };
        dbContext.Guests.AddRange(guest, guest2);
        dbContext.ProxyGroups.Add(new ProxyGroupEntity { Name = "group1", Owner = guest });
        dbContext.ProxyGroups.Add(new ProxyGroupEntity { Name = "group2" });
        dbContext.ProxyGroups.Add(new ProxyGroupEntity { Name = "group3", Owner = guest2 });
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<ProxyGroupDto>>(client, "/api/v1/proxy-group/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("group1", result.Value.First().Name);
    }
    
    /// <summary>
    /// An admin can create a new proxy group.
    /// </summary>
    [Fact]
    public async Task CreateProxyGroup_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dto = new CreateProxyGroupDto { Name = "group1" };
        
        // Act
        var result = await PostJsonAsync<ProxyGroupDto>(client, "/api/v1/proxy-group", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = await dbContext.ProxyGroups.FindAsync(result.Value.Id);
        Assert.NotNull(group);
        Assert.Equal("group1", group.Name);
    }
    
    /// <summary>
    /// A guest can create a new proxy group, and they will be the owner.
    /// </summary>
    [Fact]
    public async Task CreateProxyGroup_Guest_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var proxyGroupRepo = GetRequiredService<IProxyGroupRepository>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        var dto = new CreateProxyGroupDto { Name = "group1" };
        
        // Act
        var result = await PostJsonAsync<ProxyGroupDto>(client, "/api/v1/proxy-group", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        var group = await proxyGroupRepo.GetAsync(result.Value.Id);
        Assert.NotNull(group);
        Assert.Equal("group1", group.Name);
        Assert.NotNull(group.Owner);
        Assert.Equal(guest.Id, group.Owner.Id);
    }
    
    [Fact]
    public async Task UpdateProxyGroup_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = new ProxyGroupEntity { Name = "group1" };
        dbContext.ProxyGroups.Add(group);
        await dbContext.SaveChangesAsync();
        
        var dto = new UpdateProxyGroupDto { Id = group.Id, Name = "group2" };
        
        // Act
        var result = await PutJsonAsync<ProxyGroupDto>(client, "/api/v1/proxy-group", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        await dbContext.Entry(group).ReloadAsync();
        Assert.Equal("group2", group.Name);
    }
    
    [Fact]
    public async Task UpdateProxyGroup_Guest_OwnGroup_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var group = new ProxyGroupEntity { Name = "group1", Owner = guest };
        dbContext.ProxyGroups.Add(group);
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        var dto = new UpdateProxyGroupDto { Id = group.Id, Name = "group2" };
        
        // Act
        var result = await PutJsonAsync<ProxyGroupDto>(client, "/api/v1/proxy-group", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        await dbContext.Entry(group).ReloadAsync();
        Assert.Equal("group2", group.Name);
    }
    
    [Fact]
    public async Task UpdateProxyGroup_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var group = new ProxyGroupEntity { Name = "group1" };
        dbContext.ProxyGroups.Add(group);
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        var dto = new UpdateProxyGroupDto { Id = group.Id, Name = "group2" };
        
        // Act
        var result = await PutJsonAsync<ProxyGroupDto>(client, "/api/v1/proxy-group", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.ProxyGroupNotFound, result.Error.Content.ErrorCode);
    }
    
    [Fact]
    public async Task DeleteProxyGroup_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var proxyGroupRepo = GetRequiredService<IProxyGroupRepository>();
        var proxyRepo = GetRequiredService<IProxyRepository>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = new ProxyGroupEntity { Name = "group1" };
        var proxy1 = new ProxyEntity { Host = "host1", Port = 80, Group = group };
        var proxy2 = new ProxyEntity { Host = "host2", Port = 80, Group = group };
        dbContext.ProxyGroups.Add(group);
        dbContext.Proxies.Add(proxy1);
        dbContext.Proxies.Add(proxy2);
        await dbContext.SaveChangesAsync();
        
        // Act
        var result = await DeleteAsync(client, $"/api/v1/proxy-group?id={group.Id}");
        
        // Assert
        Assert.Null(result);
        group = await proxyGroupRepo.GetAsync(group.Id);
        Assert.Null(group);
        
        // Proxies should be deleted as well
        proxy1 = await proxyRepo.GetAsync(proxy1.Id);
        Assert.Null(proxy1);
        proxy2 = await proxyRepo.GetAsync(proxy2.Id);
        Assert.Null(proxy2);
    }
    
    [Fact]
    public async Task DeleteProxyGroup_Guest_OwnGroup_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var proxyGroupRepo = GetRequiredService<IProxyGroupRepository>();
        var proxyRepo = GetRequiredService<IProxyRepository>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var group = new ProxyGroupEntity { Name = "group1", Owner = guest };
        var proxy1 = new ProxyEntity { Host = "host1", Port = 80, Group = group };
        var proxy2 = new ProxyEntity { Host = "host2", Port = 80, Group = group };
        dbContext.ProxyGroups.Add(group);
        dbContext.Proxies.Add(proxy1);
        dbContext.Proxies.Add(proxy2);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await DeleteAsync(client, $"/api/v1/proxy-group?id={group.Id}");
        
        // Assert
        Assert.Null(result);
        group = await proxyGroupRepo.GetAsync(group.Id);
        Assert.Null(group);
        
        // Proxies should be deleted as well
        proxy1 = await proxyRepo.GetAsync(proxy1.Id);
        Assert.Null(proxy1);
        proxy2 = await proxyRepo.GetAsync(proxy2.Id);
        Assert.Null(proxy2);
    }
    
    [Fact]
    public async Task DeleteProxyGroup_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var group = new ProxyGroupEntity { Name = "group1" };
        dbContext.ProxyGroups.Add(group);
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await DeleteAsync(client, $"/api/v1/proxy-group?id={group.Id}");
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.Equal(ErrorCode.ProxyGroupNotFound, result.Content.ErrorCode);
    }
    
    [Fact]
    public async Task DeleteProxyGroup_InUse_ByProxyCheckJob_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = new ProxyGroupEntity { Name = "group1" };
        var proxy = new ProxyEntity { Host = "host", Port = 80, Group = group };
        dbContext.ProxyGroups.Add(group);
        dbContext.Proxies.Add(proxy);
        await dbContext.SaveChangesAsync();
        
        var jobManager = GetRequiredService<JobManagerService>();
        var jobFactory = GetRequiredService<JobFactoryService>();
        var job = jobFactory.FromOptions(1, 0, new ProxyCheckJobOptions {
            GroupId = group.Id,
            Target = new ProxyCheckTarget()
        });
        jobManager.AddJob(job);
        
        // Act
        var result = await DeleteAsync(client, $"/api/v1/proxy-group?id={group.Id}");
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.Equal(ErrorCode.ProxyGroupInUse, result.Content.ErrorCode);
    }
    
    [Fact]
    public async Task DeleteProxyGroup_InUse_ByMultiRunJob_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = new ProxyGroupEntity { Name = "group1" };
        var proxy = new ProxyEntity { Host = "host", Port = 80, Group = group };
        dbContext.ProxyGroups.Add(group);
        dbContext.Proxies.Add(proxy);
        await dbContext.SaveChangesAsync();
        
        var jobManager = GetRequiredService<JobManagerService>();
        var jobFactory = GetRequiredService<JobFactoryService>();
        var job = jobFactory.FromOptions(1, 0, new MultiRunJobOptions {
            ProxySources = [new GroupProxySourceOptions { GroupId = group.Id }]
        });
        jobManager.AddJob(job);
        
        // Act
        var result = await DeleteAsync(client, $"/api/v1/proxy-group?id={group.Id}");
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.Equal(ErrorCode.ProxyGroupInUse, result.Content.ErrorCode);
    }
}
