using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Tests.Utils;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ConversionMethods = RuriLib.Blocks.Utility.Conversion.Methods;
using FileMethods = RuriLib.Blocks.Utility.Files.Methods;
using ImageMethods = RuriLib.Blocks.Utility.Images.Methods;
using StringEncoding = RuriLib.Blocks.Utility.Conversion.StringEncoding;
using UtilityMethods = RuriLib.Blocks.Utility.Methods;
using BotProviders = RuriLib.Models.Bots.Providers;

namespace RuriLib.Tests.Blocks;

public sealed class UtilityBlocksTests : IDisposable
{
    private readonly string tempDir;

    public UtilityBlocksTests()
    {
        tempDir = Path.Combine(Path.GetTempPath(), "ob2-utility-block-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
    }

    [Fact]
    public void ClearCookies_EmptiesCookieJar()
    {
        var data = NewBotData();
        data.COOKIES["session"] = "abc";

        UtilityMethods.ClearCookies(data);

        Assert.Empty(data.COOKIES);
    }

    [Fact]
    public void UTF8AndBase64_RoundTrip()
    {
        var data = NewBotData();
        var base64 = ConversionMethods.UTF8ToBase64(data, "ciao");
        var utf8 = ConversionMethods.Base64ToUTF8(data, base64);

        Assert.Equal("ciao", utf8);
    }

    [Fact]
    public void StringToBytes_And_Back_RoundTrip()
    {
        var data = NewBotData();
        var bytes = ConversionMethods.StringToBytes(
            data,
            "hello",
            StringEncoding.UTF8);
        var value = ConversionMethods.BytesToString(
            data,
            bytes,
            StringEncoding.UTF8);

        Assert.Equal("hello", value);
    }

    [WindowsFact]
    public void SvgToPng_ReturnsPngBytes()
    {
        var data = NewBotData();
        var png = ImageMethods.SvgToPng(
            data,
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"10\" height=\"10\"><rect width=\"10\" height=\"10\" fill=\"red\"/></svg>",
            10,
            10);

        Assert.True(png.Length > 8);
        Assert.Equal([137, 80, 78, 71, 13, 10, 26, 10], png.Take(8).ToArray());
    }

    [Fact]
    public async Task FileWriteReadAppend_And_List_Work()
    {
        var data = NewBotData();
        var filePath = Path.Combine(tempDir, "nested", "test.txt");

        await FileMethods.FileWrite(data, filePath, "line1");
        await FileMethods.FileAppendLines(data, filePath, ["line2", "line3"]);

        var text = await FileMethods.FileRead(data, filePath);
        var lines = await FileMethods.FileReadLines(data, filePath);
        var exists = await FileMethods.FileExistsAsync(data, filePath);
        var folderExists = FileMethods.FolderExists(data, Path.GetDirectoryName(filePath)!);
        var files = FileMethods.GetFilesInFolder(data, Path.GetDirectoryName(filePath)!);

        Assert.True(exists);
        Assert.True(folderExists);
        Assert.Contains("line1", text);
        Assert.Equal(["line1line2", "line3"], lines);
        Assert.Contains(filePath, files);
    }

    [Fact]
    public async Task FileBytes_CopyMoveDelete_And_CreatePath_Work()
    {
        var data = NewBotData();
        var sourcePath = Path.Combine(tempDir, "bytes", "source.bin");
        var copyPath = Path.Combine(tempDir, "copy", "copied.bin");
        var movePath = Path.Combine(tempDir, "moved", "moved.bin");
        var createdFilePath = Path.Combine(tempDir, "created", "nested", "file.txt");

        await FileMethods.FileWriteBytes(data, sourcePath, [0x01, 0x02, 0x03]);

        var bytes = await FileMethods.FileReadBytes(data, sourcePath);
        FileMethods.FileCopy(data, sourcePath, copyPath);
        FileMethods.FileMove(data, copyPath, movePath);
        FileMethods.FileDelete(data, sourcePath);
        FileMethods.CreatePath(data, createdFilePath);

        Assert.Equal([0x01, 0x02, 0x03], bytes);
        Assert.False(File.Exists(sourcePath));
        Assert.False(File.Exists(copyPath));
        Assert.True(File.Exists(movePath));
        Assert.True(Directory.Exists(Path.Combine(tempDir, "created", "nested")));
    }

    [Fact]
    public async Task FileWriteLines_And_FolderDelete_Work()
    {
        var data = NewBotData();
        var folderPath = Path.Combine(tempDir, "lines");
        var filePath = Path.Combine(folderPath, "test.txt");

        await FileMethods.FileWriteLines(data, filePath, ["one", "two"]);

        var lines = await FileMethods.FileReadLines(data, filePath);
        FileMethods.FolderDelete(data, folderPath);

        Assert.Equal(["one", "two"], lines);
        Assert.False(Directory.Exists(folderPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static BotData NewBotData()
        => new(
            new BotProviders(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("hello", new WordlistType()));
}
