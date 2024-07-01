using OpenBullet2.Web.Dtos.Plugin;
using RuriLib.Services;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class PluginIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    private const string _pluginName = "OB2TestPlugin";
    
    [Fact]
    public async Task AddPlugin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var pluginRepo = GetRequiredService<PluginRepository>();
        var file = new FileInfo($"Resources/{_pluginName}.zip");
        await using var fs = file.OpenRead();
        var content = new MultipartFormDataContent
        {
            { new StreamContent(fs), "file", file.Name }
        };
        
        // Act
        var result = await client.PostAsync("/api/v1/plugin", content);
        
        // Assert
        result.EnsureSuccessStatusCode();
        var plugins = pluginRepo.GetPluginNames();
        Assert.Contains(_pluginName, plugins);
    }
    
    [Fact]
    public async Task GetAllPlugins_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var pluginRepo = GetRequiredService<PluginRepository>();
        var file = new FileInfo($"Resources/{_pluginName}.zip");
        await using var fs = file.OpenRead();
        pluginRepo.AddPlugin(fs);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<PluginDto>>(
            client, "/api/v1/plugin/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        var plugins = result.Value.Select(p => p.Name);
        Assert.Contains(_pluginName, plugins);
    }
    
    [Fact]
    public async Task DeletePlugin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var pluginRepo = GetRequiredService<PluginRepository>();
        var file = new FileInfo($"Resources/{_pluginName}.zip");
        await using var fs = file.OpenRead();
        pluginRepo.AddPlugin(fs);
        
        // Act
        var result = await client.DeleteAsync(
            $"/api/v1/plugin?name={_pluginName}");
        
        // Assert
        result.EnsureSuccessStatusCode();
        var plugins = pluginRepo.GetPluginNames();
        Assert.DoesNotContain(_pluginName, plugins);
    }
}
