using Xunit;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class FallbackIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task UnknownPath_WithoutIndexHtml_ReturnsNotFound()
    {
        using var client = Factory.CreateClient();

        var response = await client.GetAsync("/this-path-does-not-exist", TestCancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
