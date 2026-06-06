using FluentFTP;
using FtpMethods = RuriLib.Blocks.Requests.Ftp.Methods;
using RuriLib.Exceptions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Proxies;
using RuriLib.Tests.Utils;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;
using FtpItemKind = RuriLib.Blocks.Requests.Ftp.FtpItemKind;

namespace RuriLib.Tests.Blocks.Requests;

[Collection(nameof(FtpServerCollection))]
public class FtpRequestBlocksTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public void FtpGetLog_WithoutLogger_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() =>
            FtpMethods.FtpGetLog(data));

        Assert.Equal("No log available. Make sure to connect to a server first!", ex.Message);
    }

    [Fact]
    public async Task FtpListItems_WithoutClient_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            FtpMethods.FtpListItems(data));

        Assert.Equal("Connect to a server first!", ex.Message);
    }

    [Fact]
    public async Task FtpConnect_ListItems_AndGetLog_Verify()
    {
        await TestFtpServer.ResetHomeDirectory();
        await TestFtpServer.CreateDirectory("/folder-a");
        await TestFtpServer.CreateDirectory("/folder-b");
        await TestFtpServer.WriteTextFile("/root.txt", "root-content");
        await TestFtpServer.WriteTextFile("/folder-a/nested.txt", "nested-content");

        var connection = await TestFtpServer.GetConnectionInfo();
        var data = NewBotData();

        await FtpMethods.FtpConnect(
            data,
            connection.Host,
            connection.Port,
            connection.Username,
            connection.Password,
            20000);

        var allItems = await FtpMethods.FtpListItems(data);
        var filesOnly = await FtpMethods.FtpListItems(data, FtpItemKind.File);
        var foldersOnly = await FtpMethods.FtpListItems(data, FtpItemKind.Folder);
        var recursiveFiles = await FtpMethods.FtpListItems(data, FtpItemKind.File, recursive: true);
        var ftpLog = FtpMethods.FtpGetLog(data);

        Assert.Contains(allItems, item => item.EndsWith("/root.txt", StringComparison.Ordinal));
        Assert.Contains(allItems, item => item.EndsWith("/folder-a", StringComparison.Ordinal));
        Assert.Contains(allItems, item => item.EndsWith("/folder-b", StringComparison.Ordinal));
        Assert.DoesNotContain(filesOnly, item => item.EndsWith("/folder-a", StringComparison.Ordinal));
        Assert.Contains(filesOnly, item => item.EndsWith("/root.txt", StringComparison.Ordinal));
        Assert.Contains(foldersOnly, item => item.EndsWith("/folder-a", StringComparison.Ordinal));
        Assert.DoesNotContain(foldersOnly, item => item.EndsWith("/root.txt", StringComparison.Ordinal));
        Assert.Contains(recursiveFiles, item => item.EndsWith("/folder-a/nested.txt", StringComparison.Ordinal));
        Assert.Contains("USER", ftpLog, StringComparison.Ordinal);
        Assert.True(ftpLog.Contains("LIST", StringComparison.Ordinal) || ftpLog.Contains("MLSD", StringComparison.Ordinal));
    }

    [Fact]
    public async Task FtpDownloadFile_DownloadsRemoteContent()
    {
        await TestFtpServer.ResetHomeDirectory();
        await TestFtpServer.WriteTextFile("/remote.txt", "downloaded-content");

        var connection = await TestFtpServer.GetConnectionInfo();
        var data = NewBotData();
        var localFile = Path.Combine(Path.GetTempPath(), $"ob2-download-{Guid.NewGuid():N}.txt");

        try
        {
            await FtpMethods.FtpConnect(
                data,
                connection.Host,
                connection.Port,
                connection.Username,
                connection.Password,
                20000);

            await FtpMethods.FtpDownloadFile(data, "/remote.txt", localFile);

            Assert.Equal("downloaded-content", await File.ReadAllTextAsync(localFile, TestCancellationToken));
        }
        finally
        {
            await FtpMethods.FtpDisconnect(data);
            if (File.Exists(localFile))
            {
                File.Delete(localFile);
            }
        }
    }

    [Fact]
    public async Task FtpDownloadFolder_WithSkip_KeepsExistingLocalFilesAndDownloadsMissingOnes()
    {
        await TestFtpServer.ResetHomeDirectory();
        await TestFtpServer.CreateDirectory("/remote-folder");
        await TestFtpServer.WriteTextFile("/remote-folder/keep.txt", "remote-version");
        await TestFtpServer.WriteTextFile("/remote-folder/new.txt", "new-file");

        var connection = await TestFtpServer.GetConnectionInfo();
        var data = NewBotData();
        var localDirectory = Path.Combine(Path.GetTempPath(), $"ob2-folder-{Guid.NewGuid():N}");

        Directory.CreateDirectory(localDirectory);
        await File.WriteAllTextAsync(Path.Combine(localDirectory, "keep.txt"), "local-version", TestCancellationToken);

        try
        {
            await FtpMethods.FtpConnect(
                data,
                connection.Host,
                connection.Port,
                connection.Username,
                connection.Password,
                20000);

            await FtpMethods.FtpDownloadFolder(
                data,
                "/remote-folder",
                localDirectory,
                FtpLocalExists.Skip);

            Assert.Equal("local-version", await File.ReadAllTextAsync(Path.Combine(localDirectory, "keep.txt"), TestCancellationToken));
            Assert.Equal("new-file", await File.ReadAllTextAsync(Path.Combine(localDirectory, "new.txt"), TestCancellationToken));
        }
        finally
        {
            await FtpMethods.FtpDisconnect(data);
            if (Directory.Exists(localDirectory))
            {
                Directory.Delete(localDirectory, true);
            }
        }
    }

    [Fact]
    public async Task FtpUploadFile_OverwritesRemoteFile_AndDisconnectsClient()
    {
        await TestFtpServer.ResetHomeDirectory();
        await TestFtpServer.WriteTextFile("/uploaded.txt", "old-content");

        var connection = await TestFtpServer.GetConnectionInfo();
        var data = NewBotData();
        var localFile = Path.Combine(Path.GetTempPath(), $"ob2-upload-{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(localFile, "new-content", TestCancellationToken);

        try
        {
            await FtpMethods.FtpConnect(
                data,
                connection.Host,
                connection.Port,
                connection.Username,
                connection.Password,
                20000);

            await FtpMethods.FtpUploadFile(data, "/uploaded.txt", localFile);

            Assert.Equal("new-content", await TestFtpServer.ReadTextFile("/uploaded.txt"));

            await FtpMethods.FtpDisconnect(data);

            var client = data.TryGetObject<AsyncFtpClient>("ftpClient");
            Assert.NotNull(client);
            Assert.False(client!.IsConnected);
        }
        finally
        {
            if (File.Exists(localFile))
            {
                File.Delete(localFile);
            }
        }
    }

    [Fact]
    public async Task FtpDisconnect_WithoutClient_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            FtpMethods.FtpDisconnect(data));

        Assert.Equal("Connect to a server first!", ex.Message);
    }

    [Fact]
    public async Task FtpConnect_WithWrongPassword_Throws()
    {
        var connection = await TestFtpServer.GetConnectionInfo();
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<FluentFTP.Exceptions.FtpAuthenticationException>(() =>
            FtpMethods.FtpConnect(
                data,
                connection.Host,
                connection.Port,
                connection.Username,
                "wrong-password",
                20000));
        Assert.Contains("530", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Login authentication failed", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FtpConnect_ToUnusedPort_Throws()
    {
        var data = NewBotData();
        var unusedPort = FindUnusedTcpPort();

        var ex = await Record.ExceptionAsync(() =>
            FtpMethods.FtpConnect(
                data,
                "127.0.0.1",
                unusedPort,
                "user",
                "password",
                1000));

        Assert.NotNull(ex);
        Assert.True(
            ex is TimeoutException ||
            ex is BlockExecutionException,
            $"Unexpected exception type: {ex.GetType().FullName}");

        if (ex is TimeoutException timeout)
        {
            Assert.StartsWith("Timed out trying to connect", timeout.Message, StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal("Failed to connect to the FTP server with the given credentials", ex.Message);
        }
    }

    [Theory]
    [InlineData(ProxyType.Http)]
    [InlineData(ProxyType.Socks4)]
    [InlineData(ProxyType.Socks5)]
    public async Task FtpConnect_ThroughProxy_AndGetLog_Verify(ProxyType proxyType)
    {
        var connection = await TestFtpServer.GetConnectionInfo();
        var proxy = (await TestFtpServer.GetProxyConnectionInfo()).CreateProxy(proxyType);
        var data = NewBotData(proxy);
        var targetHost = proxyType == ProxyType.Socks4a
            ? connection.InternalAlias
            : connection.InternalHost;

        await FtpMethods.FtpConnect(
            data,
            targetHost,
            connection.InternalPort,
            connection.Username,
            connection.Password,
            20000);

        var client = data.TryGetObject<AsyncFtpClient>("ftpClient");
        var ftpLog = FtpMethods.FtpGetLog(data);

        Assert.NotNull(client);
        Assert.True(client!.IsConnected);
        Assert.Contains("USER", ftpLog, StringComparison.Ordinal);

        await FtpMethods.FtpDisconnect(data);
    }

    private static BotData NewBotData(Proxy? proxy = null)
        => new(
            new BotProviders(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("ftp-test", new WordlistType()),
            proxy,
            proxy is not null)
        {
            CancellationToken = TestCancellationToken
        };

    private static int FindUnusedTcpPort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }
}

[CollectionDefinition(nameof(FtpServerCollection), DisableParallelization = true)]
public class FtpServerCollection;
