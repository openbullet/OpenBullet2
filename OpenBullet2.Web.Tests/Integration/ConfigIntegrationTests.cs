using OpenBullet2.Core;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Tests.Extensions;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class ConfigIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
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
            Metadata = new ConfigMetadata {
                Name = "TestConfig1",
                Author = "TestAuthor1",
                Category = "TestCategory1",
                CreationDate = new DateTime(2021, 1, 2),
                LastModified = new DateTime(2021, 1, 2)
            },
            Settings = new ConfigSettings {
                ProxySettings = new ProxySettings {
                    UseProxies = false
                },
                DataSettings = new DataSettings {
                    AllowedWordlistTypes = ["Default"]
                }
            },
            IsRemote = false,
            Mode = ConfigMode.Stack
        };
        var config2 = new Config {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata {
                Name = "TestConfig2",
                Author = "TestAuthor2",
                Category = "TestCategory2",
                CreationDate = new DateTime(2021, 1, 1),
                LastModified = new DateTime(2021, 1, 1)
            },
            Settings = new ConfigSettings {
                ProxySettings = new ProxySettings {
                    UseProxies = true
                },
                DataSettings = new DataSettings {
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
        var config1 = new Config {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata {
                Name = "TestConfig1",
                Author = "TestAuthor1",
                Category = "TestCategory1",
                CreationDate = new DateTime(2021, 1, 2),
                LastModified = new DateTime(2021, 1, 2)
            },
            Settings = new ConfigSettings {
                ProxySettings = new ProxySettings { UseProxies = false },
                DataSettings = new DataSettings { AllowedWordlistTypes = ["Default"] }
            },
            IsRemote = false,
            Mode = ConfigMode.Stack
        };
        var config2 = new Config {
            Id = Guid.NewGuid().ToString(),
            Metadata = new ConfigMetadata {
                Name = "TestConfig2",
                Author = "TestAuthor2",
                Category = "TestCategory2",
                CreationDate = new DateTime(2021, 1, 2),
                LastModified = new DateTime(2021, 1, 2)
            },
            Settings = new ConfigSettings {
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

    // Guest can read all configs' overview info
    
    // Admin can get config's metadata
    
    // Guest can get config's metadata
    
    // Admin can get config's readme
    
    // Guest can get config's readme
    
    // Admin can update a config in LoliCode mode (make sure the
    // stack is updated accordingly)
    
    // Guest cannot update a config (forbidden)
    
    // Admin cannot update a remote config
    
    // We are missing the display of the X-Application-Warning header in the frontend
    
    // Check update if the LoliCode is invalid
    
    // Check with persistent true
    
    // Admin can create a config
    
    // Guest cannot create a config (forbidden)
    
    // Admin can delete a config
    
    // Guest cannot delete a config (forbidden)
    
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
}
