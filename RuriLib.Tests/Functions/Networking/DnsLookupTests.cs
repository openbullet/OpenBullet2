using RuriLib.Functions.Networking;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Functions.Networking
{
    public class DnsLookupTests
    {
        [Fact]
        public async Task FromGoogle_MailDotCom_MXRecords()
        {
            var entries = await DnsLookup.FromGoogle("mail.com", "MX");
            Assert.Contains("mx00.mail.com", entries);
            Assert.Contains("mx01.mail.com", entries);
        }
    }
}
