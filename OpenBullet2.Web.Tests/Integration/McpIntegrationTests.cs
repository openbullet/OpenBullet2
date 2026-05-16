using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Dtos.Config.Settings;
using Xunit;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class McpIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task McpEndpoint_ExposesSampleTool()
    {
        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var tools = await client.ListToolsAsync(cancellationToken: TestCancellationToken);

        Assert.Contains(tools, tool => tool.Name == "get_server_info");
        Assert.Contains(tools, tool => tool.Name == "get_environment");
        Assert.Contains(tools, tool => tool.Name == "list_configs");
        Assert.Contains(tools, tool => tool.Name == "get_config_readme");
        Assert.Contains(tools, tool => tool.Name == "update_config_readme");
        Assert.Contains(tools, tool => tool.Name == "get_config_settings");
        Assert.Contains(tools, tool => tool.Name == "update_config_settings");
    }

    [Fact]
    public async Task McpEndpoint_CallsSampleTool()
    {
        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_server_info",
            cancellationToken: TestCancellationToken);

        var version = GetRequiredService<IUpdateService>().CurrentVersion;
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

        Assert.False(result.IsError ?? false);
        Assert.Contains("OpenBullet 2 server info", text);
        Assert.Contains(version.ToString(), text);
        Assert.Contains(Path.GetFullPath(UserDataFolder), text);
    }

    [Fact]
    public async Task McpEndpoint_CallsEnvironmentTool()
    {
        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_environment",
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

        Assert.False(result.IsError ?? false);
        Assert.Contains("Environment.ini", text);
        Assert.Contains(Path.GetFullPath(Path.Combine(UserDataFolder, "Environment.ini")), text);
        Assert.Contains("[WORDLIST TYPE]", text);
        Assert.Contains("Name=Default", text);
    }

    [Fact]
    public async Task McpEndpoint_ListsAvailableConfigs()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Metadata.Name = "MCP Test Config";
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "list_configs",
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

        Assert.False(result.IsError ?? false);
        Assert.Contains(config.Id, text);
        Assert.Contains(config.Metadata.Name, text);
        Assert.Contains(config.Metadata.LastModified.ToString("O"), text);
    }

    [Fact]
    public async Task McpEndpoint_GetsConfigReadme()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Readme = "## MCP Test Readme";
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_config_readme",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var dto = System.Text.Json.JsonSerializer.Deserialize<ConfigReadmeDto>(
            text, JsonSerializerOptions);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(dto);
        Assert.Equal(config.Readme, dto.MarkdownText);
    }

    [Fact]
    public async Task McpEndpoint_UpdatesConfigReadme()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Readme = "Old readme";
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "update_config_readme",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id,
                ["readme"] = new Dictionary<string, object?>
                {
                    ["markdownText"] = "Updated MCP readme"
                }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(
            text, JsonSerializerOptions);
        var reloadedConfig = await configRepo.GetAsync(config.Id);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(response);
        Assert.True(response["updated"]);
        Assert.Equal("Updated MCP readme", config.Readme);
        Assert.Equal("Updated MCP readme", reloadedConfig.Readme);
    }

    [Fact]
    public async Task McpEndpoint_GetsConfigSettings()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Settings.ProxySettings.UseProxies = true;
        config.Settings.GeneralSettings.SuggestedBots = 42;
        config.Settings.DataSettings.AllowedWordlistTypes = ["Credentials"];
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_config_settings",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var dto = System.Text.Json.JsonSerializer.Deserialize<ConfigSettingsDto>(
            text, JsonSerializerOptions);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(dto);
        Assert.True(dto.ProxySettings.UseProxies);
        Assert.Equal(42, dto.GeneralSettings.SuggestedBots);
        Assert.Equal(["Credentials"], dto.DataSettings.AllowedWordlistTypes);
    }

    [Fact]
    public async Task McpEndpoint_UpdatesConfigSettings()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        config.Settings.ProxySettings.UseProxies = false;
        config.Settings.GeneralSettings.SuggestedBots = 10;
        config.Settings.DataSettings.AllowedWordlistTypes = ["Default"];
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "update_config_settings",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id,
                ["settings"] = new Dictionary<string, object?>
                {
                    ["generalSettings"] = new Dictionary<string, object?>
                    {
                        ["suggestedBots"] = 99
                    },
                    ["proxySettings"] = new Dictionary<string, object?>
                    {
                        ["useProxies"] = true
                    },
                    ["inputSettings"] = new Dictionary<string, object?>(),
                    ["dataSettings"] = new Dictionary<string, object?>
                    {
                        ["allowedWordlistTypes"] = new[] { "Credentials" }
                    },
                    ["browserSettings"] = new Dictionary<string, object?>(),
                    ["scriptSettings"] = new Dictionary<string, object?>()
                }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(
            text, JsonSerializerOptions);
        var reloadedConfig = await configRepo.GetAsync(config.Id);

        Assert.False(result.IsError ?? false);
        Assert.NotNull(response);
        Assert.True(response["updated"]);
        Assert.True(config.Settings.ProxySettings.UseProxies);
        Assert.Equal(99, config.Settings.GeneralSettings.SuggestedBots);
        Assert.Equal(["Credentials"], config.Settings.DataSettings.AllowedWordlistTypes);
        Assert.True(reloadedConfig.Settings.ProxySettings.UseProxies);
        Assert.Equal(99, reloadedConfig.Settings.GeneralSettings.SuggestedBots);
        Assert.Equal(["Credentials"], reloadedConfig.Settings.DataSettings.AllowedWordlistTypes);
    }

    [Fact]
    public async Task McpEndpoint_UpdateConfigSettings_InvalidSettings_ReturnsValidationError()
    {
        var configRepo = GetRequiredService<IConfigRepository>();
        var configService = GetRequiredService<ConfigService>();

        var config = await configRepo.CreateAsync();
        await configRepo.SaveAsync(config);
        configService.Configs.Add(config);

        using var httpClient = Factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "update_config_settings",
            new Dictionary<string, object?>
            {
                ["configId"] = config.Id,
                ["settings"] = new Dictionary<string, object?>
                {
                    ["generalSettings"] = new Dictionary<string, object?>
                    {
                        ["suggestedBots"] = -1
                    },
                    ["proxySettings"] = new Dictionary<string, object?>(),
                    ["inputSettings"] = new Dictionary<string, object?>(),
                    ["dataSettings"] = new Dictionary<string, object?>
                    {
                        ["allowedWordlistTypes"] = Array.Empty<string>()
                    },
                    ["browserSettings"] = new Dictionary<string, object?>(),
                    ["scriptSettings"] = new Dictionary<string, object?>()
                }
            },
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

        Assert.True(result.IsError ?? false);
        Assert.Contains("SuggestedBots must be greater than or equal to 0.", text);
        Assert.Contains("AllowedWordlistTypes must contain at least one value.", text);
    }

    [Fact]
    public async Task McpEndpoint_WhenLoginRequired_AnonymousClientIsRejected()
    {
        RequireLogin();

        using var client = Factory.CreateClient();
        var response = await client.GetAsync("/mcp", TestCancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task McpEndpoint_WhenLoginRequired_AdminBearerTokenCanCallTool()
    {
        RequireLogin();

        using var httpClient = Factory.CreateClient();
        ImpersonateAdmin(httpClient);

        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: TestCancellationToken);

        var result = await client.CallToolAsync(
            "get_server_info",
            cancellationToken: TestCancellationToken);

        Assert.False(result.IsError ?? false);
    }

    [Fact]
    public async Task McpEndpoint_WhenLoginRequired_GuestBearerTokenIsRejected()
    {
        RequireLogin();

        using var httpClient = Factory.CreateClient();
        var guestRepo = GetRequiredService<IGuestRepository>();
        var guest = new GuestEntity
        {
            Username = "guest_user",
            AccessExpiration = DateTime.UtcNow.AddDays(1),
            AllowedAddresses = string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest_pass")
        };

        await guestRepo.AddAsync(guest, TestCancellationToken);
        ImpersonateGuest(httpClient, guest);

        var response = await httpClient.GetAsync("/mcp", TestCancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
}
