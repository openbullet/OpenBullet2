using MailKit;
using ImapMethods = RuriLib.Blocks.Requests.Imap.Methods;
using Pop3Methods = RuriLib.Blocks.Requests.Pop3.Methods;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Proxies;
using RuriLib.Functions.Networking;
using RuriLib.Providers.Emails;
using SmtpMethods = RuriLib.Blocks.Requests.Smtp.Methods;
using RuriLib.Tests.Utils;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;
using SearchField = RuriLib.Functions.Imap.SearchField;

namespace RuriLib.Tests.Blocks.Requests;

[Collection(nameof(MailServerCollection))]
public class MailRequestIntegrationTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task SmtpConnect_Login_SendMail_AndGetLog_Verify()
    {
        await TestMailServer.ResetState();
        var connection = await TestMailServer.GetConnectionInfo();
        var data = NewBotData();

        await SmtpMethods.SmtpConnect(data, connection.Host, connection.SmtpPort, 20000);
        await SmtpMethods.SmtpLogin(data, connection.PrimaryEmail, connection.PrimaryPassword, 20000);
        await SmtpMethods.SmtpSendMail(
            data,
            "Sender One",
            connection.PrimaryEmail,
            "Recipient Two",
            connection.SecondaryEmail,
            "smtp-subject",
            "smtp-text-body",
            "<p>smtp-html-body</p>");

        await TestMailServer.WaitForMessageCount(connection.SecondaryEmail, 1);
        var messages = await TestMailServer.GetMessages(connection.SecondaryEmail);
        var log = SmtpMethods.SmtpGetLog(data);

        Assert.Single(messages);
        Assert.Equal("smtp-subject", messages[0].Subject);
        Assert.Contains("AUTH", log, StringComparison.Ordinal);
        Assert.Contains("DATA", log, StringComparison.Ordinal);

        await SmtpMethods.SmtpDisconnect(data);
    }

    [Fact]
    public async Task ImapConnect_Login_ListReadDelete_AndDisconnect_Verify()
    {
        await TestMailServer.ResetState();
        var connection = await TestMailServer.GetConnectionInfo();
        await TestMailServer.SendMail(
            "Sender One",
            connection.PrimaryEmail,
            "Recipient Two",
            connection.SecondaryEmail,
            "imap-subject",
            "imap-text-body",
            "<p>imap-html-body</p>");

        await TestMailServer.WaitForMessageCount(connection.SecondaryEmail, 1);

        var data = NewBotData();
        await SmtpMethods.SmtpConnect(data, connection.Host, connection.SmtpPort, 20000);
        await SmtpMethods.SmtpLogin(data, connection.PrimaryEmail, connection.PrimaryPassword, 20000);
        await SmtpMethods.SmtpSendMail(
            data,
            "Sender One",
            connection.PrimaryEmail,
            "Recipient Two",
            connection.SecondaryEmail,
            "imap-second-subject",
            "imap-second-text-body",
            "<p>imap-second-html-body</p>");
        await SmtpMethods.SmtpDisconnect(data);

        await TestMailServer.WaitForMessageCount(connection.SecondaryEmail, 2);

        data = NewBotData();
        await ImapMethods.ImapConnect(data, connection.Host, connection.ImapPort, 20000);
        await ImapMethods.ImapLogin(data, connection.SecondaryEmail, connection.SecondaryPassword, openInbox: true, timeoutMilliseconds: 20000);

        var folders = await ImapMethods.ListFolders(data);
        var opened = await ImapMethods.ImapOpenFolder(data, "INBOX", FolderAccess.ReadWrite);
        var count = await ImapMethods.GetMailCount(data);
        var ids = await ImapMethods.ImapSearchMails(
            data,
            SearchField.Subject,
            "imap-",
            SearchField.From,
            "test1@test.local",
            1);
        var firstMail = await ImapMethods.ImapReadMail(data, ids[0], preferHtml: false);
        var rawMail = await ImapMethods.ImapReadMailRaw(data, ids[0]);
        var log = ImapMethods.ImapGetLog(data);
        var lastMessageIndex = await ImapMethods.GetLastMessageId(data);

        Assert.True(opened);
        Assert.Contains("INBOX", folders);
        Assert.Equal(2, count);
        Assert.Equal(2, ids.Count);
        Assert.Contains("Subject: imap-subject", firstMail, StringComparison.Ordinal);
        Assert.Contains("imap-text-body", firstMail, StringComparison.Ordinal);
        Assert.Contains("Subject: imap-subject", System.Text.Encoding.UTF8.GetString(rawMail), StringComparison.Ordinal);
        Assert.Contains("LOGIN", log, StringComparison.Ordinal);
        Assert.Equal(1, lastMessageIndex);

        await ImapMethods.ImapDeleteMail(data, ids[0]);
        await ImapMethods.ImapCloseFolder(data);
        await ImapMethods.ImapOpenInbox(data);

        var countAfterDelete = await ImapMethods.GetMailCount(data);
        Assert.Equal(1, countAfterDelete);

        await ImapMethods.ImapDisconnect(data);
    }

    [Fact]
    public async Task Pop3Connect_Login_ReadDelete_AndDisconnect_Verify()
    {
        await TestMailServer.ResetState();
        var connection = await TestMailServer.GetConnectionInfo();
        await TestMailServer.SendMail(
            "Sender One",
            connection.PrimaryEmail,
            "Recipient Two",
            connection.SecondaryEmail,
            "pop3-first-subject",
            "pop3-first-text-body",
            "<p>pop3-first-html-body</p>");
        await TestMailServer.SendMail(
            "Sender One",
            connection.PrimaryEmail,
            "Recipient Two",
            connection.SecondaryEmail,
            "pop3-second-subject",
            "pop3-second-text-body",
            "<p>pop3-second-html-body</p>");

        await TestMailServer.WaitForMessageCount(connection.SecondaryEmail, 2);

        var data = NewBotData();
        await Pop3Methods.Pop3Connect(data, connection.Host, connection.Pop3Port, 20000);
        await Pop3Methods.Pop3Login(data, connection.SecondaryEmail, connection.SecondaryPassword, 20000);

        var mails = await Pop3Methods.Pop3GetMails(data);
        var latestMail = await Pop3Methods.Pop3ReadMail(data, 0, preferHtml: false);
        var log = Pop3Methods.Pop3GetLog(data);

        Assert.Equal(2, mails.Count);
        Assert.StartsWith("Sender One|Recipient Two|pop3-second-subject", mails[0], StringComparison.Ordinal);
        Assert.Contains("Subject: pop3-second-subject", latestMail, StringComparison.Ordinal);
        Assert.Contains("pop3-second-text-body", latestMail, StringComparison.Ordinal);
        Assert.Contains("pop://", log, StringComparison.Ordinal);

        await Pop3Methods.Pop3DeleteMail(data, 0);
        await Pop3Methods.Pop3Disconnect(data);

        data = NewBotData();
        await Pop3Methods.Pop3Connect(data, connection.Host, connection.Pop3Port, 20000);
        await Pop3Methods.Pop3Login(data, connection.SecondaryEmail, connection.SecondaryPassword, 20000);
        var mailsAfterDelete = await Pop3Methods.Pop3GetMails(data);

        Assert.Single(mailsAfterDelete);
        Assert.StartsWith("Sender One|Recipient Two|pop3-second-subject", mailsAfterDelete[0], StringComparison.Ordinal);

        await Pop3Methods.Pop3Disconnect(data);
    }

    [Fact]
    public async Task MailAutoConnect_UsesSeededRepositoryEntries()
    {
        await TestMailServer.ResetState();
        var connection = await TestMailServer.GetConnectionInfo();
        var repository = new InMemoryEmailDomainRepository();
        repository.AddImap("test.local", connection.Host, connection.ImapPort);
        repository.AddPop3("test.local", connection.Host, connection.Pop3Port);
        repository.AddSmtp("test.local", connection.Host, connection.SmtpPort);

        var smtpData = NewBotData(repository);
        await SmtpMethods.SmtpAutoConnect(smtpData, connection.PrimaryEmail, 20000);
        await SmtpMethods.SmtpLogin(smtpData, connection.PrimaryEmail, connection.PrimaryPassword, 20000);
        await SmtpMethods.SmtpDisconnect(smtpData);

        var imapData = NewBotData(repository);
        await ImapMethods.ImapAutoConnect(imapData, connection.SecondaryEmail, 20000);
        await ImapMethods.ImapLogin(imapData, connection.SecondaryEmail, connection.SecondaryPassword, openInbox: true, timeoutMilliseconds: 20000);
        await ImapMethods.ImapDisconnect(imapData);

        var pop3Data = NewBotData(repository);
        await Pop3Methods.Pop3AutoConnect(pop3Data, connection.SecondaryEmail, 20000);
        await Pop3Methods.Pop3Login(pop3Data, connection.SecondaryEmail, connection.SecondaryPassword, 20000);
        await Pop3Methods.Pop3Disconnect(pop3Data);
    }

    [Theory]
    [InlineData(ProxyType.Http)]
    [InlineData(ProxyType.Socks4)]
    [InlineData(ProxyType.Socks4a)]
    [InlineData(ProxyType.Socks5)]
    public async Task SmtpConnect_Login_SendMail_ThroughProxy_Verify(ProxyType proxyType)
    {
        await TestMailServer.ResetState();
        var connection = await TestMailServer.GetConnectionInfo();
        var proxy = (await TestMailServer.GetProxyConnectionInfo()).CreateProxy(proxyType);
        var data = NewBotData(proxy: proxy);

        await SmtpMethods.SmtpConnect(data, connection.InternalHost, connection.InternalSmtpPort, 20000);
        await SmtpMethods.SmtpLogin(data, connection.PrimaryEmail, connection.PrimaryPassword, 20000);
        await SmtpMethods.SmtpSendMail(
            data,
            "Proxy Sender",
            connection.PrimaryEmail,
            "Proxy Recipient",
            connection.SecondaryEmail,
            $"smtp-proxy-{proxyType}",
            "smtp-proxy-text",
            "<p>smtp-proxy-html</p>");

        await TestMailServer.WaitForMessageCount(connection.SecondaryEmail, 1);
        var messages = await TestMailServer.GetMessages(connection.SecondaryEmail);
        var log = SmtpMethods.SmtpGetLog(data);

        Assert.Single(messages);
        Assert.Equal($"smtp-proxy-{proxyType}", messages[0].Subject);
        Assert.Contains("AUTH", log, StringComparison.Ordinal);
        Assert.Contains("DATA", log, StringComparison.Ordinal);

        await SmtpMethods.SmtpDisconnect(data);
    }

    [Theory]
    [InlineData(ProxyType.Http)]
    [InlineData(ProxyType.Socks4)]
    [InlineData(ProxyType.Socks4a)]
    [InlineData(ProxyType.Socks5)]
    public async Task ImapConnect_Login_ReadMail_ThroughProxy_Verify(ProxyType proxyType)
    {
        await TestMailServer.ResetState();
        var connection = await TestMailServer.GetConnectionInfo();
        var proxy = (await TestMailServer.GetProxyConnectionInfo()).CreateProxy(proxyType);

        await TestMailServer.SendMail(
            "Sender One",
            connection.PrimaryEmail,
            "Recipient Two",
            connection.SecondaryEmail,
            $"imap-proxy-{proxyType}",
            "imap-proxy-text",
            "<p>imap-proxy-html</p>");

        await TestMailServer.WaitForMessageCount(connection.SecondaryEmail, 1);

        var data = NewBotData(proxy: proxy);
        await ImapMethods.ImapConnect(data, connection.InternalHost, connection.InternalImapPort, 20000);
        await ImapMethods.ImapLogin(data, connection.SecondaryEmail, connection.SecondaryPassword, openInbox: true, timeoutMilliseconds: 20000);

        var ids = await ImapMethods.ImapSearchMails(
            data,
            SearchField.Subject,
            $"imap-proxy-{proxyType}",
            SearchField.From,
            "test1@test.local",
            1);
        var firstMail = await ImapMethods.ImapReadMail(data, ids[0], preferHtml: false);

        Assert.Single(ids);
        Assert.Contains($"Subject: imap-proxy-{proxyType}", firstMail, StringComparison.Ordinal);
        Assert.Contains("imap-proxy-text", firstMail, StringComparison.Ordinal);

        await ImapMethods.ImapDisconnect(data);
    }

    [Theory]
    [InlineData(ProxyType.Http)]
    [InlineData(ProxyType.Socks4)]
    [InlineData(ProxyType.Socks4a)]
    [InlineData(ProxyType.Socks5)]
    public async Task Pop3Connect_Login_ReadMail_ThroughProxy_Verify(ProxyType proxyType)
    {
        await TestMailServer.ResetState();
        var connection = await TestMailServer.GetConnectionInfo();
        var proxy = (await TestMailServer.GetProxyConnectionInfo()).CreateProxy(proxyType);

        await TestMailServer.SendMail(
            "Sender One",
            connection.PrimaryEmail,
            "Recipient Two",
            connection.SecondaryEmail,
            $"pop3-proxy-{proxyType}",
            "pop3-proxy-text",
            "<p>pop3-proxy-html</p>");

        await TestMailServer.WaitForMessageCount(connection.SecondaryEmail, 1);

        var data = NewBotData(proxy: proxy);
        await Pop3Methods.Pop3Connect(data, connection.InternalHost, connection.InternalPop3Port, 20000);
        await Pop3Methods.Pop3Login(data, connection.SecondaryEmail, connection.SecondaryPassword, 20000);

        var mails = await Pop3Methods.Pop3GetMails(data);
        var latestMail = await Pop3Methods.Pop3ReadMail(data, 0, preferHtml: false);

        Assert.Single(mails);
        Assert.StartsWith($"Sender One|Recipient Two|pop3-proxy-{proxyType}", mails[0], StringComparison.Ordinal);
        Assert.Contains($"Subject: pop3-proxy-{proxyType}", latestMail, StringComparison.Ordinal);
        Assert.Contains("pop3-proxy-text", latestMail, StringComparison.Ordinal);

        await Pop3Methods.Pop3Disconnect(data);
    }

    private static BotData NewBotData(IEmailDomainRepository? emailDomains = null, Proxy? proxy = null)
        => new(
            new BotProviders(null!)
            {
                EmailDomains = emailDomains ?? new InMemoryEmailDomainRepository(),
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("mail-test", new WordlistType()),
            proxy,
            proxy is not null)
        {
            CancellationToken = TestCancellationToken
        };
}

