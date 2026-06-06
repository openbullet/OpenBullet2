using System.IO;
using RuriLib.Functions.Files;
using Xunit;

namespace RuriLib.Tests.Functions.Files;

public class FileUtilsTests
{
    [Fact]
    public void GetFirstAvailableFileName_ValidFileName_ReturnSame()
    {
        var file = Path.GetRandomFileName();
        Assert.Equal(file, FileUtils.GetFirstAvailableFileName(file));
    }

    [Fact]
    public void GetFirstAvailableFileName_OneFileWithSameName_Add1()
    {
        var file = Path.GetTempFileName();
        var fileWithOne = file.Replace(".tmp", "1.tmp");

        try
        {
            Assert.Equal(fileWithOne, FileUtils.GetFirstAvailableFileName(file));
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void GetFirstAvailableFileName_TwoFilesWithSameName_Add2()
    {
        var file = Path.GetTempFileName();
        var fileWithOne = file.Replace(".tmp", "1.tmp");
        var fileWithTwo = file.Replace(".tmp", "2.tmp");

        try
        {
            using (File.Create(fileWithOne))
            {
            }

            Assert.Equal(fileWithTwo, FileUtils.GetFirstAvailableFileName(file));
        }
        finally
        {
            File.Delete(file);
            File.Delete(fileWithOne);
        }
    }
}
