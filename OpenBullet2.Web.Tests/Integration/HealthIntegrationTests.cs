using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class HealthIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task HealthCheck_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/v1/health");
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }
}
