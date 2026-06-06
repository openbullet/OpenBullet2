using RuriLib.Models.Environment;
using System;
using System.IO;
using Xunit;

namespace RuriLib.Tests.Models.EnvironmentModels;

public class EnvironmentSettingsTests
{
    [Fact]
    public void RecognizeWordlistType_WhenRegexMatches_ReturnsMatchingType()
    {
        var environment = new EnvironmentSettings
        {
            WordlistTypes =
            [
                new WordlistType { Name = "Default", Regex = ".*" },
                new WordlistType { Name = "EmailPass", Regex = ".+:.+" }
            ]
        };

        var result = environment.RecognizeWordlistType("user@example.com:pass");

        Assert.Equal("Default", result);
    }

    [Fact]
    public void RecognizeWordlistType_WhenNoRegexMatches_ReturnsFirstType()
    {
        var environment = new EnvironmentSettings
        {
            WordlistTypes =
            [
                new WordlistType { Name = "Fallback", Regex = "abc" },
                new WordlistType { Name = "Other", Regex = "def" }
            ]
        };

        var result = environment.RecognizeWordlistType("zzz");

        Assert.Equal("Fallback", result);
    }

    [Fact]
    public void FromIni_ParsesAllSupportedSections()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ob2-env-{Guid.NewGuid():N}.ini");

        try
        {
            File.WriteAllText(path,
                """
                [WORDLIST TYPE]
                Name=EmailPass
                Regex=.+:.+
                Verify=true
                Separator=:
                Slices=EMAIL,PASS

                [CUSTOM STATUS]
                Name=CUSTOM
                Color=#112233

                [EXPORT FORMAT]
                Format=<USER>:<PASS>
                """);

            var environment = EnvironmentSettings.FromIni(path);

            Assert.Single(environment.WordlistTypes);
            Assert.Equal("EmailPass", environment.WordlistTypes[0].Name);
            Assert.True(environment.WordlistTypes[0].Verify);
            Assert.Equal(new[] { "EMAIL", "PASS" }, environment.WordlistTypes[0].Slices);
            Assert.Single(environment.CustomStatuses);
            Assert.Equal("#112233", environment.CustomStatuses[0].Color);
            Assert.Single(environment.ExportFormats);
            Assert.Equal("<USER>:<PASS>", environment.ExportFormats[0].Format);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
