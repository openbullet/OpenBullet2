using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using FluentFTP;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Utils;

internal static class TestFtpServer
{
    private const ushort ControlPort = 21;
    private const ushort HttpProxyPort = 3128;
    private const ushort SocksProxyPort = 1080;
    private const string ContainerImage = "stilliard/pure-ftpd:trixie-latest";
    private const string ProxyImage = "tarampampam/3proxy:1.12.1";
    private const string NetworkAlias = "ob2-ftp";
    private const string Username = "ob2-user";
    private const string Password = "ob2-password";
    private const string ContainerHomeDirectory = "/home/ftpusers/ob2-user";
    private const int PassivePortCount = 10;
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static IContainer? container;
    private static IContainer? proxyContainer;
    private static INetwork? network;
    private static string? skipReason;
    private static string? homeDirectory;
    private static FtpServerConnectionInfo? connectionInfo;
    private static ProxyContainerConnectionInfo? proxyConnectionInfo;

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    public static async Task<FtpServerConnectionInfo> GetConnectionInfo()
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

    public static async Task ResetHomeDirectory()
    {
        var connection = await GetConnectionInfo();
        await using var client = await ConnectClient(connection);
        var entries = await client.GetListing("/", FtpListOption.Recursive).ConfigureAwait(false);

        foreach (var file in entries.Where(e => e.Type == FtpObjectType.File).OrderByDescending(e => e.FullName.Length))
        {
            await client.DeleteFile(file.FullName, TestCancellationToken).ConfigureAwait(false);
        }

        foreach (var directory in entries.Where(e => e.Type == FtpObjectType.Directory).OrderByDescending(e => e.FullName.Length))
        {
            await client.DeleteDirectory(directory.FullName, TestCancellationToken).ConfigureAwait(false);
        }
    }

    public static async Task CreateDirectory(string remoteDirectory)
    {
        var connection = await GetConnectionInfo();
        await using var client = await ConnectClient(connection);
        await client.CreateDirectory(remoteDirectory).ConfigureAwait(false);
    }

    public static async Task WriteTextFile(string remotePath, string content)
    {
        var connection = await GetConnectionInfo();
        var tempFile = Path.Combine(Path.GetTempPath(), $"ob2-ftp-seed-{Guid.NewGuid():N}.txt");

        try
        {
            await File.WriteAllTextAsync(tempFile, content, Encoding.UTF8, TestCancellationToken);

            await using var client = await ConnectClient(connection);
            await client.UploadFile(tempFile, remotePath, FtpRemoteExists.Overwrite, true, FtpVerify.None, null, TestCancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    public static async Task<string> ReadTextFile(string remotePath)
    {
        var connection = await GetConnectionInfo();
        var tempFile = Path.Combine(Path.GetTempPath(), $"ob2-ftp-read-{Guid.NewGuid():N}.txt");

        try
        {
            await using var client = await ConnectClient(connection);
            await client.DownloadFile(tempFile, remotePath, FtpLocalExists.Overwrite, FtpVerify.None, null, TestCancellationToken).ConfigureAwait(false);
            return await File.ReadAllTextAsync(tempFile, TestCancellationToken);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
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

            var passivePortStart = FindFreePortRangeStart(30000, 40000, PassivePortCount);
            var passivePortEnd = passivePortStart + PassivePortCount - 1;
            homeDirectory = Path.Combine(Path.GetTempPath(), $"ob2-ftp-{Guid.NewGuid():N}");
            Directory.CreateDirectory(homeDirectory);

            try
            {
                network = new NetworkBuilder()
                    .WithName($"ob2-ftp-{Guid.NewGuid():N}")
                    .Build();

                await network.CreateAsync(TestCancellationToken);

                var builder = new ContainerBuilder(ContainerImage)
                    .WithNetwork(network)
                    .WithNetworkAliases(NetworkAlias)
                    .WithPortBinding(ControlPort, true)
                    .WithBindMount(homeDirectory, ContainerHomeDirectory)
                    .WithEnvironment("PUBLICHOST", "127.0.0.1")
                    .WithEnvironment("FTP_USER_NAME", Username)
                    .WithEnvironment("FTP_USER_PASS", Password)
                    .WithEnvironment("FTP_USER_HOME", ContainerHomeDirectory)
                    .WithEnvironment("FTP_PASSIVE_PORTS", $"{passivePortStart}:{passivePortEnd}")
                    .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(ControlPort));

                for (var port = passivePortStart; port <= passivePortEnd; port++)
                {
                    builder = builder.WithPortBinding((ushort)port, (ushort)port);
                }

                container = builder.Build();

                await container.StartAsync(TestCancellationToken);

                connectionInfo = new FtpServerConnectionInfo(
                    "127.0.0.1",
                    container.GetMappedPublicPort(ControlPort),
                    container.IpAddress!,
                    NetworkAlias,
                    ControlPort,
                    Username,
                    Password);

                await WaitUntilReady(connectionInfo);
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

    private static async Task WaitUntilReady(FtpServerConnectionInfo info)
    {
        for (var attempt = 0; attempt < 6; attempt++)
        {
            await using var client = CreateClient(info);

            try
            {
                await client.AutoConnect(TestCancellationToken).ConfigureAwait(false);
                if (client.IsConnected)
                {
                    return;
                }
            }
            catch
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(1), TestCancellationToken);
        }

        throw new TimeoutException("Timed out waiting for the local FTP container");
    }

    private static AsyncFtpClient CreateClient(FtpServerConnectionInfo info)
        => new(
            info.Host,
            new NetworkCredential(info.Username, info.Password),
            info.Port,
            new FtpConfig
            {
                ConnectTimeout = 15000,
                ReadTimeout = 15000,
                DataConnectionConnectTimeout = 15000,
                DataConnectionReadTimeout = 15000
            });

    private static async Task<AsyncFtpClient> ConnectClient(FtpServerConnectionInfo info)
    {
        var client = CreateClient(info);
        await client.AutoConnect(TestCancellationToken).ConfigureAwait(false);
        return client;
    }

    private static int FindFreePortRangeStart(int startInclusive, int endExclusive, int count)
    {
        for (var start = startInclusive; start <= endExclusive - count; start++)
        {
            if (Enumerable.Range(start, count).All(IsPortAvailable))
            {
                return start;
            }
        }

        throw new InvalidOperationException($"Could not find {count} contiguous free ports in the range {startInclusive}-{endExclusive - 1}");
    }

    private static bool IsPortAvailable(int port)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

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

        if (container is not null)
        {
            try
            {
                await container.DisposeAsync();
            }
            finally
            {
                AppDomain.CurrentDomain.ProcessExit -= DisposeContainerOnProcessExit;
                container = null;
            }
        }

        if (network is not null)
        {
            await network.DisposeAsync();
            network = null;
        }

        if (homeDirectory is not null && Directory.Exists(homeDirectory))
        {
            Directory.Delete(homeDirectory, true);
            homeDirectory = null;
        }
    }
}

internal sealed record FtpServerConnectionInfo(
    string Host,
    ushort Port,
    string InternalHost,
    string InternalAlias,
    ushort InternalPort,
    string Username,
    string Password);
