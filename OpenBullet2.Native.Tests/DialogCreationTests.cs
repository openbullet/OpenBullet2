using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Data.Rules;
using RuriLib.Models.Jobs;
using RuriLib.Services;

namespace OpenBullet2.Native.Tests;

[Collection("WPF")]
public sealed class DialogCreationTests(WpfAppFixture fixture)
{
    [Fact]
    public async Task Create_CriticalDialogs_Succeeds()
    {
        await fixture.InvokeAsync(services =>
        {
            var uiFactory = services.GetRequiredService<IUiFactory>();
            var rlSettings = services.GetRequiredService<RuriLibSettingsService>();
            var configService = services.GetRequiredService<ConfigService>();

            var config = TestConfigFactory.Create(rlSettings);
            configService.Configs = [config];
            configService.SelectedConfig = config;

            var wordlistType = rlSettings.Environment.WordlistTypes.First().Name;
            Func<JobOptions, Task> onAccept = _ => Task.CompletedTask;

            Assert.NotNull(uiFactory.Create<AddBlockDialog>(new object()));
            Assert.NotNull(uiFactory.Create<ImportProxiesDialog>(new object()));
            Assert.NotNull(uiFactory.Create<SelectConfigDialog>(new object()));
            Assert.NotNull(uiFactory.Create<SelectWordlistDialog>(new object()));
            Assert.NotNull(uiFactory.Create<MultiRunJobOptionsDialog>(onAccept));
            Assert.NotNull(uiFactory.Create<ProxyCheckJobOptionsDialog>(onAccept));
            Assert.NotNull(uiFactory.Create<TestDataRulesDialog>("test", wordlistType, Array.Empty<DataRule>()));
        });
    }
}
