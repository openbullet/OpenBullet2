using System;
using RuriLib.Functions.Imap;
using RuriLib.Functions.Pop3;
using RuriLib.Functions.Smtp;
using Xunit;

namespace RuriLib.Tests.Functions.Networking;

public class MailAutoconfigTests
{
    [Fact]
    public void SmtpParse_ValidXml_ReturnsHosts()
    {
        const string xml = """
            <clientConfig>
              <emailProvider>
                <outgoingServer type="smtp">
                  <hostname>smtp.example.com</hostname>
                  <port>587</port>
                </outgoingServer>
              </emailProvider>
            </clientConfig>
            """;

        var hosts = SmtpAutoconfig.Parse(xml);

        var host = Assert.Single(hosts);
        Assert.Equal("smtp.example.com", host.Host);
        Assert.Equal(587, host.Port);
    }

    [Fact]
    public void Pop3Parse_MissingPort_ThrowsFormatException()
    {
        const string xml = """
            <clientConfig>
              <emailProvider>
                <incomingServer type="pop3">
                  <hostname>pop3.example.com</hostname>
                </incomingServer>
              </emailProvider>
            </clientConfig>
            """;

        Assert.Throws<FormatException>(() => Pop3Autoconfig.Parse(xml));
    }

    [Fact]
    public void ImapParse_InvalidPort_ThrowsFormatException()
    {
        const string xml = """
            <clientConfig>
              <emailProvider>
                <incomingServer type="imap">
                  <hostname>imap.example.com</hostname>
                  <port>abc</port>
                </incomingServer>
              </emailProvider>
            </clientConfig>
            """;

        Assert.Throws<FormatException>(() => ImapAutoconfig.Parse(xml));
    }

    [Fact]
    public void Parse_NoMatchingServers_ReturnsEmptyList()
    {
        const string xml = """
            <clientConfig>
              <emailProvider>
              </emailProvider>
            </clientConfig>
            """;

        Assert.Empty(SmtpAutoconfig.Parse(xml));
        Assert.Empty(Pop3Autoconfig.Parse(xml));
        Assert.Empty(ImapAutoconfig.Parse(xml));
    }
}
