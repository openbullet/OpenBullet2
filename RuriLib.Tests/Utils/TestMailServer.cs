using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Utils;

internal static class TestMailServer
{
    private const string ContainerImage = "greenmail/standalone:2.1.8";
    private const string ProxyImage = "tarampampam/3proxy:1.12.1";
    private const ushort SmtpPort = 3025;
    private const ushort Pop3Port = 3110;
    private const ushort ImapPort = 3143;
    private const ushort ApiPort = 8080;
    private const ushort HttpProxyPort = 3128;
    private const ushort SocksProxyPort = 1080;
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static IContainer? container;
    private static IContainer? proxyContainer;
    private static INetwork? network;
    private static string? skipReason;
    private static MailServerConnectionInfo? connectionInfo;
    private static ProxyContainerConnectionInfo? proxyConnectionInfo;

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    public static async Task<MailServerConnectionInfo> GetConnectionInfo()
    {
        await EnsureInitialized();
        if (skipReason is not null)
        {
            Assert.Skip(skipReason);
        }

        return connectionInfo!;
    }

    public static async Task<ProxyContainerConnectionInfo> GetProxyConnectionInfo()
    {
        await EnsureInitialized();
        if (skipReason is not null)
        {
            Assert.Skip(skipReason);
        }

        await EnsureProxyInitialized();
        return proxyConnectionInfo!;
    }

