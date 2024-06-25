using System.Net;
using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Settings;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Services;
using OpenBullet2.Web.Tests.Extensions;
using RuriLib.Models.Settings;
using RuriLib.Services;
using Xunit.Abstractions;
using GeneralSettings = RuriLib.Models.Settings.GeneralSettings;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class SettingsIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task GetEnvironmentSettings()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var result = await GetJsonAsync<EnvironmentSettingsDto>(
            client, "/api/v1/settings/environment");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.WordlistTypes, x => x.Name == "Default");
        Assert.True(result.Value.ExportFormats.Count > 0);
        Assert.Contains(result.Value.CustomStatuses, x => x.Name == "CUSTOM");
    }
    
    [Fact]
    public async Task GetRuriLibSettings_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var ruriLibSettings = GetRequiredService<RuriLibSettingsService>();
        ruriLibSettings.RuriLibSettings.GeneralSettings.LogAllResults = true;
        await ruriLibSettings.Save();
        
        // Act
        var result = await GetJsonAsync<GlobalSettings>(
            client, "/api/v1/settings/rurilib");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.GeneralSettings.LogAllResults);
    }
    
    [Fact]
    public async Task GetRuriLibSettings_Guest_Forbidden()
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
        var result = await GetJsonAsync<GlobalSettings>(
            client, "/api/v1/settings/rurilib");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task GetRuriLibDefaultSettings_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var result = await GetJsonAsync<GlobalSettings>(
            client, "/api/v1/settings/rurilib/default");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }
    
    [Fact]
    public async Task UpdateRuriLibSettings_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var ruriLibSettings = GetRequiredService<RuriLibSettingsService>();
        ruriLibSettings.RuriLibSettings.GeneralSettings.LogAllResults = true;
        await ruriLibSettings.Save();
        
        var newSettings = new GlobalSettings
        {
            GeneralSettings = new GeneralSettings { LogAllResults = false }
        };
        
        // Act
        var result = await PutJsonAsync<GlobalSettings>(
            client, "/api/v1/settings/rurilib", newSettings);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.GeneralSettings.LogAllResults);
        Assert.False(ruriLibSettings.RuriLibSettings.GeneralSettings.LogAllResults);
    }
    
    [Fact]
    public async Task UpdateRuriLibSettings_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        var newSettings = new GlobalSettings
        {
            GeneralSettings = new GeneralSettings { LogAllResults = false }
        };
        
        // Act
        var result = await PutJsonAsync<GlobalSettings>(
            client, "/api/v1/settings/rurilib", newSettings);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task GetSettings_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var openBulletSettings = GetRequiredService<OpenBulletSettingsService>();
        openBulletSettings.Settings.GeneralSettings.DefaultAuthor = "test";
        await openBulletSettings.SaveAsync();
        
        // Act
        var result = await GetJsonAsync<OpenBulletSettingsDto>(
            client, "/api/v1/settings");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("test", result.Value.GeneralSettings.DefaultAuthor);
    }
    
    [Fact]
    public async Task GetSettings_Guest_Forbidden()
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
        var result = await GetJsonAsync<OpenBulletSettingsDto>(
            client, "/api/v1/settings");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task GetDefaultSettings_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var result = await GetJsonAsync<OpenBulletSettingsDto>(
            client, "/api/v1/settings/default");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }
    
    [Fact]
    public async Task UpdateSettings_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var openBulletSettings = GetRequiredService<OpenBulletSettingsService>();
        openBulletSettings.Settings.GeneralSettings.DefaultAuthor = "test";
        await openBulletSettings.SaveAsync();
        
        var newSettings = new OpenBulletSettingsDto
        {
            GeneralSettings = new OBGeneralSettingsDto { DefaultAuthor = "test2" }
        };
        
        // Act
        var result = await PutJsonAsync<OpenBulletSettingsDto>(
            client, "/api/v1/settings", newSettings);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test2", result.Value.GeneralSettings.DefaultAuthor);
        Assert.Equal("test2", openBulletSettings.Settings.GeneralSettings.DefaultAuthor);
    }
    
    [Fact]
    public async Task UpdateAdminPassword_Admin_StrongPassword_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var openBulletSettings = GetRequiredService<OpenBulletSettingsService>();
        openBulletSettings.Settings.SecuritySettings.SetupAdminPassword("StrongPassword1_");
        await openBulletSettings.SaveAsync();
        
        var dto = new UpdateAdminPasswordDto { Password = "StrongPassword2#" };
        
        // Act
        var error = await PatchAsync(
            client, "/api/v1/settings/admin/password", dto);
        
        // Assert
        Assert.Null(error);
        BCrypt.Net.BCrypt.Verify(
            "test2", openBulletSettings.Settings.SecuritySettings.AdminPasswordHash);
    }
    
    [Fact]
    public async Task UpdateAdminPassword_Admin_WeakPassword_BadRequest()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var openBulletSettings = GetRequiredService<OpenBulletSettingsService>();
        openBulletSettings.Settings.SecuritySettings.SetupAdminPassword("StrongPassword1_");
        await openBulletSettings.SaveAsync();
        
        var dto = new UpdateAdminPasswordDto { Password = "weak" };
        
        // Act
        var error = await PatchAsync(
            client, "/api/v1/settings/admin/password", dto);
        
        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.BadRequest, error.Response.StatusCode);
        Assert.Equal(ErrorCode.ValidationError, error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task UpdateAdminPassword_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        var dto = new UpdateAdminPasswordDto { Password = "StrongPassword2#" };
        
        // Act
        var error = await PatchAsync(
            client, "/api/v1/settings/admin/password", dto);
        
        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.Forbidden, error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task AddTheme_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync(".test { color: red; }");
        stream.Position = 0;
        
        var content = new MultipartFormDataContent
        {
            { new StreamContent(stream), "file", "test.css" }
        };
        
        // Act
        var response = await client.PostAsync(
            "/api/v1/settings/theme", content);
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        var themePath = Path.Combine(
            UserDataFolder, "Themes", "test.css");
        Assert.True(File.Exists(themePath));
    }
    
    [Fact]
    public async Task AddTheme_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync(".test { color: red; }");
        stream.Position = 0;
        
        var content = new MultipartFormDataContent
        {
            { new StreamContent(stream), "file", "test.css" }
        };
        
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var response = await client.PostAsync(
            "/api/v1/settings/theme", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAllThemes_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var themeService = GetRequiredService<ThemeService>();
        using var stream = new MemoryStream();
        await themeService.SaveCssFileAsync("test.css", stream);
        
        // Act
        var result = await GetJsonAsync<List<ThemeDto>>(
            client, "/api/v1/settings/theme/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, x => x.Name == "test");
    }
    
    [Fact]
    public async Task GetAllThemes_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var themeService = GetRequiredService<ThemeService>();
        using var stream = new MemoryStream();
        await themeService.SaveCssFileAsync("test.css", stream);
        
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<List<ThemeDto>>(
            client, "/api/v1/settings/theme/all");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task GetTheme_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        Directory.CreateDirectory(Path.Combine(UserDataFolder, "Themes"));
        var filePath = Path.Combine(UserDataFolder, "Themes", "test.css");
        await File.WriteAllTextAsync(filePath, ".test { color: red; }");
        
        // Anyone can call this endpoint, so check if it works
        // for anonymous users as well
        RequireLogin();
        
        // Act
        var queryParams = new
        {
            name = "test"
        };
        var response = await client.GetAsync(
            "/api/v1/settings/theme".ToUri(queryParams));
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(".test { color: red; }", content);
    }
    
    /// <summary>
    /// Admin can get custom snippets.
    /// </summary>
    [Fact]
    public async Task GetCustomSnippets_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var openBulletSettings = GetRequiredService<OpenBulletSettingsService>();
        var snippet = new CustomSnippet {
            Name = "test",
            Description = "test",
            Body = "test"
        };
        openBulletSettings.Settings.GeneralSettings.CustomSnippets.Add(snippet);
        await openBulletSettings.SaveAsync();
        
        // Act
        var result = await GetJsonAsync<Dictionary<string, string>>(
            client, "/api/v1/settings/custom-snippets");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Contains(result.Value, x => x.Key == "test");
    }
    
    /// <summary>
    /// Guest cannot get custom snippets.
    /// </summary>
    [Fact]
    public async Task GetCustomSnippets_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        RequireLogin();
        ImpersonateGuest(client, new GuestEntity { Username = "guest" });
        
        // Act
        var result = await GetJsonAsync<Dictionary<string, string>>(
            client, "/api/v1/settings/custom-snippets");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
}
