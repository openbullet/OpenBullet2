using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.ViewModels;
using RuriLib.Services;

namespace OpenBullet2.Native.Tests;

[Collection("WPF")]
public sealed class ConfigsViewModelTests(WpfAppFixture fixture)
{
    [Fact]
    public async Task SelectedConfig_Setter_UpdatesConfigServiceAndSelectionState()
    {
        await fixture.InvokeAsync(services =>
        {
            var rlSettings = services.GetRequiredService<RuriLibSettingsService>();
            var configService = services.GetRequiredService<ConfigService>();
            var configsViewModel = services.GetRequiredService<ConfigsViewModel>();

            var config = TestConfigFactory.Create(rlSettings);
            configService.Configs = [config];
            configsViewModel.CreateCollection();

            configsViewModel.SelectedConfig = Assert.Single(configsViewModel.ConfigsCollection);

            Assert.Same(config, configService.SelectedConfig);
            Assert.True(configsViewModel.IsConfigSelected);
        });
    }

    [Fact]
    public async Task CreateCollection_WithEmptyConfigIcon_DoesNotThrowAndExposesNullIcon()
    {
        await fixture.InvokeAsync(services =>
        {
            var rlSettings = services.GetRequiredService<RuriLibSettingsService>();
            var configService = services.GetRequiredService<ConfigService>();
            var configsViewModel = services.GetRequiredService<ConfigsViewModel>();

            var config = TestConfigFactory.Create(rlSettings);
            config.Metadata.Base64Image = string.Empty;

            configService.Configs = [config];
            configsViewModel.CreateCollection();

            var configViewModel = Assert.Single(configsViewModel.ConfigsCollection);
            Assert.Null(configViewModel.Icon);
        });
    }
}
