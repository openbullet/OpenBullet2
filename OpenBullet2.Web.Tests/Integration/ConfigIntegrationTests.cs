using System.IO.Compression;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Dtos.Config.Blocks;
using OpenBullet2.Web.Dtos.Config.Convert;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Models.Errors;
using OpenBullet2.Web.Tests.Extensions;
using RuriLib.Helpers;
using RuriLib.Models.Blocks;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Services;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class ConfigIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    private const string _additionScript = """
                                           BLOCK:Addition
                                             firstNumber = 1
                                             secondNumber = 2
                                             => VAR @result
                                           ENDBLOCK
                                           """;
    
    /// <summary>
    /// Admin can read all configs' overview info.
    /// </summary>
    [Fact]
    public async Task GetAll_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var config1 = new Config {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig1",
                Author = "TestAuthor1",
                Category = "TestCategory1",
                CreationDate = new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            Settings = new ConfigSettings
            {
                ProxySettings = new ProxySettings
                {
                    UseProxies = false
                },
                DataSettings = new DataSettings
                {
                    AllowedWordlistTypes = ["Default"]
                }
            },
            IsRemote = false,
            Mode = ConfigMode.Stack
        };
        var config2 = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig2",
                Author = "TestAuthor2",
                Category = "TestCategory2",
                CreationDate = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            Settings = new ConfigSettings
            {
                ProxySettings = new ProxySettings
                {
                    UseProxies = true
                },
                DataSettings = new DataSettings
                {
                    AllowedWordlistTypes = ["Default", "Credentials"]
                }
            },
            IsRemote = true,
            Mode = ConfigMode.LoliCode,
            LoliCodeScript = "int n = 1;"
        };
        configService.Configs.AddRange([config1, config2]);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<ConfigInfoDto>>(
            client, "/api/v1/config/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        var configs = result.Value.ToList();
        Assert.Equal(2, configs.Count);
        
        // The returned configs are sorted by last modified date (desc)
        Assert.Equal(config1.Id, configs[0].Id);
        Assert.Equal("TestConfig1", configs[0].Name);
        Assert.Equal("TestAuthor1", configs[0].Author);
        Assert.Equal("TestCategory1", configs[0].Category);
        Assert.Equal(config1.Metadata.CreationDate, configs[0].CreationDate);
        Assert.Equal(config1.Metadata.LastModified, configs[0].LastModified);
        Assert.False(configs[0].NeedsProxies);
        Assert.Collection(configs[0].AllowedWordlistTypes,
            x => Assert.Equal("Default", x));
        Assert.False(configs[0].IsRemote);
        Assert.Equal(ConfigMode.Stack, configs[0].Mode);
        Assert.False(configs[0].Dangerous);
        
        Assert.Equal(config2.Id, configs[1].Id);
        Assert.Equal("TestConfig2", configs[1].Name);
        Assert.Equal("TestAuthor2", configs[1].Author);
        Assert.Equal("TestCategory2", configs[1].Category);
        Assert.Equal(config2.Metadata.CreationDate, configs[1].CreationDate);
        Assert.Equal(config2.Metadata.LastModified, configs[1].LastModified);
        Assert.True(configs[1].NeedsProxies);
        Assert.Collection(configs[1].AllowedWordlistTypes,
            x => Assert.Equal("Default", x),
            x => Assert.Equal("Credentials", x));
        Assert.True(configs[1].IsRemote);
        Assert.Equal(ConfigMode.LoliCode, configs[1].Mode);
        Assert.True(configs[1].Dangerous);
    }
    
    /// <summary>
    /// Admin can read all configs' overview info when reloaded
    /// from the repository.
    /// </summary>
    [Fact]
    public async Task GetAll_Admin_Reload_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        var config1 = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig1",
                Author = "TestAuthor1",
                Category = "TestCategory1",
                CreationDate = new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            Settings = new ConfigSettings
            {
                ProxySettings = new ProxySettings { UseProxies = false },
                DataSettings = new DataSettings { AllowedWordlistTypes = ["Default"] }
            },
            IsRemote = false,
            Mode = ConfigMode.Stack
        };
        var config2 = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig2",
                Author = "TestAuthor2",
                Category = "TestCategory2",
                CreationDate = new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            Settings = new ConfigSettings
            {
                ProxySettings = new ProxySettings { UseProxies = false },
                DataSettings = new DataSettings { AllowedWordlistTypes = ["Default"] }
            },
            IsRemote = false,
            Mode = ConfigMode.Stack
        };
        configService.Configs.AddRange([config1, config2]);
        await configRepository.SaveAsync(config1);
        
        // Act
        var queryParams = new
        {
            reload = true
        };
        var result = await GetJsonAsync<IEnumerable<ConfigInfoDto>>(
            client, "/api/v1/config/all".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        var configs = result.Value.ToList();
        Assert.Single(configs);
        Assert.Equal(config1.Id, configs[0].Id);
    }
    
    /// <summary>
    /// Guest can read all configs' overview info.
    /// </summary>
    [Fact]
    public async Task GetAll_Guest_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var config = new Config { Id = Guid.NewGuid().ToString() };
        configService.Configs.Add(config);
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<ConfigInfoDto>>(
            client, "/api/v1/config/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        var configs = result.Value.ToList();
        Assert.Single(configs);
        Assert.Equal(config.Id, configs[0].Id);
    }
    
    /// <summary>
    /// Admin can get config's metadata.
    /// </summary>
    [Fact]
    public async Task GetMetadata_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        await AddTestPluginAsync();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig",
                Author = "TestAuthor",
                Category = "TestCategory",
                Base64Image = "abc"
            },
            LoliCodeScript = _additionScript,
            Mode = ConfigMode.LoliCode
        };
        configService.Configs.Add(config);
        await configRepository.SaveAsync(config);
        
        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var result = await GetJsonAsync<ConfigMetadataDto>(
            client, "/api/v1/config/metadata".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(config.Metadata.Name, result.Value.Name);
        Assert.Equal(config.Metadata.Author, result.Value.Author);
        Assert.Equal(config.Metadata.Category, result.Value.Category);
        Assert.Equal(config.Metadata.Base64Image, result.Value.Base64Image);
        Assert.Single(result.Value.Plugins);
        Assert.Contains("OB2TestPlugin", result.Value.Plugins[0]);
    }
    
    /// <summary>
    /// Guest can get config's metadata.
    /// </summary>
    [Fact]
    public async Task GetMetadata_Guest_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        await AddTestPluginAsync();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig",
                Author = "TestAuthor",
                Category = "TestCategory",
                Base64Image = "abc"
            },
            LoliCodeScript = _additionScript,
            Mode = ConfigMode.LoliCode
        };
        configService.Configs.Add(config);
        await configRepository.SaveAsync(config);

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var result = await GetJsonAsync<ConfigMetadataDto>(
            client, "/api/v1/config/metadata".ToUri(queryParams));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(config.Metadata.Name, result.Value.Name);
        Assert.Equal(config.Metadata.Author, result.Value.Author);
        Assert.Equal(config.Metadata.Category, result.Value.Category);
        Assert.Equal(config.Metadata.Base64Image, result.Value.Base64Image);
        Assert.Single(result.Value.Plugins);
        Assert.Contains("OB2TestPlugin", result.Value.Plugins[0]);
    }
    
    /// <summary>
    /// Admin can get config's readme
    /// </summary>
    [Fact]
    public async Task GetReadme_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            Readme = "This is a test readme"
        };
        configService.Configs.Add(config);

        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var result = await GetJsonAsync<ConfigReadmeDto>(
            client, "/api/v1/config/readme".ToUri(queryParams));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(config.Readme, result.Value.MarkdownText);
    }
    
    /// <summary>
    /// // Guest can get config's readme.
    /// </summary>
    [Fact]
    public async Task GetReadme_Guest_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            Readme = "This is a test readme"
        };
        configService.Configs.Add(config);

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var result = await GetJsonAsync<ConfigReadmeDto>(
            client, "/api/v1/config/readme".ToUri(queryParams));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(config.Readme, result.Value.MarkdownText);
    }
    
    /// <summary>
    /// Admin can get a config's full data.
    /// </summary>
    [Fact]
    public async Task Get_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            LoliCodeScript = "LOG \"Hello, world!\"",
            Mode = ConfigMode.LoliCode
        };
        configService.Configs.Add(config);

        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var result = await GetJsonAsync<ConfigDto>(
            client, "/api/v1/config".ToUri(queryParams));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(config.Id, result.Value.Id);
        Assert.Equal(config.Metadata.Name, result.Value.Metadata.Name);
        Assert.Equal(config.LoliCodeScript, result.Value.LoliCodeScript);
        Assert.Equal(config.Mode, result.Value.Mode);
    }
    
    /// <summary>
    /// Guest cannot get a config's full data (forbidden).
    /// </summary>
    [Fact]
    public async Task Get_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            LoliCodeScript = "LOG \"Hello, world!\"",
            Mode = ConfigMode.LoliCode
        };
        configService.Configs.Add(config);

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var result = await GetJsonAsync<ConfigDto>(
            client, "/api/v1/config".ToUri(queryParams));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Admin can update a config in LoliCode mode.
    /// </summary>
    [Fact]
    public async Task UpdateConfig_Admin_NotPersistent_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            LoliCodeScript = "LOG \"Hello, world!\"",
            Mode = ConfigMode.LoliCode
        };
        configService.Configs.Add(config);
        await configRepository.SaveAsync(config);

        // Act
        var dto = new UpdateConfigDto
        {
            Id = config.Id,
            Metadata = new UpdateConfigMetadataDto
            {
                Name = "UpdatedConfig"
            },
            Settings = new ConfigSettingsDto(),
            LoliCodeScript = "LOG \"Hello, world! 2\"",
            Mode = ConfigMode.LoliCode,
            Persistent = false
        };
        var result = await PutJsonAsync<ConfigDto>(
            client, "/api/v1/config", dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dto.Id, result.Value.Id);
        Assert.Equal(dto.Metadata.Name, result.Value.Metadata.Name);
        Assert.Equal(dto.LoliCodeScript, result.Value.LoliCodeScript);
        Assert.Equal(dto.Mode, result.Value.Mode);

        // Reload the config from the repository and make sure it was NOT updated
        var reloadedConfig = await configRepository.GetAsync(config.Id);
        Assert.Equal("TestConfig", reloadedConfig.Metadata.Name);

        // Make sure the stack was converted
        Assert.Collection(config.Stack,
            x => Assert.Equal(
                "LOG \"Hello, world! 2\"", ((LoliCodeBlockInstance)x).Script));
    }
    
    /// <summary>
    /// Admin can update a config in LoliCode mode, with persistency.
    /// </summary>
    [Fact]
    public async Task UpdateConfig_Admin_Persistent_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            LoliCodeScript = "LOG \"Hello, world!\"",
            Mode = ConfigMode.LoliCode
        };
        configService.Configs.Add(config);
        await configRepository.SaveAsync(config);

        // Act
        var dto = new UpdateConfigDto
        {
            Id = config.Id,
            Metadata = new UpdateConfigMetadataDto
            {
                Name = "UpdatedConfig"
            },
            Settings = new ConfigSettingsDto(),
            LoliCodeScript = "LOG \"Hello, world! 2\"",
            Mode = ConfigMode.LoliCode,
            Persistent = true
        };
        var result = await PutJsonAsync<ConfigDto>(
            client, "/api/v1/config", dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dto.Id, result.Value.Id);
        Assert.Equal(dto.Metadata.Name, result.Value.Metadata.Name);
        Assert.Equal(dto.LoliCodeScript, result.Value.LoliCodeScript);
        Assert.Equal(dto.Mode, result.Value.Mode);

        // Reload the config from the repository and check if it was updated
        var reloadedConfig = await configRepository.GetAsync(config.Id);

        Assert.Equal(dto.Metadata.Name, reloadedConfig.Metadata.Name);
        Assert.Equal(dto.LoliCodeScript, reloadedConfig.LoliCodeScript);
        Assert.Equal(dto.Mode, reloadedConfig.Mode);

        // Make sure the stack was converted
        Assert.Collection(config.Stack,
            x => Assert.Equal(
                "LOG \"Hello, world! 2\"", ((LoliCodeBlockInstance)x).Script));
    }
    
    /// <summary>
    /// Guest cannot update a config (forbidden).
    /// </summary>
    [Fact]
    public async Task UpdateConfig_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            LoliCodeScript = "LOG \"Hello, world!\"",
            Mode = ConfigMode.LoliCode
        };
        configService.Configs.Add(config);

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var dto = new UpdateConfigDto
        {
            Id = config.Id,
            Metadata = new UpdateConfigMetadataDto
            {
                Name = "UpdatedConfig"
            },
            Settings = new ConfigSettingsDto(),
            LoliCodeScript = "LOG \"Hello, world! 2\"",
            Mode = ConfigMode.LoliCode,
            Persistent = true
        };
        var result = await PutJsonAsync<ConfigDto>(
            client, "/api/v1/config", dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Admin cannot update a remote config.
    /// </summary>
    [Fact]
    public async Task UpdateConfig_Admin_Remote_NotAllowed()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            LoliCodeScript = "LOG \"Hello, world!\"",
            Mode = ConfigMode.LoliCode,
            IsRemote = true
        };
        configService.Configs.Add(config);

        // Act
        var dto = new UpdateConfigDto
        {
            Id = config.Id,
            Metadata = new UpdateConfigMetadataDto
            {
                Name = "UpdatedConfig"
            },
            Settings = new ConfigSettingsDto(),
            LoliCodeScript = "LOG \"Hello, world! 2\"",
            Mode = ConfigMode.LoliCode,
            Persistent = true
        };
        var result = await PutJsonAsync<ConfigDto>(
            client, "/api/v1/config", dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.ActionNotAllowedForRemoteConfig, result.Error.Content.ErrorCode);
    }
    
    // We are missing the display of the X-Application-Warning header in the frontend
    [Fact]
    public async Task UpdateConfig_Admin_MissingPlugin_Warning()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig",
                Plugins = ["MissingPlugin"]
            }
        };
        configService.Configs.Add(config);

        // Act
        var dto = new UpdateConfigDto
        {
            Id = config.Id,
            Metadata = new UpdateConfigMetadataDto
            {
                Name = "UpdatedConfig"
            },
            Settings = new ConfigSettingsDto()
        };
        var json = JsonSerializer.Serialize(dto, JsonSerializerOptions);
        var response = await client.PutAsync(
            "/api/v1/config", new StringContent(json, Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var warning = response.Headers.GetValues("X-Application-Warning").FirstOrDefault();
        Assert.NotNull(warning);
        Assert.Contains("MissingPlugin", warning);
    }
    
    [Fact]
    public async Task UpdateConfig_Admin_InvalidLoliCode_BadRequest()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig",
                Plugins = ["MissingPlugin"]
            }
        };
        configService.Configs.Add(config);

        // Act
        var dto = new UpdateConfigDto
        {
            Id = config.Id,
            Metadata = new UpdateConfigMetadataDto
            {
                Name = "UpdatedConfig"
            },
            Settings = new ConfigSettingsDto(),
            LoliCodeScript = "BLOCK:ThisIsInvalid\n  abc"
        };
        var result = await PutJsonAsync<ConfigDto>(
            client, "/api/v1/config", dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        // TODO: Needs a more talking error
    }
    
    /// <summary>
    /// Admin can create a config.
    /// </summary>
    [Fact]
    public async Task CreateConfig_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        
        // Act
        var result = await PostJsonAsync<ConfigDto>(
            client, "/api/v1/config", new { });

        // Assert
        Assert.True(result.IsSuccess);
        var config = result.Value;
        Assert.NotNull(config.Id);

        // Make sure the config was created both in the service and in the repository
        var createdConfig = await configRepository.GetAsync(config.Id);
        Assert.NotNull(createdConfig);
        Assert.Equal(config.Id, createdConfig.Id);

        var createdConfigInService = configService.Configs.Find(c => c.Id == config.Id);
        Assert.NotNull(createdConfigInService);
    }
    
    /// <summary>
    /// Guest cannot create a config (forbidden).
    /// </summary>
    [Fact]
    public async Task CreateConfig_Guest_Forbidden()
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
        var result = await PostJsonAsync<ConfigDto>(
            client, "/api/v1/config", new { });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// // Admin can delete a config.
    /// </summary>
    [Fact]
    public async Task DeleteConfig_Admin_Success()
    {
        // Arrange
        var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            }
        };
        configService.Configs.Add(config);
        await configRepository.SaveAsync(config);

        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var error = await DeleteAsync(client,
            "/api/v1/config".ToUri(queryParams));

        // Assert
        Assert.Null(error);
        Assert.Empty(await configRepository.GetAllAsync());
        Assert.Empty(configService.Configs);
    }
    
    /// <summary>
    /// Guest cannot delete a config (forbidden).
    /// </summary>
    [Fact]
    public async Task DeleteConfig_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            }
        };
        configService.Configs.Add(config);
        await configRepository.SaveAsync(config);

        RequireLogin();
        ImpersonateGuest(client, guest);

        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var error = await DeleteAsync(client,
                       "/api/v1/config".ToUri(queryParams));

        // Assert
        Assert.NotNull(error);
        Assert.NotNull(error.Content);
        Assert.Equal(ErrorCode.NotAdmin, error.Content.ErrorCode);
        Assert.NotNull(await configRepository.GetAsync(config.Id));
        Assert.NotNull(configService.Configs.Find(c => c.Id == config.Id));
    }
    
    /// <summary>
    /// Admin can clone a config.
    /// </summary>
    [Fact]
    public async Task CloneConfig_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            Settings = new ConfigSettings
            {
                ProxySettings = new ProxySettings
                {
                    UseProxies = false
                },
                DataSettings = new DataSettings
                {
                    AllowedWordlistTypes = ["Default"]
                }
            },
            LoliCodeScript = _additionScript,
            Mode = ConfigMode.LoliCode
        };
        configService.Configs.Add(config);
        await configRepository.SaveAsync(config);
        
        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var result = await PostJsonAsync<ConfigDto>(
            client, "/api/v1/config/clone".ToUri(queryParams), new { });
        
        // Assert
        Assert.True(result.IsSuccess);
        var clonedConfig = result.Value;
        Assert.NotEqual(config.Id, clonedConfig.Id);
        Assert.Equal(config.Metadata.Name + " (Cloned)", clonedConfig.Metadata.Name);
        Assert.Equal(config.Settings.ProxySettings.UseProxies, clonedConfig.Settings.ProxySettings.UseProxies);
        Assert.Equal(config.Settings.DataSettings.AllowedWordlistTypes, clonedConfig.Settings.DataSettings.AllowedWordlistTypes);
        Assert.Equal(config.LoliCodeScript, clonedConfig.LoliCodeScript);
        Assert.Equal(config.Mode, clonedConfig.Mode);
        
        // Make sure the cloned config was created both in the service and in the repository
        var createdConfig = await configRepository.GetAsync(clonedConfig.Id);
        Assert.NotNull(createdConfig);
        
        var createdConfigInService = configService.Configs.Find(c => c.Id == clonedConfig.Id);
        Assert.NotNull(createdConfigInService);
    }
    
    /// <summary>
    /// Admin cannot clone a remote config.
    /// </summary>
    [Fact]
    public async Task CloneConfig_Admin_Remote_NotAllowed()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            IsRemote = true
        };
        configService.Configs.Add(config);
        
        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var error = await PostAsync(client,
            "/api/v1/config/clone".ToUri(queryParams), new { });

        // Assert
        Assert.NotNull(error);
        Assert.NotNull(error.Content);
        Assert.Equal(ErrorCode.ActionNotAllowedForRemoteConfig, error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Guest cannot clone a config (forbidden).
    /// </summary>
    [Fact]
    public async Task CloneConfig_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            }
        };
        configService.Configs.Add(config);
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var error = await PostAsync(client,
            "/api/v1/config/clone".ToUri(queryParams), new { });

        // Assert
        Assert.NotNull(error);
        Assert.NotNull(error.Content);
        Assert.Equal(ErrorCode.NotAdmin, error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Admin can download a config.
    /// </summary>
    [Fact]
    public async Task DownloadConfig_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            }
        };
        configService.Configs.Add(config);
        
        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var response = await client.GetAsync(
            "/api/v1/config/download".ToUri(queryParams));
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("application/octet-stream", response.Content.Headers.ContentType!.MediaType);
        
        var downloadedConfig = await ConfigPacker.UnpackAsync(
            await response.Content.ReadAsStreamAsync());

        // The id will not be the same since it's not saved inside the
        // config but it's the name of the file
        Assert.Equal(config.Metadata.Name, downloadedConfig.Metadata.Name);
    }
    
    /// <summary>
    /// Admin cannot download a remote config.
    /// </summary>
    [Fact]
    public async Task DownloadConfig_Admin_Remote_NotAllowed()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            IsRemote = true
        };
        configService.Configs.Add(config);
        
        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var response = await client.GetAsync(
            "/api/v1/config/download".ToUri(queryParams));
        
        // Assert
        Assert.False(response.IsSuccessStatusCode);
    }
    
    /// <summary>
    /// Guest cannot download a config.
    /// </summary>
    [Fact]
    public async Task DownloadConfig_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            }
        };
        configService.Configs.Add(config);
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = config.Id
        };
        var response = await client.GetAsync(
            "/api/v1/config/download".ToUri(queryParams));
        
        // Assert
        Assert.False(response.IsSuccessStatusCode);
    }
    
    /// <summary>
    /// Admin can download all configs (except remote).
    /// </summary>
    [Fact]
    public async Task DownloadAllConfigs_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        var config1 = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig1"
            }
        };
        var config2 = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig2"
            },
            IsRemote = true
        };
        configService.Configs.AddRange([config1, config2]);
        await configRepository.SaveAsync(config1);
        await configRepository.SaveAsync(config2);
        
        // Act
        var response = await client.GetAsync("/api/v1/config/download/all");
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("application/zip", response.Content.Headers.ContentType!.MediaType);
        
        await using var zipStream = await response.Content.ReadAsStreamAsync();
        var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        Assert.Single(zipArchive.Entries);
        Assert.Equal("TestConfig1.opk", zipArchive.Entries[0].Name);
    }
    
    /// <summary>
    /// Guest cannot download all configs (forbidden).
    /// </summary>
    [Fact]
    public async Task DownloadAllConfigs_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        var config1 = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig1"
            }
        };
        var config2 = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig2"
            }
        };
        configService.Configs.AddRange([config1, config2]);
        await configRepository.SaveAsync(config1);
        await configRepository.SaveAsync(config2);
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var response = await client.GetAsync("/api/v1/config/download/all");
        
        // Assert
        Assert.False(response.IsSuccessStatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error);
        Assert.Equal(ErrorCode.NotAdmin, error.ErrorCode);
    }
    
    /// <summary>
    /// Admin can upload configs (with all kinds of modes).
    /// </summary>
    [Fact]
    public async Task UploadConfig_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();
        var loliCodeConfig = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            LoliCodeScript = "LOG \"Hello, world!\"",
            Mode = ConfigMode.LoliCode
        };
        var cSharpConfig = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            CSharpScript = "using System; public class TestConfig { public void Main() { Console.WriteLine(\"Hello, world!\"); } }",
            Mode = ConfigMode.CSharp
        };
        var legacyConfig = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            LoliScript = "LOG \"Hello, world!\"",
            Mode = ConfigMode.Legacy
        };
        var dllConfig = new Config
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata
            {
                Name = "TestConfig"
            },
            DLLBytes = [0x00, 0x01, 0x02],
            Mode = ConfigMode.DLL
        };
        configService.Configs.AddRange([loliCodeConfig, cSharpConfig, legacyConfig, dllConfig]);
        
        // Act
        var packedLoliCodeConfig = await ConfigPacker.PackAsync(loliCodeConfig);
        var packedCSharpConfig = await ConfigPacker.PackAsync(cSharpConfig);
        var packedLegacyConfig = await ConfigPacker.PackAsync(legacyConfig);
        var packedDllConfig = await ConfigPacker.PackAsync(dllConfig);
        var formData = new MultipartFormDataContent
        {
            { new ByteArrayContent(packedLoliCodeConfig), "files", "TestConfig1.opk" },
            { new ByteArrayContent(packedCSharpConfig), "files", "TestConfig2.opk" },
            { new ByteArrayContent(packedLegacyConfig), "files", "TestConfig3.opk" },
            { new ByteArrayContent(packedDllConfig), "files", "TestConfig4.opk" }
        };
        var response = await client.PostAsync("/api/v1/config/upload/many", formData);
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var affectedEntries = await response.Content.ReadFromJsonAsync<AffectedEntriesDto>(JsonSerializerOptions);
        Assert.NotNull(affectedEntries);
        Assert.Equal(4, affectedEntries.Count);
        
        var configs = await configRepository.GetAllAsync();
        Assert.Equal(4, configs.Count());
    }
    
    /// <summary>
    /// Guest cannot upload configs (forbidden).
    /// </summary>
    [Fact]
    public async Task UploadConfig_Guest_Forbidden()
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
        var response = await client.PostAsync("/api/v1/config/upload/many",
            new MultipartFormDataContent());
        
        // Assert
        Assert.False(response.IsSuccessStatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error);
        Assert.Equal(ErrorCode.NotAdmin, error.ErrorCode);
    }
    
    /// <summary>
    /// Admin can convert LoliCode to C#.
    /// </summary>
    [Fact]
    public async Task ConvertLoliCodeToCSharp_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();

        // Act
        var dto = new ConvertLoliCodeToCSharpDto
        {
            LoliCode = "LOG \"Hello, world!\"",
            Settings = new ConfigSettingsDto()
        };
        var result = await PostJsonAsync<ConvertedCSharpDto>(
            client, "/api/v1/config/convert/lolicode/csharp", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("data.Logger.LogObject(\"Hello, world!\");",
            result.Value.CSharpScript);
    }
    
    [Fact]
    public async Task ConvertLoliCodeToCSharp_Admin_InvalidLoliCode_BadRequest()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var dto = new ConvertLoliCodeToCSharpDto
        {
            LoliCode = "BLOCK:ThisIsInvalid\n  abc",
            Settings = new ConfigSettingsDto()
        };
        var result = await PostJsonAsync<ConvertedCSharpDto>(
            client, "/api/v1/config/convert/lolicode/csharp", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        
        // TODO: This needs a more talking error
    }
    
    /// <summary>
    /// Guest cannot convert LoliCode to C# (forbidden).
    /// </summary>
    [Fact]
    public async Task ConvertLoliCodeToCSharp_Guest_Forbidden()
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
        var dto = new ConvertLoliCodeToCSharpDto
        {
            LoliCode = "LOG \"Hello, world!\"",
            Settings = new ConfigSettingsDto()
        };
        var result = await PostJsonAsync<ConvertedCSharpDto>(
            client, "/api/v1/config/convert/lolicode/csharp", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Admin can convert LoliCode to Stack.
    /// </summary>
    [Fact]
    public async Task ConvertLoliCodeToStack_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var dto = new ConvertLoliCodeToStackDto
        {
            LoliCode = "LOG \"Hello, world!\""
        };
        var result = await PostJsonAsync<ConvertedStackDto>(
            client, "/api/v1/config/convert/lolicode/stack", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Collection(result.Value.Stack,
            x => Assert.Equal("LOG \"Hello, world!\"", ((JsonElement)x).GetProperty("script").GetString()));
    }
    
    [Fact]
    public async Task ConvertLoliCodeToStack_Admin_InvalidLoliCode_BadRequest()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var dto = new ConvertLoliCodeToStackDto
        {
            LoliCode = "BLOCK:ThisIsInvalid\n  abc"
        };
        var result = await PostJsonAsync<ConvertedStackDto>(
            client, "/api/v1/config/convert/lolicode/stack", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
    }
    
    /// <summary>
    /// Guest cannot convert LoliCode to Stack (forbidden).
    /// </summary>
    [Fact]
    public async Task ConvertLoliCodeToStack_Guest_Forbidden()
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
        var dto = new ConvertLoliCodeToStackDto
        {
            LoliCode = "LOG \"Hello, world!\""
        };
        var result = await PostJsonAsync<ConvertedStackDto>(
            client, "/api/v1/config/convert/lolicode/stack", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Admin can convert Stack to LoliCode.
    /// </summary>
    [Fact]
    public async Task ConvertStackToLoliCode_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var block = new 
        {
            Script = "LOG \"Hello, world!\"",
            Id = "loliCode",
            Disabled = false,
            Label = "Log",
            Settings = new { },
            Type = "loliCode"
        };
        var dto = new ConvertStackToLoliCodeDto
        {
            Stack = [
                JsonSerializer.SerializeToElement(block, JsonSerializerOptions)
            ]
        };
        var result = await PostJsonAsync<ConvertedLoliCodeDto>(
            client, "/api/v1/config/convert/stack/lolicode", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("LOG \"Hello, world!\"", 
            result.Value.LoliCode.Trim('\n', '\r', ' '));
    }
    
    /// <summary>
    /// Guest cannot convert Stack to LoliCode (forbidden).
    /// </summary>
    [Fact]
    public async Task ConvertStackToLoliCode_Guest_Forbidden()
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
        var block = new
        {
            Script = "LOG \"Hello, world!\"",
            Id = "loliCode",
            Disabled = false,
            Label = "Log",
            Settings = new { },
            Type = "loliCode"
        };
        var dto = new ConvertStackToLoliCodeDto
        {
            Stack = [
                JsonSerializer.SerializeToElement(block, JsonSerializerOptions)
            ]
        };
        var result = await PostJsonAsync<ConvertedLoliCodeDto>(
            client, "/api/v1/config/convert/stack/lolicode", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Admin can get all block descriptors, including ones from plugins.
    /// </summary>
    [Fact]
    public async Task GetAllBlockDescriptors_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        await AddTestPluginAsync();
        
        // Act
        var result = await GetJsonAsync<Dictionary<string, BlockDescriptorDto>>(
            client, "/api/v1/config/block-descriptors");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        Assert.Contains("Parse", result.Value.Keys);
        Assert.Contains("Addition", result.Value.Keys);
    }
    
    /// <summary>
    /// Guest cannot get all block descriptors.
    /// </summary>
    [Fact]
    public async Task GetAllBlockDescriptors_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        await AddTestPluginAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<Dictionary<string, BlockDescriptorDto>>(
            client, "/api/v1/config/block-descriptors");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Admin can get the category tree, including plugins.
    /// </summary>
    [Fact]
    public async Task GetCategoryTree_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        await AddTestPluginAsync();
        
        // Act
        var result = await GetJsonAsync<CategoryTreeNodeDto>(
            client, "/api/v1/config/category-tree");
        
        // Assert
        Assert.True(result.IsSuccess);
        var root = result.Value;
        Assert.Contains("Parse",
            root.SubCategories
                .First(sc => sc.Name == "RuriLib")
                .SubCategories.First(sc => sc.Name == "Blocks")
                .SubCategories.First(sc => sc.Name == "Parsing")
                .DescriptorIds);
        Assert.Contains("Addition",
            root.SubCategories
                .First(sc => sc.Name == "OB2TestPlugin")
                .SubCategories.First(sc => sc.Name == "Blocks")
                .SubCategories.First(sc => sc.Name == "Functions")
                .DescriptorIds);
    }
    
    /// <summary>
    /// Guest cannot get the category tree.
    /// </summary>
    [Fact]
    public async Task GetCategoryTree_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        await AddTestPluginAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<CategoryTreeNodeDto>(
            client, "/api/v1/config/category-tree");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Admin can get a new block instance.
    /// </summary>
    [Fact]
    public async Task GetBlockInstance_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var queryParams = new
        {
            id = "ConstantString"
        };
        var result = await GetJsonAsync<BlockInstanceDto>(
            client, "/api/v1/config/block-instance".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("ConstantString", result.Value.Id);
        Assert.Contains("value", result.Value.Settings.Keys);
    }
    
    [Fact]
    public async Task GetBlockInstance_Admin_InvalidId_NotFound()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var queryParams = new
        {
            id = "InvalidId"
        };
        var result = await GetJsonAsync<BlockInstanceDto>(
            client, "/api/v1/config/block-instance".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.InvalidBlockId, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Guest cannot get a new block instance (forbidden).
    /// </summary>
    [Fact]
    public async Task GetBlockInstance_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new
        {
            id = "ConstantString"
        };
        var result = await GetJsonAsync<BlockInstanceDto>(
            client, "/api/v1/config/block-instance".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    /// <summary>
    /// Admin can get block snippets.
    /// </summary>
    [Fact]
    public async Task GetBlockSnippets_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var result = await GetJsonAsync<Dictionary<string, string>>(
            client, "/api/v1/config/block-snippets");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        Assert.Contains("ConstantString", result.Value.Keys);
    }
    
    /// <summary>
    /// Guest cannot get block snippets.
    /// </summary>
    [Fact]
    public async Task GetBlockSnippets_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<Dictionary<string, string>>(
            client, "/api/v1/config/block-snippets");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }

    /// <summary>
    /// Admin can debug a config.
    /// </summary>
    [Fact]
    public async Task Debug_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var configService = GetRequiredService<ConfigService>();
        var config = new Config {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata(),
            Settings = new ConfigSettings
            {
                DataSettings = new DataSettings
                {
                    AllowedWordlistTypes = ["Default"]
                }
            },
            IsRemote = false,
            Mode = ConfigMode.LoliCode,
            LoliCodeScript = "LOG \"Hello, World!\"\nSET VAR \"TEST\" @input.DATA"
        };
        configService.Configs.Add(config);
        
        var dto = new DebugConfigDto
        {
            ConfigId = config.Id,
            TestData = "Test data",
            WordlistType = "Default",
        };
        
        // Act
        var result = await PostJsonAsync<DebugConfigResultDto>(
            client, "/api/v1/config/debug", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        
        var variables = result.Value.Variables.ToList();

        Assert.Single(variables);

        var variable = variables[0];
        
        Assert.Equal("TEST", variable.Name);
        Assert.IsType<JsonElement>(variable.Value);
        
        var jsonElement = (JsonElement)variable.Value!;
        
        Assert.Equal("Test data", jsonElement.GetString());
        
        Assert.Contains(result.Value.Log, l => l.Message.Contains("Hello, World!"));
    }

    /// <summary>
    /// Guest cannot debug a config (forbidden).
    /// </summary>
    [Fact]
    public async Task Debug_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        var dbContext = GetRequiredService<ApplicationDbContext>();
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        var dto = new DebugConfigDto
        {
            ConfigId = Guid.NewGuid().ToString(),
        };
        
        // Act
        var result = await PostJsonAsync<DebugConfigResultDto>(
            client, "/api/v1/config/debug", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Content);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content.ErrorCode);
    }
    
    private async Task AddTestPluginAsync() {
        var pluginRepo = GetRequiredService<PluginRepository>();
        var file = new FileInfo("Resources/OB2TestPlugin.zip");
        await using var fs = file.OpenRead();
        pluginRepo.AddPlugin(fs);
    }
}
