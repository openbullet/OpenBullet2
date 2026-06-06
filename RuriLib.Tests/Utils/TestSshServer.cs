using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Renci.SshNet;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Utils;

internal static class TestSshServer
{
    private const ushort ContainerPort = 2222;
    private const string ContainerImage = "lscr.io/linuxserver/openssh-server:latest";
    private const string ProxyImage = "tarampampam/3proxy:1.12.1";
    private const ushort HttpProxyPort = 3128;
    private const ushort SocksProxyPort = 1080;
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static IContainer? container;
    private static IContainer? proxyContainer;
    private static INetwork? network;
    private static string? skipReason;
    private static string? configDirectory;
    private static SshServerConnectionInfo? connectionInfo;
    private static ProxyContainerConnectionInfo? proxyConnectionInfo;

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    public static async Task<SshServerConnectionInfo> GetConnectionInfo()
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

            configDirectory = Path.Combine(Path.GetTempPath(), $"ob2-ssh-{Guid.NewGuid():N}");
            Directory.CreateDirectory(configDirectory);

            try
            {
                network = new NetworkBuilder()
                    .WithName($"ob2-ssh-{Guid.NewGuid():N}")
                    .Build();

                await network.CreateAsync(TestCancellationToken);

                container = new ContainerBuilder(ContainerImage)
                    .WithNetwork(network)
                    .WithPortBinding(ContainerPort, true)
                    .WithBindMount(configDirectory, "/config")
                    .WithEnvironment("PUID", "1000")
                    .WithEnvironment("PGID", "1000")
                    .WithEnvironment("TZ", "Etc/UTC")
                    .WithEnvironment("PASSWORD_ACCESS", "true")
                    .WithEnvironment("USER_NAME", "ob2-user")
                    .WithEnvironment("USER_PASSWORD", "ob2-password")
                    .WithEnvironment("SUDO_ACCESS", "false")
                    .WithEnvironment("LOG_STDOUT", "true")
                    .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(ContainerPort))
                    .Build();

                await container.StartAsync(TestCancellationToken);

                connectionInfo = new SshServerConnectionInfo(
                    "127.0.0.1",
                    container.GetMappedPublicPort(ContainerPort),
                    container.IpAddress!,
                    ContainerPort,
                    "ob2-user",
                    "ob2-password");

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

    private static async Task WaitUntilReady(SshServerConnectionInfo info)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                using var client = new SshClient(info.Host, info.Port, info.Username, info.Password);
                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
                client.Connect();
                if (client.IsConnected)
                {
                    client.Disconnect();
                    return;
                }
            }
            catch
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(1), TestCancellationToken);
        }

        throw new TimeoutException("Timed out waiting for the local SSH container");
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

        if (configDirectory is not null && Directory.Exists(configDirectory))
        {
            try
            {
                Directory.Delete(configDirectory, true);
            }
            catch
            {
            }

            configDirectory = null;
        }

        if (network is not null)
        {
            await network.DisposeAsync();
            network = null;
        }
    }
}

internal sealed record SshServerConnectionInfo(
    string Host,
    ushort Port,
    string InternalHost,
    ushort InternalPort,
    string Username,
    string Password);
