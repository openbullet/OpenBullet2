using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.Views.Pages;
using RuriLib.Services;

namespace OpenBullet2.Native.Tests;

[Collection("WPF")]
public sealed class PageCreationTests(WpfAppFixture fixture)
{
    [Fact]
    public async Task Create_CriticalPages_Succeeds()
    {
        await fixture.InvokeAsync(services =>
        {
            var uiFactory = services.GetRequiredService<IUiFactory>();
            var rlSettings = services.GetRequiredService<RuriLibSettingsService>();
            var configService = services.GetRequiredService<ConfigService>();

            var config = TestConfigFactory.Create(rlSettings);
            configService.Configs = [config];
            configService.SelectedConfig = config;

            Assert.NotNull(uiFactory.Create<Home>());
            Assert.NotNull(uiFactory.Create<Proxies>());
            Assert.NotNull(uiFactory.Create<MultiRunJobViewer>());
            Assert.NotNull(uiFactory.Create<ProxyCheckJobViewer>());
            Assert.NotNull(uiFactory.Create<ConfigMetadata>());
            Assert.NotNull(uiFactory.Create<ConfigReadme>());
            Assert.NotNull(uiFactory.Create<ConfigSettings>());
            Assert.NotNull(uiFactory.Create<RLSettings>());
            Assert.NotNull(uiFactory.Create<Plugins>());
        });
    }

    [Fact]
    public async Task UpdateViewModel_ConfigPagesWithSelectedConfig_Succeeds()
    {
        await fixture.InvokeAsync(services =>
        {
            var uiFactory = services.GetRequiredService<IUiFactory>();
            var rlSettings = services.GetRequiredService<RuriLibSettingsService>();
            var configService = services.GetRequiredService<ConfigService>();

            var config = TestConfigFactory.Create(rlSettings);
            configService.Configs = [config];
            configService.SelectedConfig = config;

            var metadataPage = uiFactory.Create<ConfigMetadata>();
            metadataPage.UpdateViewModel();

            var readmePage = uiFactory.Create<ConfigReadme>();
            readmePage.UpdateViewModel();

            var settingsPage = uiFactory.Create<ConfigSettings>();
            settingsPage.UpdateViewModel();
        });
    }
}
