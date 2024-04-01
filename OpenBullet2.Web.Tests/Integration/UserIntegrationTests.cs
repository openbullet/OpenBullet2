using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.User;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class UserIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task Login_AdminUser_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var obSettings = GetRequiredService<OpenBulletSettingsService>();
        obSettings.Settings.SecuritySettings.RequireAdminLogin = true;
        obSettings.Settings.SecuritySettings.AdminUsername = "admin_user";
        obSettings.Settings.SecuritySettings.AdminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin_pass");
        
        var dto = new UserLoginDto
        {
            Username = "admin_user",
            Password = "admin_pass"
        };
        
        // Act
        var result = await PostJsonAsync<LoggedInUserDto>(client, "/api/v1/user/login", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Token);
        
        // Read the token and make sure the claims are correct
        var token = result.Value.Token;
        var claims = GetClaimsFromToken(token).ToList();
        
        // Make sure there is a claim of type ClaimTypes.NameIdentifier with value 0
        var nameIdentifierClaim = claims.Find(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.NotNull(nameIdentifierClaim);
        Assert.Equal("0", nameIdentifierClaim.Value);
        
        // Make sure there is a claim of type ClaimTypes.Name with value admin_user
        var nameClaim = claims.Find(c => c.Type == ClaimTypes.Name);
        Assert.NotNull(nameClaim);
        Assert.Equal("admin_user", nameClaim.Value);
        
        // Make sure there is a claim of type ClaimTypes.Role with value Admin
        var roleClaim = claims.Find(c => c.Type == ClaimTypes.Role);
        Assert.NotNull(roleClaim);
        Assert.Equal("Admin", roleClaim.Value);
    }
    
    private static IEnumerable<Claim> GetClaimsFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        return jsonToken?.Claims ?? throw new ArgumentException("Invalid token", nameof(token));
    }
    
    [Fact]
    public async Task Login_AdminUser_Fail()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var obSettings = GetRequiredService<OpenBulletSettingsService>();
        obSettings.Settings.SecuritySettings.RequireAdminLogin = true;
        obSettings.Settings.SecuritySettings.AdminUsername = "admin_user";
        obSettings.Settings.SecuritySettings.AdminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin_pass");
        
        var dto = new UserLoginDto
        {
            Username = "admin_user",
            Password = "wrong_pass"
        };
        
        // Act
        var result = await PostJsonAsync<LoggedInUserDto>(client, "/api/v1/user/login", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Unauthorized, result.Error.Response.StatusCode);
    }
    
    [Fact]
    public async Task Login_GuestUser_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guestRepo = GetRequiredService<IGuestRepository>();
        var entity = new GuestEntity
        {
            Username = "guest_user",
            AccessExpiration = DateTime.Today.Add(TimeSpan.FromDays(7)),
            AllowedAddresses = string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest_pass")
        };
        await guestRepo.AddAsync(entity);
        var dto = new UserLoginDto
        {
            Username = "guest_user",
            Password = "guest_pass"
        };
        
        // Act
        var result = await PostJsonAsync<LoggedInUserDto>(client, "/api/v1/user/login", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Token);
        
        // Read the token and make sure the claims are correct
        var token = result.Value.Token;
        var claims = GetClaimsFromToken(token).ToList();
        
        // Make sure there is a claim of type ClaimTypes.NameIdentifier with value 0
        var nameIdentifierClaim = claims.Find(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.NotNull(nameIdentifierClaim);
        Assert.Equal(entity.Id.ToString(), nameIdentifierClaim.Value);
        
        // Make sure there is a claim of type ClaimTypes.Name with value guest_user
        var nameClaim = claims.Find(c => c.Type == ClaimTypes.Name);
        Assert.NotNull(nameClaim);
        Assert.Equal("guest_user", nameClaim.Value);
        
        // Make sure there is a claim of type ClaimTypes.Role with value Guest
        var roleClaim = claims.Find(c => c.Type == ClaimTypes.Role);
        Assert.NotNull(roleClaim);
        Assert.Equal("Guest", roleClaim.Value);
    }
    
    [Fact]
    public async Task Login_GuestUserInvalidIp_Fail()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guestRepo = GetRequiredService<IGuestRepository>();
        var entity = new GuestEntity
        {
            Username = "guest_user",
            AccessExpiration = DateTime.Today.Add(TimeSpan.FromDays(7)),
            AllowedAddresses = "1.1.1.1",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest_pass")
        };
        await guestRepo.AddAsync(entity);
        var dto = new UserLoginDto
        {
            Username = "guest_user",
            Password = "guest_pass"
        };
        
        // Act
        var result = await PostJsonAsync<LoggedInUserDto>(client, "/api/v1/user/login", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Unauthorized, result.Error.Response.StatusCode);
    }
    
    [Fact]
    public async Task Login_GuestUserExpired_Fail()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guestRepo = GetRequiredService<IGuestRepository>();
        var entity = new GuestEntity
        {
            Username = "guest_user",
            AccessExpiration = DateTime.Today.Add(TimeSpan.FromDays(-7)),
            AllowedAddresses = string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest_pass")
        };
        await guestRepo.AddAsync(entity);
        var dto = new UserLoginDto
        {
            Username = "guest_user",
            Password = "guest_pass"
        };
        
        // Act
        var result = await PostJsonAsync<LoggedInUserDto>(client, "/api/v1/user/login", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Unauthorized, result.Error.Response.StatusCode);
    }
    
    [Fact]
    public async Task Login_GuestUser_Fail()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guestRepo = GetRequiredService<IGuestRepository>();
        var entity = new GuestEntity
        {
            Username = "guest_user",
            AccessExpiration = DateTime.Today.Add(TimeSpan.FromDays(7)),
            AllowedAddresses = string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest_pass")
        };
        await guestRepo.AddAsync(entity);
        var dto = new UserLoginDto
        {
            Username = "guest_user",
            Password = "wrong_pass"
        };
        
        // Act
        var result = await PostJsonAsync<LoggedInUserDto>(client, "/api/v1/user/login", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Unauthorized, result.Error.Response.StatusCode);
    }
}
