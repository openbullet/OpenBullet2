using System.Threading;
using System.Threading.Tasks;
using RuriLib.Functions.Networking;
using Xunit;

namespace RuriLib.Tests.Functions.Networking;

public class DnsLookupTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task FromGoogle_MailDotCom_MXRecords()
    {
        var entries = await DnsLookup.FromGoogleAsync("mail.com", "MX", cancellationToken: TestCancellationToken);
        Assert.Contains("mx00.mail.com", entries);
        Assert.Contains("mx01.mail.com", entries);
    }
}
