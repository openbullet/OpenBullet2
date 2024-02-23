using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Web.Dtos.Wordlist;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Tests.Extensions;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class WordlistIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task GetWordlist_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var wordlist = new WordlistEntity
        {
            Name = "test",
            FileName = Path.GetTempFileName(),
            Purpose = "test",
            Total = 10,
            Type = "Default"
        };
        dbContext.Wordlists.Add(wordlist);
        await dbContext.SaveChangesAsync();
        
        // Act
        var queryParams = new
        {
            id = wordlist.Id
        };
        var result = await GetJsonAsync<WordlistDto>(
            client, "/api/v1/wordlist".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(wordlist.Id, result.Value.Id);
        Assert.Equal(wordlist.Name, result.Value.Name);
        Assert.Equal(wordlist.FileName, result.Value.FilePath);
        Assert.Equal(wordlist.Purpose, result.Value.Purpose);
        Assert.Equal(wordlist.Total, result.Value.LineCount);
        Assert.Equal(wordlist.Type, result.Value.WordlistType);
        Assert.Null(result.Value.Owner);
    }
    
    [Fact]
    public async Task GetWordlist_GuestOwned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest" };
        dbContext.Guests.Add(guest);
        var wordlist = new WordlistEntity
        {
            Name = "test",
            FileName = Path.GetTempFileName(),
            Purpose = "test",
            Total = 10,
            Type = "Default",
            Owner = guest
        };
        dbContext.Wordlists.Add(wordlist);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = wordlist.Id
        };
        var result = await GetJsonAsync<WordlistDto>(
            client, "/api/v1/wordlist".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(wordlist.Id, result.Value.Id);
        Assert.Equal(wordlist.Name, result.Value.Name);
        Assert.Equal(wordlist.FileName, result.Value.FilePath);
        Assert.Equal(wordlist.Purpose, result.Value.Purpose);
        Assert.Equal(wordlist.Total, result.Value.LineCount);
        Assert.Equal(wordlist.Type, result.Value.WordlistType);
        Assert.NotNull(result.Value.Owner);
        Assert.Equal(guest.Id, result.Value.Owner.Id);
    }
    
    [Fact]
    public async Task GetWordlist_GuestNotOwned_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest" };
        dbContext.Guests.Add(guest);
        var wordlist = new WordlistEntity
        {
            Name = "test",
            FileName = Path.GetTempFileName(),
            Purpose = "test",
            Total = 10,
            Type = "Default",
            Owner = new GuestEntity { Username = "other" }
        };
        dbContext.Wordlists.Add(wordlist);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = wordlist.Id
        };
        var result = await GetJsonAsync<WordlistDto>(
            client, "/api/v1/wordlist".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.WordlistNotFound, result.Error.Content!.ErrorCode);
    }

    [Fact]
    public async Task GetAll_Admin_ListAll()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var wordlist1 = new WordlistEntity
        {
            Name = "test1",
            FileName = Path.GetTempFileName(),
            Purpose = "test",
            Total = 10,
            Type = "Default"
        };
        var wordlist2 = new WordlistEntity
        {
            Name = "test2",
            FileName = Path.GetTempFileName(),
            Purpose = "test",
            Total = 10,
            Type = "Default"
        };
        dbContext.Wordlists.AddRange(wordlist1, wordlist2);
        await dbContext.SaveChangesAsync();
        
        // Act
        var result = await GetJsonAsync<IEnumerable<WordlistDto>>(
            client, "/api/v1/wordlist/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
        
        var result1 = result.Value.First();
        Assert.Equal(wordlist1.Id, result1.Id);
        Assert.Equal(wordlist1.Name, result1.Name);
        Assert.Equal(wordlist1.FileName, result1.FilePath);
        Assert.Equal(wordlist1.Purpose, result1.Purpose);
        Assert.Equal(wordlist1.Total, result1.LineCount);
        Assert.Equal(wordlist1.Type, result1.WordlistType);
        Assert.Null(result1.Owner);
    }

    [Fact]
    public async Task GetAll_Guest_OnlyOwn()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest" };
        var guest2 = new GuestEntity { Username = "guest" };
        dbContext.Guests.AddRange(guest, guest2);
        var wordlist1 = new WordlistEntity
        {
            Name = "test1",
            FileName = Path.GetTempFileName(),
            Purpose = "test",
            Total = 10,
            Type = "Default",
            Owner = guest
        };
        var wordlist2 = new WordlistEntity
        {
            Name = "test2",
            FileName = Path.GetTempFileName(),
            Purpose = "test",
            Total = 10,
            Type = "Default",
            Owner = guest2
        };
        var wordlist3 = new WordlistEntity
        {
            Name = "test3",
            FileName = Path.GetTempFileName(),
            Purpose = "test",
            Total = 10,
            Type = "Default"
        };
        dbContext.Wordlists.AddRange(wordlist1, wordlist2, wordlist3);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<WordlistDto>>(
            client, "/api/v1/wordlist/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        
        var result1 = result.Value.First();
        Assert.Equal(wordlist1.Id, result1.Id);
    }

    [Fact]
    public async Task GetPreview_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var fileName = Path.GetTempFileName();
        await File.WriteAllLinesAsync(
            fileName, Enumerable.Range(0, 100).Select(i => i.ToString()));
        var wordlist = new WordlistEntity
        {
            Name = "test",
            FileName = fileName,
            Purpose = "test",
            Total = 10,
            Type = "Default"
        };
        dbContext.Wordlists.Add(wordlist);
        await dbContext.SaveChangesAsync();
        
        // Act
        var queryParams = new
        {
            id = wordlist.Id,
            lineCount = 5
        };
        var result = await GetJsonAsync<WordlistPreviewDto>(
            client, "/api/v1/wordlist/preview".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.FirstLines.Length);
        Assert.Equal("0", result.Value.FirstLines[0]);
        
        var fileInfo = new FileInfo(fileName);
        Assert.Equal(fileInfo.Length, result.Value.SizeInBytes);
    }
    
    [Fact]
    public async Task GetPreview_FileNotFound_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var wordlist = new WordlistEntity
        {
            Name = "test",
            FileName = Guid.NewGuid() + ".txt",
            Purpose = "test",
            Total = 10,
            Type = "Default"
        };
        dbContext.Wordlists.Add(wordlist);
        await dbContext.SaveChangesAsync();
        
        // Act
        var queryParams = new
        {
            id = wordlist.Id,
            lineCount = 5
        };
        var result = await GetJsonAsync<WordlistPreviewDto>(
            client, "/api/v1/wordlist/preview".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.FileNotFound, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task GetPreview_Guest_Owned_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest" };
        dbContext.Guests.Add(guest);
        var fileName = Path.GetTempFileName();
        await File.WriteAllLinesAsync(
            fileName, Enumerable.Range(0, 100).Select(i => i.ToString()));
        var wordlist = new WordlistEntity
        {
            Name = "test",
            FileName = fileName,
            Purpose = "test",
            Total = 10,
            Type = "Default",
            Owner = guest
        };
        dbContext.Wordlists.Add(wordlist);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = wordlist.Id,
            lineCount = 5
        };
        var result = await GetJsonAsync<WordlistPreviewDto>(
            client, "/api/v1/wordlist/preview".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.FirstLines.Length);
        Assert.Equal("0", result.Value.FirstLines[0]);
        
        var fileInfo = new FileInfo(fileName);
        Assert.Equal(fileInfo.Length, result.Value.SizeInBytes);
    }
    
    [Fact]
    public async Task GetPreview_Guest_NotOwned_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest" };
        dbContext.Guests.Add(guest);
        var wordlist = new WordlistEntity
        {
            Name = "test",
            FileName = Path.GetTempFileName(),
            Purpose = "test",
            Total = 10,
            Type = "Default",
            Owner = new GuestEntity { Username = "other" }
        };
        dbContext.Wordlists.Add(wordlist);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = wordlist.Id,
            lineCount = 5
        };
        var result = await GetJsonAsync<WordlistPreviewDto>(
            client, "/api/v1/wordlist/preview".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.WordlistNotFound, result.Error.Content!.ErrorCode);
    }
}
