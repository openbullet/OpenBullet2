using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Interfaces;
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

        Assert.Contains(tools, tool => tool.Name == "get_openbullet_server_info");
        Assert.Contains(tools, tool => tool.Name == "get_openbullet_environment");
        Assert.Contains(tools, tool => tool.Name == "list_openbullet_configs");
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
            "get_openbullet_server_info",
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
            "get_openbullet_environment",
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
            "list_openbullet_configs",
            cancellationToken: TestCancellationToken);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

        Assert.False(result.IsError ?? false);
        Assert.Contains(config.Id, text);
        Assert.Contains(config.Metadata.Name, text);
        Assert.Contains(config.Metadata.LastModified.ToString("O"), text);
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
            "get_openbullet_server_info",
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
