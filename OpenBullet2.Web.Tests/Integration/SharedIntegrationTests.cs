using System.IO.Compression;
using System.Net;
using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Sharing;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Shared;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Services;
using OpenBullet2.Web.Tests.Extensions;
using RuriLib.Helpers;
using RuriLib.Models.Configs;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class SharedIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task GetAllEndpoints_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configSharingService = GetRequiredService<ConfigSharingService>();
        var config1 = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "config1"
            }
        };
        var config2 = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "config2"
            }
        };
        var config3 = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "config3"
            }
        };
        configService.Configs.Add(config1);
        configService.Configs.Add(config2);
        configService.Configs.Add(config3);
        var endpoint1 = new Endpoint
        {
            Route = "test",
            ApiKeys = ["apikey1", "apikey2"],
            ConfigIds = [config1.Id, config2.Id]
        };
        var endpoint2 = new Endpoint
        {
            Route = "test2",
            ApiKeys = ["apikey3"],
            ConfigIds = [config3.Id]
        };
        configSharingService.Endpoints.Add(endpoint1);
        configSharingService.Endpoints.Add(endpoint2);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<EndpointDto>>(
            client, "/api/v1/shared/endpoint/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
        
        var endpoints = result.Value.ToList();
        Assert.Equal("test", endpoints[0].Route);
        Assert.Equal(2, endpoints[0].ApiKeys.Count());
        Assert.Equal(2, endpoints[0].ConfigIds.Count());
        Assert.Equal("test2", endpoints[1].Route);
        Assert.Single(endpoints[1].ApiKeys);
        Assert.Single(endpoints[1].ConfigIds);
    }
    
    [Fact]
    public async Task GetAllEndpoints_Guest_Forbidden()
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
        var result = await GetJsonAsync<IEnumerable<EndpointDto>>(
            client, "/api/v1/shared/endpoint/all");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task CreateEndpoint_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configSharingService = GetRequiredService<ConfigSharingService>();
        var config = new Config { Id = Guid.NewGuid().ToString() };
        configService.Configs.Add(config);
        var dto = new EndpointDto
        {
            Route = "test",
            ApiKeys = ["apikey1", "apikey2"],
            ConfigIds = [config.Id]
        };
        
        // Act
        var result = await PostJsonAsync<EndpointDto>(
            client, "/api/v1/shared/endpoint", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        var endpoint = configSharingService.GetEndpoint("test");
        Assert.NotNull(endpoint);
        Assert.Equal("test", endpoint.Route);
        Assert.Equal(2, endpoint.ApiKeys.Count);
        Assert.Single(endpoint.ConfigIds);
    }
    
    [Fact]
    public async Task CreateEndpoint_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var dto = new EndpointDto
        {
            Route = "test",
            ApiKeys = ["apikey1", "apikey2"],
            ConfigIds = []
        };
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await PostJsonAsync<EndpointDto>(
            client, "/api/v1/shared/endpoint", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task UpdateEndpoint_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configSharingService = GetRequiredService<ConfigSharingService>();
        var config = new Config { Id = Guid.NewGuid().ToString() };
        configService.Configs.Add(config);
        var endpoint = new Endpoint
        {
            Route = "test",
            ApiKeys = ["apikey1", "apikey2"],
            ConfigIds = [config.Id]
        };
        configSharingService.Endpoints.Add(endpoint);
        var dto = new EndpointDto
        {
            Route = "test",
            ApiKeys = ["apikey3"],
            ConfigIds = [config.Id]
        };
        
        // Act
        var result = await PutJsonAsync<EndpointDto>(
            client, "/api/v1/shared/endpoint", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        var updatedEndpoint = configSharingService.GetEndpoint("test");
        Assert.NotNull(updatedEndpoint);
        Assert.Equal("test", updatedEndpoint.Route);
        Assert.Single(updatedEndpoint.ApiKeys);
        Assert.Single(updatedEndpoint.ConfigIds);
    }
    
    [Fact]
    public async Task UpdateEndpoint_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configSharingService = GetRequiredService<ConfigSharingService>();
        var config = new Config { Id = Guid.NewGuid().ToString() };
        configService.Configs.Add(config);
        var endpoint = new Endpoint
        {
            Route = "test",
            ApiKeys = ["apikey1", "apikey2"],
            ConfigIds = [config.Id]
        };
        configSharingService.Endpoints.Add(endpoint);
        var dto = new EndpointDto
        {
            Route = "test",
            ApiKeys = ["apikey3"],
            ConfigIds = [config.Id]
        };
        
        RequireLogin();
        ImpersonateGuest(client, new GuestEntity { Username = "guest" });
        
        // Act
        var result = await PutJsonAsync<EndpointDto>(
            client, "/api/v1/shared/endpoint", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task DeleteEndpoint_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configSharingService = GetRequiredService<ConfigSharingService>();
        var config = new Config { Id = Guid.NewGuid().ToString() };
        configService.Configs.Add(config);
        var endpoint = new Endpoint
        {
            Route = "test",
            ApiKeys = ["apikey1", "apikey2"],
            ConfigIds = [config.Id]
        };
        configSharingService.Endpoints.Add(endpoint);
        
        // Act
        var queryParams = new { route = "test" };
        var error = await DeleteAsync(
            client, "/api/v1/shared/endpoint".ToUri(queryParams));
        
        // Assert
        Assert.Null(error);
        Assert.Null(configSharingService.GetEndpoint("test"));
    }
    
    [Fact]
    public async Task DeleteEndpoint_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configSharingService = GetRequiredService<ConfigSharingService>();
        var config = new Config { Id = Guid.NewGuid().ToString() };
        configService.Configs.Add(config);
        var endpoint = new Endpoint
        {
            Route = "test",
            ApiKeys = ["apikey1", "apikey2"],
            ConfigIds = [config.Id]
        };
        configSharingService.Endpoints.Add(endpoint);
        
        RequireLogin();
        ImpersonateGuest(client, new GuestEntity { Username = "guest" });
        
        // Act
        var queryParams = new { route = "test" };
        var error = await DeleteAsync(
            client, "/api/v1/shared/endpoint".ToUri(queryParams));
        
        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.Forbidden, error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task DownloadConfigs_CorrectApiKey_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepo = GetRequiredService<IConfigRepository>();
        var configSharingService = GetRequiredService<ConfigSharingService>();
        var config = await configRepo.CreateAsync();
        config.Metadata.Name = "myConfig";
        configService.Configs.Add(config);
        await configRepo.SaveAsync(config);
        var endpoint = new Endpoint
        {
            Route = "test",
            ApiKeys = ["apikey1", "apikey2"],
            ConfigIds = [config.Id]
        };
        configSharingService.Endpoints.Add(endpoint);
        client.DefaultRequestHeaders.Add("Api-Key", "apikey1");
        
        // Since this endpoint can be called by anybody, we
        // make sure it doesn't error out if login is required and
        // no form of authentication is provided
        RequireLogin();
        
        // Act
        var result = await client.GetAsync("/api/v1/shared/configs/test");
        
        // Assert
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal("application/zip", result.Content.Headers.ContentType!.MediaType);
        Assert.Equal("attachment", result.Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("configs.zip", result.Content.Headers.ContentDisposition!.FileName);
        
        // Open the zip stream and unpack the config, make sure the ID is correct
        var archive = new ZipArchive(await result.Content.ReadAsStreamAsync());
        Assert.Single(archive.Entries);
        var entry = archive.Entries[0];
        await using var stream = entry.Open();
        var unpackedConfig = await ConfigPacker.UnpackAsync(stream);
        
        // The id is randomized on the downloaded configs, so
        // we check the name instead
        Assert.Equal("myConfig", unpackedConfig.Metadata.Name);
    }
    
    [Fact]
    public async Task DownloadConfigs_IncorrectApiKey_Unauthorized()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepo = GetRequiredService<IConfigRepository>();
        var configSharingService = GetRequiredService<ConfigSharingService>();
        var config = await configRepo.CreateAsync();
        config.Metadata.Name = "myConfig";
        configService.Configs.Add(config);
        await configRepo.SaveAsync(config);
        var endpoint = new Endpoint
        {
            Route = "test",
            ApiKeys = ["apikey1", "apikey2"],
            ConfigIds = [config.Id]
        };
        configSharingService.Endpoints.Add(endpoint);
        client.DefaultRequestHeaders.Add("Api-Key", "invalid");
        
        // Act
        var result = await client.GetAsync("/api/v1/shared/configs/test");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}