    public static async Task ResetState()
    {
        var connection = await GetConnectionInfo();
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        using var response = await httpClient.PostAsync($"{connection.ApiBaseUrl}/api/mail/purge", null, TestCancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public static async Task SendMail(string fromName, string fromAddress, string toName, string toAddress, string subject, string textBody, string htmlBody)
    {
        var connection = await GetConnectionInfo();

        using var client = new SmtpClient();
        await client.ConnectAsync(connection.Host, connection.SmtpPort, MailKit.Security.SecureSocketOptions.Auto, TestCancellationToken);
        client.AuthenticationMechanisms.Remove("XOAUTH2");
        await client.AuthenticateAsync(fromAddress, GetPassword(fromAddress), TestCancellationToken);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(toName, toAddress));
        message.Subject = subject;
        message.Body = new BodyBuilder
        {
            TextBody = textBody,
            HtmlBody = htmlBody
        }.ToMessageBody();

        await client.SendAsync(message, TestCancellationToken);
        await client.DisconnectAsync(true, TestCancellationToken);
    }

    public static async Task<IReadOnlyList<GreenMailMessageInfo>> GetMessages(string emailAddress, string folderName = "INBOX")
    {
        var connection = await GetConnectionInfo();
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        var url = $"{connection.ApiBaseUrl}/api/user/{Uri.EscapeDataString(emailAddress)}/messages/{Uri.EscapeDataString(folderName)}";
        using var response = await httpClient.GetAsync(url, TestCancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestCancellationToken);
        return JsonConvert.DeserializeObject<List<GreenMailMessageInfo>>(content) ?? [];
    }

    public static async Task WaitForMessageCount(string emailAddress, int expectedCount, string folderName = "INBOX")
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if ((await GetMessages(emailAddress, folderName)).Count == expectedCount)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), TestCancellationToken);
        }

        throw new TimeoutException($"Timed out waiting for {expectedCount} messages in {folderName} for {emailAddress}");
    }

    private static async Task EnsureInitialized()
    {
        if (connectionInfo is not null || skipReason is not null)
        {
            return;
        }

        await SyncLock.WaitAsync(TestCancellationToken);
        try
        {
            if (connectionInfo is not null || skipReason is not null)
            {
                return;
            }

            try
            {
                network = new NetworkBuilder()
                    .WithName($"ob2-mail-{Guid.NewGuid():N}")
                    .Build();

                await network.CreateAsync(TestCancellationToken);

                container = new ContainerBuilder(ContainerImage)
                    .WithNetwork(network)
                    .WithPortBinding(SmtpPort, true)
                    .WithPortBinding(Pop3Port, true)
                    .WithPortBinding(ImapPort, true)
                    .WithPortBinding(ApiPort, true)
                    .WithEnvironment("GREENMAIL_OPTS", "-Dgreenmail.setup.test.all -Dgreenmail.hostname=0.0.0.0")
                    .Build();

                await container.StartAsync(TestCancellationToken);

                connectionInfo = new MailServerConnectionInfo(
                    "127.0.0.1",
                    container.IpAddress!,
                    container.GetMappedPublicPort(SmtpPort),
                    container.GetMappedPublicPort(Pop3Port),
                    container.GetMappedPublicPort(ImapPort),
                    container.GetMappedPublicPort(ApiPort),
                    "test1@test.local",
                    "pwd1",
                    "test2@test.local",
                    "pwd2");

                await WaitUntilReady(connectionInfo);
                await EnsureUsers(connectionInfo);
                AppDomain.CurrentDomain.ProcessExit += DisposeContainerOnProcessExit;
            }
            catch (Exception ex)
            {
                await DisposeContainer();
                skipReason = $"Docker is unavailable for {ContainerImage}: {ex.GetType().Name}: {ex.Message}";
            }
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private static async Task EnsureProxyInitialized()
    {
        if (proxyConnectionInfo is not null)
        {
            return;
        }

        await SyncLock.WaitAsync(TestCancellationToken);
        try
        {
            if (proxyConnectionInfo is not null)
            {
                return;
            }

            proxyContainer = new ContainerBuilder(ProxyImage)
                .WithNetwork(network!)
                .WithPortBinding(HttpProxyPort, true)
                .WithPortBinding(SocksProxyPort, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(HttpProxyPort))
                .Build();

            await proxyContainer.StartAsync(TestCancellationToken);

            proxyConnectionInfo = new ProxyContainerConnectionInfo(
                "127.0.0.1",
                proxyContainer.GetMappedPublicPort(HttpProxyPort),
                proxyContainer.GetMappedPublicPort(SocksProxyPort));
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private static async Task WaitUntilReady(MailServerConnectionInfo connection)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                using var response = await httpClient.GetAsync($"{connection.ApiBaseUrl}/api/service/readiness", TestCancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(1), TestCancellationToken);
        }

        throw new TimeoutException("Timed out waiting for the local mail container");
    }

    private static async Task EnsureUsers(MailServerConnectionInfo connection)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        await CreateUser(httpClient, connection.ApiBaseUrl, connection.PrimaryEmail, connection.PrimaryPassword);
        await CreateUser(httpClient, connection.ApiBaseUrl, connection.SecondaryEmail, connection.SecondaryPassword);
    }

    private static async Task CreateUser(HttpClient httpClient, string apiBaseUrl, string emailAddress, string password)
    {
        using var response = await httpClient.PostAsJsonAsync($"{apiBaseUrl}/api/user", new
        {
            email = emailAddress,
            login = emailAddress,
            password
        }, TestCancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static string GetPassword(string emailAddress) => emailAddress switch
    {
        "test1@test.local" => "pwd1",
        "test2@test.local" => "pwd2",
        _ => throw new InvalidOperationException($"No password configured for {emailAddress}")
    };

    private static void DisposeContainerOnProcessExit(object? sender, EventArgs e)
        => DisposeContainer().GetAwaiter().GetResult();

    private static async Task DisposeContainer()
    {
        if (proxyContainer is not null)
        {
            try
            {
                await proxyContainer.DisposeAsync();
            }
            finally
            {
                proxyContainer = null;
                proxyConnectionInfo = null;
            }
        }

        if (container is null)
        {
            if (network is not null)
            {
                await network.DisposeAsync();
                network = null;
            }

            return;
        }

        try
        {
            await container.DisposeAsync();
        }
        finally
        {
            AppDomain.CurrentDomain.ProcessExit -= DisposeContainerOnProcessExit;
            container = null;
        }

        if (network is not null)
        {
            await network.DisposeAsync();
            network = null;
        }
    }
}

internal sealed record MailServerConnectionInfo(
    string Host,
    string InternalHost,
    ushort SmtpPort,
    ushort Pop3Port,
    ushort ImapPort,
    ushort ApiPort,
    string PrimaryEmail,
    string PrimaryPassword,
    string SecondaryEmail,
    string SecondaryPassword)
{
    public string ApiBaseUrl => $"http://{Host}:{ApiPort}";
    public ushort InternalSmtpPort => 3025;
    public ushort InternalPop3Port => 3110;
    public ushort InternalImapPort => 3143;
}

internal sealed class GreenMailMessageInfo
{
    public double Uid { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string MimeMessage { get; set; } = string.Empty;
}
