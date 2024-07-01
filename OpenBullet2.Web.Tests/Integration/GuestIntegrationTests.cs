using System.Net;
using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Web.Dtos.Guest;
using OpenBullet2.Web.Exceptions;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class GuestIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task CreateGuest_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var repo = GetRequiredService<IGuestRepository>();
        var dto = new CreateGuestDto 
        {
            Username = "guest",
            Password = "guest123",
            AccessExpiration = DateTime.UtcNow.AddDays(30),
            AllowedAddresses = [
                "192.168.1.1",
                "::1",
                "10.0.0.0/24",
                "example.dyndns.org"
            ]
        };
        
        // Act
        var result = await PostJsonAsync<GuestDto>(client, "/api/v1/guest", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        var guest = await repo.GetAsync(result.Value.Id);
        Assert.NotNull(guest);
        Assert.Equal("guest", guest.Username);
        Assert.Equal(dto.AccessExpiration, guest.AccessExpiration);
        Assert.True(guest.AllowedAddresses.Split(',').SequenceEqual(dto.AllowedAddresses));
    }
    
    [Fact]
    public async Task CreateGuest_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();

        var dto = new CreateGuestDto 
        {
            Username = "guest2",
            Password = "guest123"
        };
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await PostJsonAsync<GuestDto>(client, "/api/v1/guest", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task CreateGuest_UsernameAlreadyExists_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var repo = GetRequiredService<IGuestRepository>();
        var dto = new CreateGuestDto {
            Username = "guest",
            Password = "guest123"
        };
        await repo.AddAsync(new GuestEntity
        {
            Username = "guest",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123")
        });
        
        // Act
        var result = await PostJsonAsync<GuestDto>(client, "/api/v1/guest", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.UsernameTaken, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task CreateGuest_AdminUsernameAlreadyExists_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dto = new CreateGuestDto {
            Username = "admin",
            Password = "guest123"
        };
        
        // Act
        var result = await PostJsonAsync<GuestDto>(client, "/api/v1/guest", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.UsernameTaken, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task UpdateGuestInfo_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var repo = GetRequiredService<IGuestRepository>();
        var guest = new GuestEntity {
            Username = "guest",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123"),
            AccessExpiration = DateTime.UtcNow.AddDays(30),
            AllowedAddresses = ""
        };
        await repo.AddAsync(guest);

        var dto = new UpdateGuestInfoDto {
            Id = guest.Id,
            Username = "guest2",
            AccessExpiration = DateTime.UtcNow.AddDays(60),
            AllowedAddresses = [
                "127.0.0.1"
            ]
        };
        
        // Act
        var result = await PatchJsonAsync<GuestDto>(client, "/api/v1/guest/info", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        var updatedGuest = await repo.GetAsync(result.Value.Id);
        await dbContext.Entry(guest).ReloadAsync();
        Assert.NotNull(updatedGuest);
        Assert.Equal(dto.AccessExpiration, updatedGuest.AccessExpiration);
        Assert.True(updatedGuest.AllowedAddresses.Split(',').SequenceEqual(dto.AllowedAddresses));
    }
    
    [Fact]
    public async Task UpdateGuestInfo_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        var dto = new UpdateGuestInfoDto {
            Id = guest.Id,
            Username = "guest2"
        };
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await PatchJsonAsync<GuestDto>(
            client, "/api/v1/guest/info", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task UpdateGuestInfo_UsernameAlreadyExists_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var repo = GetRequiredService<IGuestRepository>();
        var guest1 = new GuestEntity {
            Username = "guest1",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123"),
            AccessExpiration = DateTime.UtcNow.AddDays(30),
            AllowedAddresses = ""
        };
        var guest2 = new GuestEntity {
            Username = "guest2",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123"),
            AccessExpiration = DateTime.UtcNow.AddDays(30),
            AllowedAddresses = ""
        };
        await repo.AddAsync(guest1);
        await repo.AddAsync(guest2);
        
        var dto = new UpdateGuestInfoDto {
            Id = guest1.Id,
            Username = "guest2"
        };
        
        // Act
        var result = await PatchJsonAsync<GuestDto>(client, "/api/v1/guest/info", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.UsernameTaken, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task UpdateGuestInfo_UsernameIsAdminUsername_Failure()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var repo = GetRequiredService<IGuestRepository>();
        var guest = new GuestEntity {
            Username = "guest",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123"),
            AccessExpiration = DateTime.UtcNow.AddDays(30),
            AllowedAddresses = ""
        };
        await repo.AddAsync(guest);
        var dto = new UpdateGuestInfoDto {
            Id = guest.Id,
            Username = "admin"
        };
        
        // Act
        var result = await PatchJsonAsync<GuestDto>(client, "/api/v1/guest/info", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.UsernameTaken, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task UpdateGuestPassword_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var repo = GetRequiredService<IGuestRepository>();
        var guest = new GuestEntity {
            Username = "guest",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123"),
            AccessExpiration = DateTime.UtcNow.AddDays(30),
            AllowedAddresses = ""
        };
        await repo.AddAsync(guest);
        var dto = new UpdateGuestPasswordDto {
            Id = guest.Id,
            Password = "guest1234"
        };
        
        // Act
        var result = await PatchJsonAsync<GuestDto>(client, "/api/v1/guest/password", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        var updatedGuest = await repo.GetAsync(result.Value.Id);
        await dbContext.Entry(guest).ReloadAsync();
        Assert.NotNull(updatedGuest);
        Assert.True(BCrypt.Net.BCrypt.Verify(dto.Password, updatedGuest.PasswordHash));
    }
    
    [Fact]
    public async Task UpdateGuestPassword_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var dto = new UpdateGuestPasswordDto {
            Id = guest.Id,
            Password = "guest1234"
        };
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await PatchJsonAsync<GuestDto>(
            client, "/api/v1/guest/password", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task DeleteGuest_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var repo = GetRequiredService<IGuestRepository>();
        var guest = new GuestEntity {
            Username = "guest",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123"),
            AccessExpiration = DateTime.UtcNow.AddDays(30),
            AllowedAddresses = ""
        };
        await repo.AddAsync(guest);
        
        // Act
        var result = await DeleteAsync(client, $"/api/v1/guest?id={guest.Id}");
        
        // Assert
        Assert.Null(result);
        Assert.Null(await repo.GetAsync(guest.Id));
    }
    
    [Fact]
    public async Task DeleteGuest_Guest_Forbidden()
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
        var error = await DeleteAsync(client, $"/api/v1/guest?id={guest.Id}");
        
        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.Forbidden, error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, error.Content!.ErrorCode);
    }
}
