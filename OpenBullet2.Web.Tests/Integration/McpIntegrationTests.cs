using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
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
}
