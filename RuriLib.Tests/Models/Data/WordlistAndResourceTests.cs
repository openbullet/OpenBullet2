using RuriLib.Models.Data;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Data.Resources;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Environment;
using RuriLib.Functions.Files;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace RuriLib.Tests.Models.Data;

public class WordlistAndResourceTests
{
    [Theory]
    [InlineData("", 0L)]
    [InlineData("one", 1L)]
    [InlineData("one\n", 1L)]
    [InlineData("one\r\n", 1L)]
    [InlineData("one\r", 1L)]
    [InlineData("\n", 1L)]
    [InlineData("\r\n", 1L)]
    [InlineData("\r", 1L)]
    [InlineData("one\ntwo\r\nthree\rfour", 4L)]
    [InlineData("one\r\ntwo\nthree\r\n", 3L)]
    public void FileUtils_CountLines_HandlesSupportedLineTerminators(string content, long expected)
    {
        var fileName = Path.GetTempFileName();

        try
        {
            File.WriteAllText(fileName, content);

            Assert.Equal(expected, FileUtils.CountLines(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void Wordlist_CountLinesFalse_AllowsInMemoryWordlist()
    {
        var type = new WordlistType { Name = "Default" };

        var wordlist = new Wordlist("test", null, type, null, countLines: false);

        Assert.Null(wordlist.Path);
        Assert.Equal(string.Empty, wordlist.Purpose);
        Assert.Equal(0, wordlist.Total);
    }

    [Fact]
    public void Wordlist_CountLinesTrue_UsesLongSafeCounting()
    {
        var fileName = Path.GetTempFileName();

        try
        {
            File.WriteAllText(fileName, "one\r\ntwo\nthree\rfour");
            var wordlist = new Wordlist("test", fileName, new WordlistType { Name = "Default" }, null);

            Assert.Equal(4L, wordlist.Total);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void WordlistDataPool_WordlistWithoutPath_ThrowsExplicitly()
    {
        var wordlist = new Wordlist("test", null, new WordlistType { Name = "Default" }, null, countLines: false);

        var exception = Assert.Throws<ArgumentException>(() => new WordlistDataPool(wordlist));

        Assert.Contains("reside on disk", exception.Message);
    }

    [Fact]
    public void FileDataPool_NullFileName_Throws() => Assert.Throws<ArgumentNullException>(() => new FileDataPool(null!));

    [Fact]
    public void LinesFromFileResource_LoopsAroundAfterEnd()
    {
        var fileName = Path.GetTempFileName();

        try
        {
            File.WriteAllText(fileName, $"one{Environment.NewLine}two{Environment.NewLine}");
            using var resource = new LinesFromFileResource(new LinesFromFileResourceOptions
            {
                Location = fileName,
                LoopsAround = true
            });

            Assert.Equal(["one", "two", "one"], resource.Take(3));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void RandomLinesFromFileResource_Unique_ExhaustsSource()
    {
        var fileName = Path.GetTempFileName();

        try
        {
            File.WriteAllText(fileName, $"one{Environment.NewLine}two{Environment.NewLine}");
            var resource = new RandomLinesFromFileResource(new RandomLinesFromFileResourceOptions
            {
                Location = fileName,
                Unique = true
            });

            var taken = resource.Take(2);

            Assert.Equal(2, taken.Distinct().Count());
            Assert.Throws<Exception>(() => resource.TakeOne());
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
