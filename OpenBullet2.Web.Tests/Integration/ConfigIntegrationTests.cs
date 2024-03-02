using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Dtos.Config.Settings;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Tests.Extensions;
using RuriLib.Models.Blocks;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Services;
using System.Text;
using System.Text.Json;
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
                CreationDate = new DateTime(2021, 1, 2),
                LastModified = new DateTime(2021, 1, 2)
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
                CreationDate = new DateTime(2021, 1, 1),
                LastModified = new DateTime(2021, 1, 1)
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
            Mode = ConfigMode.LoliCode
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
                CreationDate = new DateTime(2021, 1, 2),
                LastModified = new DateTime(2021, 1, 2)
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
                CreationDate = new DateTime(2021, 1, 2),
                LastModified = new DateTime(2021, 1, 2)
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
        var guest = new GuestEntity { Username = "guest" };
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
        var guest = new GuestEntity { Username = "guest" };
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
        var guest = new GuestEntity { Username = "guest" };
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
        var guest = new GuestEntity { Username = "guest" };
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
        var guest = new GuestEntity { Username = "guest" };
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
            LoliCodeScript = "BLOCK:ThisIsInvalid\n  abc",
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

        var createdConfigInService = configService.Configs.FirstOrDefault(c => c.Id == config.Id);
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
        var guest = new GuestEntity { Username = "guest" };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        var configService = GetRequiredService<ConfigService>();
        var configRepository = GetRequiredService<IConfigRepository>();

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
        var guest = new GuestEntity { Username = "guest" };
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
        Assert.NotNull(configService.Configs.FirstOrDefault(c => c.Id == config.Id));
    }

    // Admin can clone a config

    // Admin cannot clone a remote config

    // Guest cannot clone a config (forbidden)

    // Admin can download a config

    // Admin cannot download a remote config

    // Guest cannot download a config

    // Admin can download all configs (except remote)

    // Guest cannot download all configs

    // Admin can upload configs (with all kinds of modes)

    // Guest cannot upload configs

    // Admin can convert LoliCode to C#

    // What happens if LoliCode is invalid?

    // Guest cannot convert LoliCode to C#

    // Admin can convert LoliCode to Stack

    // What happens if LoliCode is invalid?

    // Guest cannot convert LoliCode to Stack

    // Admin can convert Stack to LoliCode

    // Guest cannot convert Stack to LoliCode

    // Admin can get all block descriptors, including ones from plugins

    // Guest cannot get all block descriptors

    // Admin can get the category tree, including plugins

    // Guest cannot get the category tree

    // Admin can get a new block instance

    // Guest cannot get a new block instance

    private async Task AddTestPluginAsync() {
        var pluginRepo = GetRequiredService<PluginRepository>();
        var file = new FileInfo("Resources/OB2TestPlugin.zip");
        await using var fs = file.OpenRead();
        pluginRepo.AddPlugin(fs);
    }
}
