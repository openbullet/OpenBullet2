using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Wordlist;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Tests.Extensions;
using RuriLib.Extensions;
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
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
    
    /// <summary>
    /// An admin should be able to create a wordlist.
    /// </summary>
    [Fact]
    public async Task CreateWordlist_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var fileName = Path.GetTempFileName();
        await File.WriteAllLinesAsync(
            fileName, Enumerable.Range(0, 100).Select(i => i.ToString()));
        var dto = new CreateWordlistDto
        {
            Name = "test",
            Purpose = "test",
            WordlistType = "Default",
            FilePath = fileName
        };
        
        // Act
        var result = await PostJsonAsync<WordlistDto>(
            client, "/api/v1/wordlist", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        var wordlist = await dbContext.Wordlists
            .FirstOrDefaultAsync(w => w.Id == result.Value.Id);
        
        Assert.NotNull(wordlist);
        Assert.Equal(wordlist.Id, result.Value.Id);
        Assert.Equal(dto.Name, result.Value.Name);
        Assert.Equal(wordlist.FileName, result.Value.FilePath);
        Assert.Equal(dto.Purpose, result.Value.Purpose);
        Assert.Equal(100, result.Value.LineCount);
        Assert.Equal(dto.WordlistType, result.Value.WordlistType);
        Assert.Null(result.Value.Owner);
    }
    
    /// <summary>
    /// A guest should be able to create a wordlist that
    /// references a file in the allowed subdirectory.
    /// </summary>
    [Fact]
    public async Task CreateWordlist_Guest_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        Directory.CreateDirectory(Path.Combine(UserDataFolder, "Wordlists"));
        var fileName = Path.Combine(UserDataFolder, "Wordlists", Guid.NewGuid() + ".txt");
        await File.WriteAllLinesAsync(
            fileName, Enumerable.Range(0, 100).Select(i => i.ToString()));
        var dto = new CreateWordlistDto
        {
            Name = "test",
            Purpose = "test",
            WordlistType = "Default",
            FilePath = fileName
        };
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await PostJsonAsync<WordlistDto>(
            client, "/api/v1/wordlist", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        var wordlist = await dbContext.Wordlists
            .Include(w => w.Owner)
            .FirstOrDefaultAsync(w => w.Id == result.Value.Id);
        
        Assert.NotNull(wordlist);
        Assert.Equal(wordlist.Id, result.Value.Id);
        Assert.Equal(dto.Name, result.Value.Name);
        Assert.Equal(wordlist.FileName, result.Value.FilePath);
        Assert.Equal(dto.Purpose, result.Value.Purpose);
        Assert.Equal(100, result.Value.LineCount);
        Assert.Equal(dto.WordlistType, result.Value.WordlistType);
        Assert.NotNull(result.Value.Owner);
        Assert.Equal(guest.Id, result.Value.Owner.Id);
    }
    
    [Fact]
    public async Task CreateWordlist_Guest_OutsideAllowedPath_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var fileName = Path.GetTempFileName();
        await File.WriteAllLinesAsync(
            fileName, Enumerable.Range(0, 100).Select(i => i.ToString()));
        var dto = new CreateWordlistDto
        {
            Name = "test",
            Purpose = "test",
            WordlistType = "Default",
            FilePath = fileName
        };
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await PostJsonAsync<WordlistDto>(
            client, "/api/v1/wordlist", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.FileOutsideAllowedPath, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task CreateWordlist_FileNotFound_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dto = new CreateWordlistDto
        {
            Name = "test",
            Purpose = "test",
            WordlistType = "Default",
            FilePath = Guid.NewGuid() + ".txt"
        };
        
        // Act
        var result = await PostJsonAsync<WordlistDto>(
            client, "/api/v1/wordlist", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.FileNotFound, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task UploadWordlistFile_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var lines = Enumerable.Range(0, 100).Select(i => i.ToString());
        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        foreach (var line in lines) await writer.WriteLineAsync(line);
        stream.Position = 0;
        
        var content = new MultipartFormDataContent
        {
            { new StreamContent(stream), "file", "test.txt" }
        };
        
        // Act
        var result = await client.PostAsync(
            "/api/v1/wordlist/upload", content);
        
        // Assert
        result.EnsureSuccessStatusCode();
        
        var response = await result.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<WordlistFileDto>(
            response, JsonSerializerOptions);
        
        Assert.NotNull(dto);
        Assert.True(dto.FilePath.IsSubPathOf(
            Path.Combine(UserDataFolder, "Wordlists")));
    }
    
    // TODO: Delete wordlist (admin)
    [Fact]
    public async Task DeleteWordlist_Admin_WithoutFile_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var fileName = Path.GetTempFileName();
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
            alsoDeleteFile = false
        };
        var result = await DeleteAsync(
            client, "/api/v1/wordlist".ToUri(queryParams));
        
        // Assert
        Assert.Null(result);
        
        var deleted = await dbContext.Wordlists
            .FirstOrDefaultAsync(w => w.Id == wordlist.Id);
        
        Assert.Null(deleted);
        Assert.True(File.Exists(fileName));
    }
    
    [Fact]
    public async Task DeleteWordlist_Admin_WithFile_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var fileName = Path.GetTempFileName();
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
            alsoDeleteFile = true
        };
        var result = await DeleteAsync(
            client, "/api/v1/wordlist".ToUri(queryParams));
        
        // Assert
        Assert.Null(result);
        
        var deleted = await dbContext.Wordlists
            .FirstOrDefaultAsync(w => w.Id == wordlist.Id);
        
        Assert.Null(deleted);
        Assert.False(File.Exists(fileName));
    }
    
    [Fact]
    public async Task DeleteWordlist_Guest_NotOwned_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
            alsoDeleteFile = false
        };
        var error = await DeleteAsync(
            client, "/api/v1/wordlist".ToUri(queryParams));
        
        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.BadRequest, error.Response.StatusCode);
        Assert.Equal(ErrorCode.WordlistNotFound, error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task DeleteNotFound_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var wordlist1 = new WordlistEntity
        {
            Name = "test1",
            FileName = Path.GetTempFileName(), // Exists
            Purpose = "test",
            Total = 10,
            Type = "Default"
        };
        var wordlist2 = new WordlistEntity
        {
            Name = "test2",
            FileName = Guid.NewGuid() + ".txt", // Doesn't exist
            Purpose = "test",
            Total = 10,
            Type = "Default"
        };
        dbContext.Wordlists.AddRange(wordlist1, wordlist2);
        await dbContext.SaveChangesAsync();
        
        // Act
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/wordlist/not-found");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Count);
        
        var deleted = await dbContext.Wordlists
            .FirstOrDefaultAsync(w => w.Id == wordlist2.Id);
        
        Assert.Null(deleted);
        
        var notDeleted = await dbContext.Wordlists
            .FirstOrDefaultAsync(w => w.Id == wordlist1.Id);
        
        Assert.NotNull(notDeleted);
    }
    
    [Fact]
    public async Task DeleteNotFound_Guest_OnlyOwn()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        var wordlist1 = new WordlistEntity
        {
            Name = "test1",
            FileName = Path.GetTempFileName(), // Exists
            Purpose = "test",
            Total = 10,
            Type = "Default",
            Owner = guest
        };
        var wordlist2 = new WordlistEntity
        {
            Name = "test2",
            FileName = Guid.NewGuid() + ".txt", // Doesn't exist
            Purpose = "test",
            Total = 10,
            Type = "Default",
            Owner = guest
        };
        var wordlist3 = new WordlistEntity
        {
            Name = "test3",
            FileName = Guid.NewGuid() + ".txt", // Doesn't exist
            Purpose = "test",
            Total = 10,
            Type = "Default",
            Owner = new GuestEntity { Username = "other" }
        };
        var wordlist4 = new WordlistEntity
        {
            Name = "test4",
            FileName = Guid.NewGuid() + ".txt", // Doesn't exist
            Purpose = "test",
            Total = 10,
            Type = "Default"
        };
        dbContext.Wordlists.AddRange(wordlist1, wordlist2, wordlist3, wordlist4);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await DeleteJsonAsync<AffectedEntriesDto>(
            client, "/api/v1/wordlist/not-found");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Count);
        
        var deleted = await dbContext.Wordlists
            .FirstOrDefaultAsync(w => w.Id == wordlist2.Id);
        
        Assert.Null(deleted);
        
        var notDeleted = await dbContext.Wordlists
            .ToListAsync();
        
        Assert.Equal(3, notDeleted.Count);
    }
    
    [Fact]
    public async Task UpdateWordlistInfo_Admin_Success()
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
        
        var dto = new UpdateWordlistInfoDto
        {
            Id = wordlist.Id,
            Name = "test2",
            Purpose = "test2",
            WordlistType = "Default2"
        };
        
        // Act
        var result = await PatchJsonAsync<WordlistDto>(
            client, "/api/v1/wordlist/info", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(wordlist.Id, result.Value.Id);
        
        await dbContext.Entry(wordlist).ReloadAsync();
        
        Assert.Equal(dto.Name, wordlist.Name);
        Assert.Equal(dto.Purpose, wordlist.Purpose);
        Assert.Equal(dto.WordlistType, wordlist.Type);
    }
    
    [Fact]
    public async Task UpdateWordlistInfo_Guest_NotOwned_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
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
        
        var dto = new UpdateWordlistInfoDto
        {
            Id = wordlist.Id,
            Name = "test2",
            Purpose = "test2",
            WordlistType = "Default2"
        };
        
        // Act
        var result = await PatchJsonAsync<WordlistDto>(
            client, "/api/v1/wordlist/info", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.WordlistNotFound, result.Error.Content!.ErrorCode);
    }
}
