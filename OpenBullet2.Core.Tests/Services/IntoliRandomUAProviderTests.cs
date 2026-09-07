using OpenBullet2.Core.Services;
using RuriLib.Providers.UserAgents;

namespace OpenBullet2.Core.Tests.Services;

public class IntoliRandomUAProviderTests
{
    [Fact]
    public void Generate_LinuxArmv8l_ReturnsAndroidUserAgent()
    {
        var jsonFile = Path.Combine(Path.GetTempPath(), $"{nameof(IntoliRandomUAProviderTests)}-{Guid.NewGuid():N}.json");

        try
        {
            const string userAgent = "Mozilla/5.0 (Linux; Android 16; Test Device) AppleWebKit/537.36";
            File.WriteAllText(jsonFile, $$"""
                [
                  {
                    "userAgent": "{{userAgent}}",
                    "platform": "Linux armv8l",
                    "weight": 1
                  }
                ]
                """);

            var provider = new IntoliRandomUAProvider(jsonFile);

            Assert.Equal(userAgent, provider.Generate(UAPlatform.Android));
        }
        finally
        {
            File.Delete(jsonFile);
        }
    }
}
