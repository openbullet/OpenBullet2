using RuriLib.Functions.Files;
using System.IO;
using Xunit;

namespace RuriLib.Tests.Functions.Files
{
    public class FileUtilsTests
    {
        [Fact]
        public void GetFirstAvailableFileName_OneFileWithSameName_Add1()
        {
            string file = Path.GetTempFileName();
            string fileWithOne = file.Replace(".tmp", "1.tmp");
            
            Assert.Equal(fileWithOne, FileUtils.GetFirstAvailableFileName(file));
        }

        [Fact]
        public void GetFirstAvailableFileName_TwoFilesWithSameName_Add2()
        {
            string file = Path.GetTempFileName();
            string fileWithOne = file.Replace(".tmp", "1.tmp");
            string fileWithTwo = file.Replace(".tmp", "2.tmp");
            
            File.Create(fileWithOne);
            Assert.Equal(fileWithTwo, FileUtils.GetFirstAvailableFileName(file));
        }
    }
}
