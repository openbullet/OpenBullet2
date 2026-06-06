using RuriLib.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Services;

public class RuriLibSettingsServiceTests
{
    [Fact]
    public void Constructor_MissingEnvironmentFile_CreatesDefaults() => WithTempDirectory(tempDirectory =>
                                                                             {
                                                                                 var service = new RuriLibSettingsService(tempDirectory);

                                                                                 Assert.True(File.Exists(Path.Combine(tempDirectory, "Environment.ini")));
                                                                                 Assert.Contains("Default", service.Environment.WordlistTypes.Select(t => t.Name));
                                                                                 Assert.Contains("CUSTOM", service.GetStatuses());
                                                                             });

    [Fact]
    public async Task Save_ModifiedSettings_PersistsValues() => await WithTempDirectoryAsync(async tempDirectory =>
                                                                     {
                                                                         var service = new RuriLibSettingsService(tempDirectory);
                                                                         service.RuriLibSettings.GeneralSettings.VerboseMode = true;
                                                                         service.RuriLibSettings.PlaywrightSettings.BrowserType = RuriLib.Models.Settings.PlaywrightBrowserType.Firefox;

                                                                         await service.Save();

                                                                         var reloaded = new RuriLibSettingsService(tempDirectory);
                                                                         Assert.True(reloaded.RuriLibSettings.GeneralSettings.VerboseMode);
                                                                         Assert.Equal(RuriLib.Models.Settings.PlaywrightBrowserType.Firefox,
                                                                             reloaded.RuriLibSettings.PlaywrightSettings.BrowserType);
                                                                     });

    private static void WithTempDirectory(Action<string> action)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(RuriLibSettingsServiceTests)}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            action(tempDirectory);
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    private static async Task WithTempDirectoryAsync(Func<string, Task> action)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(RuriLibSettingsServiceTests)}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            await action(tempDirectory);
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }
}
