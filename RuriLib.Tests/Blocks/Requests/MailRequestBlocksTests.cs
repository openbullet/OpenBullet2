using MailKit;
using ImapMethods = RuriLib.Blocks.Requests.Imap.Methods;
using Pop3Methods = RuriLib.Blocks.Requests.Pop3.Methods;
using RuriLib.Exceptions;
using RuriLib.Functions.Imap;
using RuriLib.Functions.Pop3;
using RuriLib.Functions.Smtp;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Providers.Emails;
using SmtpMethods = RuriLib.Blocks.Requests.Smtp.Methods;
using RuriLib.Tests.Utils.Mockup;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;

namespace RuriLib.Tests.Blocks.Requests;

public class MailRequestBlocksTests
{
    [Fact]
    public void ImapGetLog_WithoutLogger_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() => ImapMethods.ImapGetLog(data));

        Assert.Equal("The IMAP protocol logger is not initialized", ex.Message);
    }

    [Fact]
    public void Pop3GetLog_WithoutLogger_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() => Pop3Methods.Pop3GetLog(data));

        Assert.Equal("The POP3 protocol logger is not initialized", ex.Message);
    }

    [Fact]
    public void SmtpGetLog_WithoutLogger_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() => SmtpMethods.SmtpGetLog(data));

        Assert.Equal("The SMTP protocol logger is not initialized", ex.Message);
    }

    [Fact]
    public void MailGetLog_WithLogger_ReturnsContent()
    {
        var data = NewBotData();
        data.SetObject("imapLogger", CreateLogger("imap-log"));
        data.SetObject("pop3Logger", CreateLogger("pop3-log"));
        data.SetObject("smtpLogger", CreateLogger("smtp-log"));

        Assert.Equal("imap-log", ImapMethods.ImapGetLog(data));
        Assert.Equal("pop3-log", Pop3Methods.Pop3GetLog(data));
        Assert.Equal("smtp-log", SmtpMethods.SmtpGetLog(data));
    }

    [Fact]
    public async Task ImapOpenFolder_WithoutFolderCache_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            ImapMethods.ImapOpenFolder(data, "Inbox"));

        Assert.Equal("Get the list of folders first!", ex.Message);
    }

    [Fact]
    public async Task Pop3DeleteMail_WithoutClient_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            Pop3Methods.Pop3DeleteMail(data, 0));

        Assert.Equal("Connect the POP3 client first!", ex.Message);
    }

    [Fact]
    public async Task SmtpSendMail_WithoutClient_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            SmtpMethods.SmtpSendMail(
                data,
                "Sender",
                "sender@example.com",
                "Recipient",
                "recipient@example.com",
                "Subject",
                "Text body",
                "<p>Html body</p>"));

        Assert.Equal("Connect the SMTP client first!", ex.Message);
    }

    [Fact]
    public async Task SmtpSendMailAdvanced_WithoutClient_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            SmtpMethods.SmtpSendMailAdvanced(
                data,
                new Dictionary<string, string> { ["Sender"] = "sender@example.com" },
                new Dictionary<string, string> { ["Recipient"] = "recipient@example.com" },
                "Subject",
                "Text body",
                "<p>Html body</p>",
                new Dictionary<string, string> { ["X-Test"] = "value" },
                []));

        Assert.Equal("Connect the SMTP client first!", ex.Message);
    }

    [Fact]
    public async Task SmtpAutoConnect_KnownServersOnly_DoesNotFallback()
    {
        var repository = new InMemoryEmailDomainRepository();
        repository.AddSmtp("test.local", "127.0.0.1", 1);
        var data = NewBotData(repository);

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            SmtpMethods.SmtpAutoConnect(data, "test1@test.local", 250, SmtpAutoConnectMode.KnownServersOnly));

        Assert.Equal("Exhausted the known SMTP servers from smtpdomains.dat, failed to connect!", ex.Message);
    }

    [Fact]
    public async Task ImapAutoConnect_KnownServersOnly_DoesNotFallback()
    {
        var repository = new InMemoryEmailDomainRepository();
        repository.AddImap("test.local", "127.0.0.1", 1);
        var data = NewBotData(repository);

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            ImapMethods.ImapAutoConnect(data, "test1@test.local", 250, ImapAutoConnectMode.KnownServersOnly));

        Assert.Equal("Exhausted the known IMAP servers from imapdomains.dat, failed to connect!", ex.Message);
    }

    [Fact]
    public async Task Pop3AutoConnect_KnownServersOnly_DoesNotFallback()
    {
        var repository = new InMemoryEmailDomainRepository();
        repository.AddPop3("test.local", "127.0.0.1", 1);
        var data = NewBotData(repository);

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            Pop3Methods.Pop3AutoConnect(data, "test1@test.local", 250, Pop3AutoConnectMode.KnownServersOnly));

        Assert.Equal("Exhausted the known POP3 servers from pop3domains.dat, failed to connect!", ex.Message);
    }

    private static ProtocolLogger CreateLogger(string content)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content))
        {
            Position = 0
        };
        return new ProtocolLogger(stream);
    }

    private static BotData NewBotData(IEmailDomainRepository? emailDomains = null)
        => new(
            new BotProviders(null!)
            {
                EmailDomains = emailDomains ?? new InMemoryEmailDomainRepository(),
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("hello", new WordlistType()));
}
