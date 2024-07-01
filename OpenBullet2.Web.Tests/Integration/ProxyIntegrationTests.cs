using System.Net;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Proxy;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Models.Pagination;
using OpenBullet2.Web.Tests.Extensions;
using RuriLib.Models.Proxies;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class ProxyIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    /// <summary>
    /// The admin can list all proxies, including those in
    /// proxy groups owned by guests.
    /// </summary>
    [Fact]
    public async Task GetAll_Admin_ListAll()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var adminGroup = new ProxyGroupEntity { Name = "adminGroup" };
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var guestGroup = new ProxyGroupEntity { Name = "guestGroup", Owner = guest };
        dbContext.ProxyGroups.AddRange(adminGroup, guestGroup);
        var adminProxies = Enumerable.Range(8080, 1000)
            .Select(p => new ProxyEntity {
                Host = "127.0.0.1",
                Port = p,
                Group = adminGroup
            });
        var guestProxies = Enumerable.Range(9080, 500)
            .Select(p => new ProxyEntity 
            {
                Host = "127.0.0.1", 
                Port = p, 
                Group = guestGroup
            });
        dbContext.Proxies.AddRange(adminProxies);
        dbContext.Proxies.AddRange(guestProxies);
        await dbContext.SaveChangesAsync();
        
        // Act
        var filters = new ProxyFiltersDto
        {
            PageNumber = 0,
            PageSize = 25,
            ProxyGroupId = -1,
            SearchTerm = null,
            Type = null,
            Status = null
        };
        var result = await GetJsonAsync<PagedList<ProxyDto>>(
            client, "/api/v1/proxy/all".ToUri(filters));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value.Items.Count);
        Assert.Equal(1500, result.Value.TotalCount);
        Assert.Equal(0, result.Value.PageNumber);
        Assert.Equal(25, result.Value.PageSize);
    }
    
    /// <summary>
    /// A guest can list all proxies in their own proxy groups.
    /// </summary>
    [Fact]
    public async Task GetAll_Guest_OnlyOwn()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var guest2 = new GuestEntity { Username = "guest2" };
        dbContext.Guests.AddRange(guest, guest2);
        var adminGroup = new ProxyGroupEntity { Name = "adminGroup" };
        var guestGroup = new ProxyGroupEntity { Name = "guestGroup", Owner = guest };
        var guest2Group = new ProxyGroupEntity { Name = "guest2Group", Owner = guest2 };
        dbContext.ProxyGroups.AddRange(adminGroup, guestGroup, guest2Group);
        var adminProxies = Enumerable.Range(8080, 1000)
            .Select(p => new ProxyEntity {
                Host = "127.0.0.1",
                Port = p,
                Group = adminGroup
            });
        var guestProxies = Enumerable.Range(9080, 2000)
            .Select(p => new ProxyEntity 
            {
                Host = "127.0.0.1",
                Port = p, 
                Group = guestGroup
            });
        var guest2Proxies = Enumerable.Range(11080, 500)
            .Select(p => new ProxyEntity 
            {
                Host = "127.0.0.1", 
                Port = p, 
                Group = guest2Group
            });
        dbContext.Proxies.AddRange(adminProxies);
        dbContext.Proxies.AddRange(guestProxies);
        dbContext.Proxies.AddRange(guest2Proxies);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var filters = new ProxyFiltersDto
        {
            PageNumber = 0,
            PageSize = 25,
            ProxyGroupId = -1,
            SearchTerm = null,
            Type = null,
            Status = null
        };
        var result = await GetJsonAsync<PagedList<ProxyDto>>(
            client, "/api/v1/proxy/all".ToUri(filters));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value.Items.Count);
        Assert.Equal(2000, result.Value.TotalCount);
        Assert.Equal(0, result.Value.PageNumber);
        Assert.Equal(25, result.Value.PageSize);
    }
    
    [Fact]
    public async Task GetAll_Filtered_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var group = new ProxyGroupEntity { Name = "group" };
        var group2 = new ProxyGroupEntity { Name = "group2", Owner = guest };
        dbContext.ProxyGroups.AddRange(group, group2);
        var proxies = Enumerable.Range(8080, 1000)
            .Select(p => new ProxyEntity {
                Host = p % 2 == 0 ? "127.0.0.1" : "localhost",
                Port = p,
                Type = p / 4 % 2 == 0 ? ProxyType.Http : ProxyType.Socks5,
                Status = p / 2 % 2 == 0 ? ProxyWorkingStatus.Working : ProxyWorkingStatus.NotWorking,
                Group = group
            });
        var proxies2 = Enumerable.Range(9080, 500)
            .Select(p => new ProxyEntity {
                Host = "1.1.1.1",
                Port = p,
                Group = group2
            });
        dbContext.Proxies.AddRange(proxies);
        dbContext.Proxies.AddRange(proxies2);
        await dbContext.SaveChangesAsync();
        
        // Act
        var filters = new ProxyFiltersDto
        {
            PageNumber = 0,
            PageSize = 25,
            ProxyGroupId = group.Id,
            SearchTerm = "loc",
            Type = ProxyType.Socks5,
            Status = ProxyWorkingStatus.NotWorking
        };
        var result = await GetJsonAsync<PagedList<ProxyDto>>(
            client, "/api/v1/proxy/all".ToUri(filters));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value.Items.Count);
        Assert.Equal(1000 / 8, result.Value.TotalCount);
        Assert.Equal(0, result.Value.PageNumber);
        Assert.Equal(25, result.Value.PageSize);
    }
    
    [Fact]
    public async Task Add_FromList_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var proxyRepo = GetRequiredService<IProxyRepository>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = new ProxyGroupEntity { Name = "group" };
        dbContext.ProxyGroups.Add(group);
        await dbContext.SaveChangesAsync();
        
        // Act
        var dto = new AddProxiesFromListDto 
        {
            Proxies = Enumerable.Range(8080, 1000)
                .Select(p => $"127.0.0.1:{p}")
                .Concat(["(Http)localhost:6001:user:pass"]),
            DefaultType = ProxyType.Socks5,
            DefaultUsername = "username",
            DefaultPassword = "password",
            ProxyGroupId = group.Id
        };
        var result = await PostJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/proxy/add", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1001, result.Value.Count);
        
        var proxies = await proxyRepo.GetAll().ToListAsync();
        
        Assert.Equal(1001, proxies.Count);
        
        var first = proxies[0];
        Assert.Equal(ProxyType.Socks5, first.Type);
        Assert.Equal("username", first.Username);
        Assert.Equal("password", first.Password);
        
        // Check that the last proxy was added with the correct type and credentials
        // and was not overwritten by the default values
        var localhost = proxies.First(p => p.Host == "localhost");
        Assert.Equal(ProxyType.Http, localhost.Type);
        Assert.Equal("user", localhost.Username);
        Assert.Equal("pass", localhost.Password);
    }
    
    [Fact]
    public async Task Add_FromRemote_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var proxyRepo = GetRequiredService<IProxyRepository>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = new ProxyGroupEntity { Name = "group" };
        dbContext.ProxyGroups.Add(group);
        await dbContext.SaveChangesAsync();
        
        // Act
        var dto = new AddProxiesFromRemoteDto 
        {
            Url = "https://api.proxyscrape.com/v2/?request=displayproxies&protocol=http&timeout=10000&country=all&ssl=all&anonymity=all",
            ProxyGroupId = group.Id
        };
        var result = await PostJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/proxy/add-from-remote", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Count > 0);
        
        var proxies = await proxyRepo.GetAll().ToListAsync();
        
        Assert.Equal(result.Value.Count, proxies.Count);
    }
    
    [Fact]
    public async Task MoveMany_Filtered_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = new ProxyGroupEntity { Name = "group" };
        var group2 = new ProxyGroupEntity { Name = "group2" };
        dbContext.ProxyGroups.AddRange(group, group2);
        var proxies = Enumerable.Range(8080, 1000)
            .Select(p => new ProxyEntity {
                Host = p % 2 == 0 ? "127.0.0.1" : "localhost",
                Port = p,
                Type = p / 4 % 2 == 0 ? ProxyType.Http : ProxyType.Socks5,
                Status = p / 2 % 2 == 0 ? ProxyWorkingStatus.Working : ProxyWorkingStatus.NotWorking,
                Group = group
            });
        var proxies2 = Enumerable.Range(9080, 500)
            .Select(p => new ProxyEntity {
                Host = "1.1.1.1",
                Port = p,
                Group = group2
            });
        dbContext.Proxies.AddRange(proxies);
        dbContext.Proxies.AddRange(proxies2);
        await dbContext.SaveChangesAsync();
        
        // Act
        var dto = new MoveProxiesDto
        {
            PageNumber = 0,
            PageSize = 25,
            ProxyGroupId = group.Id,
            SearchTerm = "loc",
            Type = ProxyType.Socks5,
            Status = ProxyWorkingStatus.NotWorking,
            DestinationGroupId = group2.Id
        };
        var result = await PostJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/proxy/move/many", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1000 / 8, result.Value.Count);
        
        var group1ProxyCount = await dbContext.Proxies
            .Where(p => p.Group.Id == group.Id)
            .CountAsync();
        
        var group2ProxyCount = await dbContext.Proxies
            .Where(p => p.Group.Id == group2.Id)
            .CountAsync();
        
        Assert.Equal(1000 - 1000 / 8, group1ProxyCount);
        Assert.Equal(500 + 1000 / 8, group2ProxyCount);
    }
    
    /// <summary>
    /// A guest should now be allowed to move proxies from
    /// an admin-owned group to a self-owned group.
    /// </summary>
    [Fact]
    public async Task MoveMany_Guest_FromNotOwned_ToOwned_NothingMoved()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var adminGroup = new ProxyGroupEntity { Name = "adminGroup" };
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var guestGroup = new ProxyGroupEntity { Name = "guestGroup", Owner = guest };
        dbContext.ProxyGroups.AddRange(adminGroup, guestGroup);
        var proxies = Enumerable.Range(8080, 1000)
            .Select(p => new ProxyEntity {
                Host = "127.0.0.1",
                Port = p,
                Group = adminGroup
            });
        dbContext.Proxies.AddRange(proxies);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var dto = new MoveProxiesDto
        {
            PageNumber = 0,
            PageSize = 25,
            ProxyGroupId = adminGroup.Id,
            DestinationGroupId = guestGroup.Id
        };
        var result = await PostJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/proxy/move/many", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.Count);
    }
    
    [Fact]
    public async Task UpdateMany_Guest_FromOwned_ToNotOwned_Fail()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var adminGroup = new ProxyGroupEntity { Name = "adminGroup" };
        var guestGroup = new ProxyGroupEntity { Name = "guestGroup", Owner = guest };
        dbContext.ProxyGroups.AddRange(adminGroup, guestGroup);
        var proxies = Enumerable.Range(8080, 1000)
            .Select(p => new ProxyEntity
            {
                Host = "127.0.0.1", 
                Port = p, 
                Group = guestGroup
            });
        dbContext.Proxies.AddRange(proxies);
        await dbContext.SaveChangesAsync();

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var dto = new MoveProxiesDto 
        {
            PageNumber = 0,
            PageSize = 25,
            ProxyGroupId = guestGroup.Id,
            DestinationGroupId = adminGroup.Id
        };
        var result = await PostJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/proxy/move/many", dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.ProxyGroupNotFound, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task DownloadMany_Filtered_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = new ProxyGroupEntity { Name = "group" };
        var group2 = new ProxyGroupEntity { Name = "group2" };
        dbContext.ProxyGroups.AddRange(group, group2);
        var proxies = Enumerable.Range(8080, 1000)
            .Select(p => new ProxyEntity {
                Host = p % 2 == 0 ? "127.0.0.1" : "localhost",
                Port = p,
                Type = p / 4 % 2 == 0 ? ProxyType.Http : ProxyType.Socks5,
                Status = p / 2 % 2 == 0 ? ProxyWorkingStatus.Working : ProxyWorkingStatus.NotWorking,
                Group = group
            });
        var proxies2 = Enumerable.Range(9080, 500)
            .Select(p => new ProxyEntity {
                Host = "1.1.1.1",
                Port = p,
                Group = group2
            });
        dbContext.Proxies.AddRange(proxies);
        dbContext.Proxies.AddRange(proxies2);
        await dbContext.SaveChangesAsync();
        
        // Act
        var filters = new ProxyFiltersDto
        {
            PageNumber = 0,
            PageSize = 25,
            ProxyGroupId = group.Id,
            SearchTerm = "loc",
            Type = ProxyType.Socks5,
            Status = ProxyWorkingStatus.NotWorking
        };
        var response = await client.GetAsync(
            "/api/v1/proxy/download/many".ToUri(filters));
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(1000 / 8, lines.Length);
        Assert.All(lines, l => Assert.Contains("loc", l));
        Assert.All(lines, l => Assert.Contains("Socks5", l));
        Assert.Equal("text/plain", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("proxies.txt", response.Content.Headers.ContentDisposition!.FileName);
    }
    
    [Fact]
    public async Task DeleteMany_Filtered_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = new ProxyGroupEntity { Name = "group" };
        dbContext.ProxyGroups.Add(group);
        var proxies = Enumerable.Range(8080, 1000)
            .Select(p => new ProxyEntity {
                Host = p % 2 == 0 ? "127.0.0.1" : "localhost",
                Port = p,
                Type = p / 4 % 2 == 0 ? ProxyType.Http : ProxyType.Socks5,
                Status = p / 2 % 2 == 0 ? ProxyWorkingStatus.Working : ProxyWorkingStatus.NotWorking,
                Group = group
            });
        dbContext.Proxies.AddRange(proxies);
        await dbContext.SaveChangesAsync();
        
        // Act
        var filters = new ProxyFiltersDto
        {
            PageNumber = 0,
            PageSize = 25,
            ProxyGroupId = group.Id,
            SearchTerm = "loc",
            Type = ProxyType.Socks5,
            Status = ProxyWorkingStatus.NotWorking
        };
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/proxy/many".ToUri(filters));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1000 / 8, result.Value.Count);
        
        var remaining = await dbContext.Proxies
            .Where(p => p.Group.Id == group.Id)
            .CountAsync();
        
        Assert.Equal(1000 - 1000 / 8, remaining);
    }
    
    [Fact]
    public async Task DeleteSlow_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var group = new ProxyGroupEntity { Name = "group" };
        dbContext.ProxyGroups.Add(group);
        dbContext.Proxies.AddRange(
            new ProxyEntity { Ping = 1000, Group = group },
            new ProxyEntity { Ping = 2000, Group = group },
            new ProxyEntity { Ping = 3000, Group = group },
            new ProxyEntity { Ping = 4000, Group = group }
        );
        await dbContext.SaveChangesAsync();
        
        // Act
        var queryParams = new {
            proxyGroupId = group.Id,
            maxPing = 2000
        };
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/proxy/slow".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        
        var remaining = await dbContext.Proxies
            .Where(p => p.Group.Id == group.Id)
            .CountAsync();
        
        Assert.Equal(2, remaining);
    }
    
    [Fact]
    public async Task DeleteSlow_Guest_NotOwned_Fail()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var adminGroup = new ProxyGroupEntity { Name = "adminGroup" };
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var guestGroup = new ProxyGroupEntity { Name = "guestGroup", Owner = guest };
        dbContext.ProxyGroups.AddRange(adminGroup, guestGroup);
        dbContext.Proxies.AddRange(
            new ProxyEntity { Ping = 1000, Group = adminGroup },
            new ProxyEntity { Ping = 2000, Group = adminGroup },
            new ProxyEntity { Ping = 3000, Group = adminGroup },
            new ProxyEntity { Ping = 4000, Group = adminGroup }
        );
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new {
            proxyGroupId = adminGroup.Id,
            maxPing = 2000
        };
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/proxy/slow".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.ProxyGroupNotFound, result.Error.Content!.ErrorCode);
    }
}
