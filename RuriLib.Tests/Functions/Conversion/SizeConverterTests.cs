using RuriLib.Functions.Conversion;
using Xunit;

namespace RuriLib.Tests.Functions.Conversion
{
    public class SizeConverterTests
    {
        [Fact]
        public void ToReadableSize_GigaBytes_PrintSize()
        {
            Assert.Equal("4.12 GB", SizeConverter.ToReadableSize(4123456789, false, false, 2));
        }

        [Fact]
        public void ToReadableSize_GibiBytes_PrintSize()
        {
            Assert.Equal("3.8 GiB", SizeConverter.ToReadableSize(4123456789, false, true, 1));
        }

        [Fact]
        public void ToReadableSize_GigaBits_PrintSize()
        {
            Assert.Equal("32.98 Gbit", SizeConverter.ToReadableSize(4123456789, true, false, 2));
        }

        [Fact]
        public void ToReadableSize_GibiBits_PrintSize()
        {
            Assert.Equal("30.722 Gibit", SizeConverter.ToReadableSize(4123456789, true, true, 3));
        }
    }
}