[CollectionDefinition(nameof(MailServerCollection), DisableParallelization = true)]
public class MailServerCollection;

internal sealed class InMemoryEmailDomainRepository : IEmailDomainRepository
{
    private readonly Dictionary<string, List<HostEntry>> imapServers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<HostEntry>> pop3Servers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<HostEntry>> smtpServers = new(StringComparer.OrdinalIgnoreCase);

    public Task<IEnumerable<HostEntry>> GetImapServers(string domain)
        => Task.FromResult<IEnumerable<HostEntry>>(Get(imapServers, domain));

    public Task<IEnumerable<HostEntry>> GetPop3Servers(string domain)
        => Task.FromResult<IEnumerable<HostEntry>>(Get(pop3Servers, domain));

    public Task<IEnumerable<HostEntry>> GetSmtpServers(string domain)
        => Task.FromResult<IEnumerable<HostEntry>>(Get(smtpServers, domain));

    public Task TryAddImapServer(string domain, HostEntry server)
    {
        Add(imapServers, domain, server);
        return Task.CompletedTask;
    }

    public Task TryAddPop3Server(string domain, HostEntry server)
    {
        Add(pop3Servers, domain, server);
        return Task.CompletedTask;
    }

    public Task TryAddSmtpServer(string domain, HostEntry server)
    {
        Add(smtpServers, domain, server);
        return Task.CompletedTask;
    }

    public void AddImap(string domain, string host, int port) => Add(imapServers, domain, new HostEntry(host, port));
    public void AddPop3(string domain, string host, int port) => Add(pop3Servers, domain, new HostEntry(host, port));
    public void AddSmtp(string domain, string host, int port) => Add(smtpServers, domain, new HostEntry(host, port));

    private static IReadOnlyList<HostEntry> Get(Dictionary<string, List<HostEntry>> source, string domain)
        => source.TryGetValue(domain, out var list) ? list : [];

    private static void Add(Dictionary<string, List<HostEntry>> source, string domain, HostEntry server)
    {
        if (!source.TryGetValue(domain, out var list))
        {
            list = [];
            source[domain] = list;
        }

        if (!list.Any(entry => entry.Host.Equals(server.Host, StringComparison.OrdinalIgnoreCase) && entry.Port == server.Port))
        {
            list.Add(server);
        }
    }
}
