using Newtonsoft.Json;
using RuriLib.Helpers;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Helpers;

public class ConfigPackerTests
{
    private const string ValidLoliCode = """
        BLOCK:ConstantString
          value = "hello"
        ENDBLOCK
        """;

    private const string ValidStartupLoliCode = """
        BLOCK:ConstantString
          value = "startup"
        ENDBLOCK
        """;

    [Fact]
    public async Task PackAndUnpackAsync_LoliCodeConfig_RoundTrips()
    {
        var config = new Config
        {
            Id = "cfg-1",
            Mode = ConfigMode.LoliCode,
            Readme = "readme",
            LoliCodeScript = ValidLoliCode,
            StartupLoliCodeScript = ValidStartupLoliCode
        };
        config.Metadata.Name = "Example";

        var packed = await ConfigPacker.PackAsync(config);
        await using var stream = new MemoryStream(packed);

        var unpacked = await ConfigPacker.UnpackAsync(stream);

        Assert.Equal(ConfigMode.LoliCode, unpacked.Mode);
        Assert.Equal(config.Readme, unpacked.Readme);
        Assert.Equal(config.LoliCodeScript, unpacked.LoliCodeScript);
        Assert.Equal(config.StartupLoliCodeScript, unpacked.StartupLoliCodeScript);
        Assert.Equal(config.Metadata.Name, unpacked.Metadata.Name);
    }

    [Fact]
    public async Task UnpackAsync_MissingMetadata_ThrowsFileNotFoundException()
    {
        await using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            CreateTextEntry(archive, "settings.json", "{}");
            CreateTextEntry(archive, "script.loli", ValidLoliCode);
        }

        stream.Position = 0;

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => ConfigPacker.UnpackAsync(stream));

        Assert.Equal("metadata.json", exception.FileName);
    }

    [Fact]
    public void Deserialize_NestedGhostCursorSettings_PopulatesGhostCursor()
    {
        var settings = JsonConvert.DeserializeObject<BrowserSettings>(
            """
            {
              "MouseAutomationMode": 1,
              "GhostCursor": {
                "ScrollSpeed": 42,
                "OvershootThreshold": 321
              }
            }
            """);

        Assert.NotNull(settings);
        Assert.Equal(BrowserMouseAutomationMode.GhostCursor, settings.MouseAutomationMode);
        Assert.Equal(42, settings.GhostCursor.ScrollSpeed);
        Assert.Equal(321, settings.GhostCursor.OvershootThreshold);
    }

    [Fact]
    public async Task PackAsync_WritesGhostCursorSettings()
    {
        var config = new Config
        {
            Id = "cfg-ghost",
            Mode = ConfigMode.LoliCode,
            Readme = "readme",
            LoliCodeScript = ValidLoliCode
        };
        config.Settings.BrowserSettings.MouseAutomationMode = BrowserMouseAutomationMode.GhostCursor;
        config.Settings.BrowserSettings.GhostCursor.ScrollSpeed = 55;

        var packed = await ConfigPacker.PackAsync(config);
        await using var stream = new MemoryStream(packed);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);

        var settingsJson = ReadTextEntry(archive, "settings.json");

        Assert.Contains(@"""GhostCursor"": {", settingsJson);
        Assert.Contains(@"""ScrollSpeed"": 55", settingsJson);
    }

    private static void CreateTextEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }

    private static string ReadTextEntry(ZipArchive archive, string path)
    {
        var entry = archive.GetEntry(path)!;
        using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
        return reader.ReadToEnd();
    }
}

public class GZipTests
{
    [Fact]
    public void ZipAndUnzip_RoundTrips()
    {
        var bytes = Encoding.UTF8.GetBytes("OpenBullet2");

        var zipped = GZip.Zip(bytes);
        var unzipped = GZip.Unzip(zipped);

        Assert.Equal(bytes, unzipped);
    }
}
