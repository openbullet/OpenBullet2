using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using System.IO;

namespace OpenBullet2.Native.Tests;

[Collection("WPF")]
public sealed class DependencyInjectionTests(WpfAppFixture fixture)
{
    [Fact]
    public async Task ResolveServices_CriticalServices_AreRegistered()
    {
        await fixture.InvokeAsync(services =>
        {
            var uiFactory = services.GetRequiredService<IUiFactory>();
            var configService = services.GetRequiredService<ConfigService>();
            var configsViewModel = services.GetRequiredService<ConfigsViewModel>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var updateService = services.GetRequiredService<UpdateService>();
            var announcementService = services.GetRequiredService<AnnouncementService>();
            var jobsViewModel = services.GetRequiredService<JobsViewModel>();

            Assert.NotNull(uiFactory);
            Assert.NotNull(configService);
            Assert.NotNull(configsViewModel);
            Assert.NotNull(loggerFactory);
            Assert.NotNull(updateService);
            Assert.NotNull(announcementService);
            Assert.NotNull(jobsViewModel);
        });
    }

    [Fact]
    public async Task ResolveServices_UserDataDirectoryProvider_IsRegistered()
    {
        await fixture.InvokeAsync(services =>
        {
            var userDataDirectory = services.GetRequiredService<UserDataDirectoryProvider>();

            Assert.NotNull(userDataDirectory);
            Assert.True(Path.IsPathRooted(userDataDirectory.RootPath));
            Assert.Equal(Path.Combine(userDataDirectory.RootPath, "Logs"), userDataDirectory.GetPath("Logs"));
        });
    }
}
