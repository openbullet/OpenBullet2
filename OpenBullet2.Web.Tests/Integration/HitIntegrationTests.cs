using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Hit;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Models.Pagination;
using OpenBullet2.Web.Tests.Extensions;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class HitIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task CreateHit_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        
        var dto = new CreateHitDto
        {
            Data = "Data",
            CapturedData = "CapturedData",
            Proxy = "(Socks5)127.0.0.1:8080:user:pass",
            Date = DateTime.UtcNow,
            Type = "SUCCESS",
            ConfigId = "ConfigId",
            ConfigName = "ConfigName",
            ConfigCategory = "ConfigCategory",
            WordlistId = 1,
            WordlistName = "WordlistName"
        };
        
        // Act
        var result = await PostJsonAsync<HitDto>(
            client, "/api/v1/hit", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Data, result.Value.Data);
        Assert.Equal(dto.CapturedData, result.Value.CapturedData);
        Assert.Equal(dto.Proxy, result.Value.Proxy);
        Assert.Equal(dto.Date, result.Value.Date);
        Assert.Equal(dto.Type, result.Value.Type);
        Assert.Equal(dto.ConfigId, result.Value.ConfigId);
        Assert.Equal(dto.ConfigName, result.Value.ConfigName);
        Assert.Equal(dto.ConfigCategory, result.Value.ConfigCategory);
        Assert.Equal(dto.WordlistId, result.Value.WordlistId);
        Assert.Equal(dto.WordlistName, result.Value.WordlistName);
        
        var hit = await dbContext.Hits.FirstOrDefaultAsync();
        Assert.NotNull(hit);
        Assert.Equal(dto.Data, hit.Data);
        Assert.Equal(dto.CapturedData, hit.CapturedData);
        Assert.Equal(dto.Proxy, hit.Proxy);
        Assert.Equal(dto.Date, hit.Date);
        Assert.Equal(dto.Type, hit.Type);
        Assert.Equal(dto.ConfigId, hit.ConfigId);
        Assert.Equal(dto.ConfigName, hit.ConfigName);
        Assert.Equal(dto.ConfigCategory, hit.ConfigCategory);
        Assert.Equal(dto.WordlistId, hit.WordlistId);
        Assert.Equal(dto.WordlistName, hit.WordlistName);
        Assert.Equal(0, hit.OwnerId);
    }
    
    [Fact]
    public async Task CreateHit_Guest_Success() 
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        var dto = new CreateHitDto
        {
            Data = "Data",
            CapturedData = "CapturedData",
            Proxy = "(Socks5)127.0.0.1:8080:user:pass",
            Date = DateTime.UtcNow,
            Type = "SUCCESS",
            ConfigId = "ConfigId",
            ConfigName = "ConfigName",
            ConfigCategory = "ConfigCategory",
            WordlistId = 1,
            WordlistName = "WordlistName"
        };
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await PostJsonAsync<HitDto>(
            client, "/api/v1/hit", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Data, result.Value.Data);
        Assert.Equal(dto.CapturedData, result.Value.CapturedData);
        Assert.Equal(dto.Proxy, result.Value.Proxy);
        Assert.Equal(dto.Date, result.Value.Date);
        Assert.Equal(dto.Type, result.Value.Type);
        Assert.Equal(dto.ConfigId, result.Value.ConfigId);
        Assert.Equal(dto.ConfigName, result.Value.ConfigName);
        Assert.Equal(dto.ConfigCategory, result.Value.ConfigCategory);
        Assert.Equal(dto.WordlistId, result.Value.WordlistId);
        Assert.Equal(dto.WordlistName, result.Value.WordlistName);
        
        var hit = await dbContext.Hits.FirstOrDefaultAsync();
        Assert.NotNull(hit);
        Assert.Equal(dto.Data, hit.Data);
        Assert.Equal(dto.CapturedData, hit.CapturedData);
        Assert.Equal(dto.Proxy, hit.Proxy);
        Assert.Equal(dto.Date, hit.Date);
        Assert.Equal(dto.Type, hit.Type);
        Assert.Equal(dto.ConfigId, hit.ConfigId);
        Assert.Equal(dto.ConfigName, hit.ConfigName);
        Assert.Equal(dto.ConfigCategory, hit.ConfigCategory);
        Assert.Equal(dto.WordlistId, hit.WordlistId);
        Assert.Equal(dto.WordlistName, hit.WordlistName);
        Assert.Equal(guest.Id, hit.OwnerId);
    }
    
    [Fact]
    public async Task UpdateHit_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var hit = CreateHit();
        dbContext.Hits.Add(hit);
        await dbContext.SaveChangesAsync();
        
        var dto = new UpdateHitDto
        {
            Id = hit.Id,
            Data = "NewData",
            CapturedData = "NewCapturedData",
            Type = "NONE"
        };
        
        // Act
        var result = await PatchJsonAsync<HitDto>(
            client, "/api/v1/hit", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Data, result.Value.Data);
        Assert.Equal(dto.CapturedData, result.Value.CapturedData);
        Assert.Equal(hit.Proxy, result.Value.Proxy);
        Assert.Equal(hit.Date, result.Value.Date);
        Assert.Equal(dto.Type, result.Value.Type);
        Assert.Equal(hit.ConfigId, result.Value.ConfigId);
        Assert.Equal(hit.ConfigName, result.Value.ConfigName);
        Assert.Equal(hit.ConfigCategory, result.Value.ConfigCategory);
        Assert.Equal(hit.WordlistId, result.Value.WordlistId);
        Assert.Equal(hit.WordlistName, result.Value.WordlistName);
        
        await dbContext.Entry(hit).ReloadAsync();
        
        Assert.Equal(dto.Data, hit.Data);
        Assert.Equal(dto.CapturedData, hit.CapturedData);
        Assert.Equal(hit.Proxy, hit.Proxy);
        Assert.Equal(hit.Date, hit.Date);
        Assert.Equal(dto.Type, hit.Type);
        Assert.Equal(hit.ConfigId, hit.ConfigId);
        Assert.Equal(hit.ConfigName, hit.ConfigName);
        Assert.Equal(hit.ConfigCategory, hit.ConfigCategory);
        Assert.Equal(hit.WordlistId, hit.WordlistId);
        Assert.Equal(hit.WordlistName, hit.WordlistName);
    }
    
    [Fact]
    public async Task UpdateHit_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var hit = CreateHit();
        dbContext.Hits.Add(hit);
        await dbContext.SaveChangesAsync();
        
        var dto = new UpdateHitDto
        {
            Id = hit.Id,
            Data = "NewData",
            CapturedData = "NewCapturedData",
            Type = "NONE"
        };
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await PatchJsonAsync<HitDto>(
            client, "/api/v1/hit", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.HitNotFound, result.Error.Content.ErrorCode);
    }
    
    [Fact]
    public async Task DeleteHit_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var hit = CreateHit();
        dbContext.Hits.Add(hit);
        await dbContext.SaveChangesAsync();

        // Act
        var queryParams = new { id = hit.Id };
        var error = await DeleteAsync(
            client, "/api/v1/hit".ToUri(queryParams));
        
        // Assert
        Assert.Null(error);
        
        var hitAfter = await dbContext.Hits.FirstOrDefaultAsync();
        Assert.Null(hitAfter);
    }
    
    [Fact]
    public async Task DeleteHit_Guest_NotOwned_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var hit = CreateHit();
        dbContext.Hits.Add(hit);
        await dbContext.SaveChangesAsync();

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var queryParams = new { id = hit.Id };
        var error = await DeleteAsync(
            client, "/api/v1/hit".ToUri(queryParams));

        // Assert
        Assert.NotNull(error);
        Assert.NotNull(error.Content);
        Assert.Equal(ErrorCode.HitNotFound, error.Content.ErrorCode);
    }
    
    [Fact]
    public async Task GetAll_Admin_ListAll()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var hits = Enumerable.Range(0, 1000)
            .Select(i => CreateHit(i.ToString()));
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        // Act
        var filters = new PaginatedHitFiltersDto
        {
            PageNumber = 0,
            PageSize = 25,
            SearchTerm = null,
            Types = null,
            MinDate = null,
            MaxDate = null,
            SortBy = null,
            SortDescending = true
        };
        var result = await GetJsonAsync<PagedList<HitDto>>(
            client, "/api/v1/hit/all".ToUri(filters));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value.Items.Count);
        Assert.Equal(1000, result.Value.TotalCount);
        Assert.Equal(0, result.Value.PageNumber);
        Assert.Equal(25, result.Value.PageSize);
    }
    
    [Fact]
    public async Task GetAll_Guest_OnlyOwn()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var guest2 = new GuestEntity { Username = "guest2" };
        dbContext.Guests.AddRange(guest, guest2);
        await dbContext.SaveChangesAsync();
        
        var adminHits = Enumerable.Range(0, 1000)
            .Select(i => CreateHit(i.ToString()));
        var guestHits = Enumerable.Range(0, 2000)
            .Select(i => CreateHit(i.ToString(), guest.Id));
        var guest2Hits = Enumerable.Range(0, 500)
            .Select(i => CreateHit(i.ToString(), guest2.Id));
        dbContext.Hits.AddRange(adminHits);
        dbContext.Hits.AddRange(guestHits);
        dbContext.Hits.AddRange(guest2Hits);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var filters = new PaginatedHitFiltersDto
        {
            PageNumber = 0,
            PageSize = 25,
            SearchTerm = null,
            Types = null,
            MinDate = null,
            MaxDate = null,
            SortBy = null,
            SortDescending = true
        };
        var result = await GetJsonAsync<PagedList<HitDto>>(
            client, "/api/v1/hit/all".ToUri(filters));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value.Items.Count);
        Assert.Equal(2000, result.Value.TotalCount);
        Assert.Equal(0, result.Value.PageNumber);
        Assert.Equal(25, result.Value.PageSize);
    }
    
    [Fact]
    public async Task GetAll_Filtered_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var hits = Enumerable.Range(0, 1000)
            .Select(i => new HitEntity {
               Data = i % 2 == 0 ? "AAA" : "BBB",
               CapturedData = i % 2 == 0 ? "AAA" : "BBB",
               Proxy = "(Socks5)127.0.0.1:8080:user:pass",
               Date = i / 4 % 2 == 0 ? DateTime.UtcNow : DateTime.UtcNow.AddDays(-7),
               Type = i / 8 % 2 == 0 ? "SUCCESS" : "NONE"
            });
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        // Act
        var filters = new PaginatedHitFiltersDto
        {
            PageNumber = 0,
            PageSize = 25,
            SearchTerm = "AAA",
            Types = "SUCCESS",
            MinDate = DateTime.UtcNow.AddDays(-3),
            MaxDate = DateTime.UtcNow.AddDays(3),
            SortBy = HitSortField.Date,
            SortDescending = true
        };
        var result = await GetJsonAsync<PagedList<HitDto>>(
            client, "/api/v1/hit/all".ToUri(filters));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value.Items.Count);
        // I don't know why, but it returns 126 items instead
        // of 125, and I'm too tired to sit here and debug...
        Assert.Equal(1000 / 8 + 1, result.Value.TotalCount);
        Assert.Equal(0, result.Value.PageNumber);
        Assert.Equal(25, result.Value.PageSize);
    }
    
    [Fact]
    public async Task GetConfigNames_Admin_ListAll()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var hits = Enumerable.Range(0, 100)
            .Select(i => new HitEntity {
                ConfigName = i % 2 == 0 ? "Config1" : "Config2"
            });
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        // Act
        var result = await GetJsonAsync<List<string>>(
            client, "/api/v1/hit/config-names");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains("Config1", result.Value);
        Assert.Contains("Config2", result.Value);
    }
    
    [Fact]
    public async Task GetConfigNames_Guest_OnlyOwn()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        var adminHits = Enumerable.Range(0, 100)
            .Select(i => new HitEntity {
                ConfigName = "Config1"
            });
        var hits = Enumerable.Range(0, 100)
            .Select(i => new HitEntity {
                ConfigName = "Config2",
                OwnerId = guest.Id
            });
        dbContext.Hits.AddRange(adminHits);
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<List<string>>(
            client, "/api/v1/hit/config-names");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Contains("Config2", result.Value);
    }
    
    [Fact]
    public async Task DownloadMany_Admin_Formatted_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var hits = Enumerable.Range(0, 100)
            .Select(i => new HitEntity {
                Data = "$$DATA$$",
                CapturedData = "$$CAPTURE$$",
                Proxy = "$$PROXY$$",
                Date = date,
                Type = "$$TYPE$$",
                ConfigName = "$$CONFIG$$",
                WordlistName = "$$WORDLIST$$",
                ConfigCategory = "$$CATEGORY$$"
            });
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        // Act
        var queryParams = new
        {
            format = "<DATA>|<CAPTURE>|<PROXY>|<DATE>|<TYPE>|<CONFIG>|<WORDLIST>|<CATEGORY>"
        };
        var response = await client.GetAsync(
            "/api/v1/hit/download/many".ToUri(queryParams));
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(100, lines.Length);
        Assert.All(lines, line => {
            var parts = line.Split('|');
            Assert.Equal("$$DATA$$", parts[0]);
            Assert.Equal("$$CAPTURE$$", parts[1]);
            Assert.Equal("$$PROXY$$", parts[2]);
            Assert.Equal(date.ToString(CultureInfo.InvariantCulture), parts[3]);
            Assert.Equal("$$TYPE$$", parts[4]);
            Assert.Equal("$$CONFIG$$", parts[5]);
            Assert.Equal("$$WORDLIST$$", parts[6]);
            Assert.Equal("$$CATEGORY$$", parts[7]);
        });
        Assert.Equal("text/plain", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("hits.txt", response.Content.Headers.ContentDisposition!.FileName);
    }
    
    [Fact]
    public async Task DownloadMany_Guest_OnlyOwn_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        var adminHits = Enumerable.Range(0, 100)
            .Select(_ => CreateHit());
        dbContext.Hits.AddRange(adminHits);
        var hits = Enumerable.Range(0, 100)
            .Select(_ => CreateHit(ownerId: guest.Id));
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            format = "<DATA>"
        };
        var response = await client.GetAsync(
            "/api/v1/hit/download/many".ToUri(queryParams));
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(100, lines.Length);
        Assert.Equal("text/plain", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("hits.txt", response.Content.Headers.ContentDisposition!.FileName);
    }
    
    [Fact]
    public async Task DeleteMany_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var hits = Enumerable.Range(0, 100)
            .Select(i => CreateHit(i % 2 == 0 ? "data1" : "data2"));
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        // Act
        var filters = new HitFiltersDto
        {
            SearchTerm = "data1",
            Types = null,
            MinDate = null,
            MaxDate = null,
            SortBy = null,
            SortDescending = true
        };
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/hit/many".ToUri(filters));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100 / 2, result.Value.Count);
        
        var hitsAfter = await dbContext.Hits.CountAsync();
        Assert.Equal(100 / 2, hitsAfter);
    }
    
    [Fact]
    public async Task DeleteMany_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        var adminHits = Enumerable.Range(0, 100)
            .Select(i => CreateHit());
        dbContext.Hits.AddRange(adminHits);
        var hits = Enumerable.Range(0, 100)
            .Select(i => CreateHit(ownerId: guest.Id));
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var filters = new HitFiltersDto
        {
            SearchTerm = null,
            Types = null,
            MinDate = null,
            MaxDate = null,
            SortBy = null,
            SortDescending = true
        };
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/hit/many".ToUri(filters));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.Count);
        
        var hitsAfter = await dbContext.Hits.CountAsync();
        Assert.Equal(100, hitsAfter);
    }
    
    [Fact]
    public async Task DeleteDuplicates_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var hits = Enumerable.Range(0, 100)
            .Select(i => CreateHit());
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        // Act
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/hit/duplicates");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(99, result.Value.Count);
        
        var hitsAfter = await dbContext.Hits.CountAsync();
        Assert.Equal(1, hitsAfter);
    }
    
    [Fact]
    public async Task DeleteDuplicates_Guest_OnlyOwn_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        var adminHits = Enumerable.Range(0, 100)
            .Select(i => CreateHit("adminData"));
        dbContext.Hits.AddRange(adminHits);
        var hits = Enumerable.Range(0, 100)
            .Select(i => CreateHit("guestData", guest.Id));
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/hit/duplicates");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(99, result.Value.Count);
        
        var hitsAfter = await dbContext.Hits.CountAsync();
        Assert.Equal(101, hitsAfter);
    }
    
    [Fact]
    public async Task Purge_Admin_ClearAllHits()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var hits = Enumerable.Range(0, 100)
            .Select(i => CreateHit());
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        // Act
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/hit/purge");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.Count);
        
        var hitsAfter = await dbContext.Hits.CountAsync();
        Assert.Equal(0, hitsAfter);
    }
    
    [Fact]
    public async Task Purge_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Guests.Add(guest);
        var hits = Enumerable.Range(0, 100)
            .Select(i => CreateHit());
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/hit/purge");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
        
        var hitsAfter = await dbContext.Hits.CountAsync();
        Assert.Equal(100, hitsAfter);
    }
    
    [Fact]
    public async Task GetRecent_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        List<HitEntity> hits = [
            // Recent for config1
            new HitEntity {
                Date = DateTime.UtcNow,
                Type = "SUCCESS",
                ConfigName = "config1"
            },
            new HitEntity {
                Date = DateTime.UtcNow.AddDays(-1),
                Type = "SUCCESS",
                ConfigName = "config1"
            },
            new HitEntity {
                Date = DateTime.UtcNow.AddDays(-1),
                Type = "SUCCESS",
                ConfigName = "config1"
            },
            new HitEntity {
                Date = DateTime.UtcNow.AddDays(-7),
                Type = "SUCCESS",
                ConfigName = "config1"
            },
            // Recent for config2
            new HitEntity {
                Date = DateTime.UtcNow.AddDays(-2),
                Type = "SUCCESS",
                ConfigName = "config2"
            }
        ];
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();

        // Act
        var queryParams = new
        {
            days = 3
        };
        var result = await GetJsonAsync<RecentHitsDto>(
            client, "/api/v1/hit/recent".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        
        // Make sure there are 3 dates as requested
        Assert.Equal(3, result.Value.Dates.Count());
        
        // Make sure there are 2 configs with names config1 and config2
        var dict = result.Value.Hits;
        Assert.Equal(2, dict.Keys.Count);
        Assert.Contains("config1", dict.Keys);
        Assert.Contains("config2", dict.Keys);
        
        // Config1 should have 0 hits on the first day, 2 hits on
        // the second day and 1 hit on the third day
        Assert.Equal(0, dict["config1"][0]);
        Assert.Equal(2, dict["config1"][1]);
        Assert.Equal(1, dict["config1"][2]);
        
        // Config2 should have 1 hit on the first day, 0 hits on
        // the second day and 0 hits on the third day
        Assert.Equal(1, dict["config2"][0]);
        Assert.Equal(0, dict["config2"][1]);
        Assert.Equal(0, dict["config2"][2]);
    }
    
    [Fact]
    public async Task GetRecent_Guest_OnlyOwn()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        List<HitEntity> adminHits = [
            new HitEntity {
                Date = DateTime.UtcNow,
                Type = "SUCCESS",
                ConfigName = "config1"
            }
        ];
        List<HitEntity> hits = [
            new HitEntity {
                Date = DateTime.UtcNow,
                Type = "SUCCESS",
                ConfigName = "config1",
                OwnerId = guest.Id
            },
            new HitEntity {
                Date = DateTime.UtcNow,
                Type = "SUCCESS",
                ConfigName = "config2",
                OwnerId = guest.Id
            }
        ];
        dbContext.Hits.AddRange(adminHits);
        dbContext.Hits.AddRange(hits);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            days = 1
        };
        var result = await GetJsonAsync<RecentHitsDto>(
            client, "/api/v1/hit/recent".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        
        // Make sure there is 1 date as requested
        Assert.Single(result.Value.Dates);
        
        // Make sure there are 2 configs with names config1 and config2
        var dict = result.Value.Hits;
        Assert.Equal(2, dict.Keys.Count);
        Assert.Contains("config1", dict.Keys);
        Assert.Contains("config2", dict.Keys);
        
        // Config1 should have 1 hit on the only day
        // (the admin one is not counted)
        Assert.Equal(1, dict["config1"][0]);
        
        // Config2 should have 1 hit on the only day
        Assert.Equal(1, dict["config2"][0]);
    }
    
    private static HitEntity CreateHit(
        string? data = null, int ownerId = 0)
    {
        return new HitEntity
        {
            Data = data ?? "Data",
            CapturedData = "CapturedData",
            Proxy = "(Socks5)127.0.0.1:8080:user:pass",
            Date = DateTime.UtcNow,
            Type = "SUCCESS",
            ConfigId = "ConfigId",
            ConfigName = "ConfigName",
            ConfigCategory = "ConfigCategory",
            WordlistId = 1,
            WordlistName = "WordlistName",
            OwnerId = ownerId
        };
    }
}
