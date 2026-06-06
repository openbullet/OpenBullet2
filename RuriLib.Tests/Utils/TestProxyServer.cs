using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using RuriLib.Models.Proxies;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Utils;

internal static class TestProxyServer
{
    internal const ushort HttpProxyPort = 3128;
    internal const ushort SocksProxyPort = 1080;
    private const ushort TargetHttpPort = 80;
    private const string ProxyImage = "tarampampam/3proxy:1.12.1";
    private const string HttpBinImage = "kennethreitz/httpbin:latest";
    private const string ProxyNetworkAlias = "proxy";
    private const string AuthenticatedProxyNetworkAlias = "auth-proxy";
    private const string TargetNetworkAlias = "httpbin";
    private const string ProxyUsername = "ob2-user";
    private const string ProxyPassword = "ob2-password";
    private static readonly SemaphoreSlim SyncLock = new(1, 1);

    private static IContainer? proxyContainer;
    private static IContainer? authenticatedProxyContainer;
    private static IContainer? targetContainer;
    private static INetwork? network;
    private static string? skipReason;
    private static ProxyServerConnectionInfo? connectionInfo;

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    public static async Task<ProxyServerConnectionInfo> GetConnectionInfo()
    {
        await EnsureInitialized();
        if (skipReason is not null)
        {
            Assert.Skip(skipReason);
        }

        return connectionInfo!;
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
                    .WithName($"ob2-proxy-{Guid.NewGuid():N}")
                    .Build();

                await network.CreateAsync(TestCancellationToken);

                targetContainer = new ContainerBuilder(HttpBinImage)
                    .WithNetwork(network)
                    .WithNetworkAliases(TargetNetworkAlias)
                    .WithPortBinding(TargetHttpPort, true)
                    .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(TargetHttpPort))
                    .Build();

                await targetContainer.StartAsync(TestCancellationToken);

                var targetBaseUrl = $"http://127.0.0.1:{targetContainer.GetMappedPublicPort(TargetHttpPort)}";
                await WaitUntilHttpBinReady(targetBaseUrl);

                proxyContainer = new ContainerBuilder(ProxyImage)
                    .WithNetwork(network)
                    .WithNetworkAliases(ProxyNetworkAlias)
                    .WithPortBinding(HttpProxyPort, true)
                    .WithPortBinding(SocksProxyPort, true)
                    .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(HttpProxyPort))
                    .Build();

                await proxyContainer.StartAsync(TestCancellationToken);

                authenticatedProxyContainer = new ContainerBuilder(ProxyImage)
                    .WithNetwork(network)
                    .WithNetworkAliases(AuthenticatedProxyNetworkAlias)
                    .WithPortBinding(HttpProxyPort, true)
                    .WithPortBinding(SocksProxyPort, true)
                    .WithEnvironment("PROXY_LOGIN", ProxyUsername)
                    .WithEnvironment("PROXY_PASSWORD", ProxyPassword)
                    .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(HttpProxyPort))
                    .Build();

                await authenticatedProxyContainer.StartAsync(TestCancellationToken);

                var targetIpAddress = targetContainer.IpAddress;
                if (string.IsNullOrWhiteSpace(targetIpAddress))
                {
                    throw new InvalidOperationException("The target HTTP container IP address is not available");
                }

                var proxyIpAddress = proxyContainer.IpAddress;
                if (string.IsNullOrWhiteSpace(proxyIpAddress))
                {
                    throw new InvalidOperationException("The proxy container IP address is not available");
                }

                var authenticatedProxyIpAddress = authenticatedProxyContainer.IpAddress;
                if (string.IsNullOrWhiteSpace(authenticatedProxyIpAddress))
                {
                    throw new InvalidOperationException("The authenticated proxy container IP address is not available");
                }

                connectionInfo = new ProxyServerConnectionInfo(
                    "127.0.0.1",
                    proxyContainer.GetMappedPublicPort(HttpProxyPort),
                    proxyContainer.GetMappedPublicPort(SocksProxyPort),
                    authenticatedProxyContainer.GetMappedPublicPort(HttpProxyPort),
                    authenticatedProxyContainer.GetMappedPublicPort(SocksProxyPort),
                    ProxyUsername,
                    ProxyPassword,
                    targetIpAddress,
                    TargetHttpPort,
                    network,
                    TargetNetworkAlias,
                    ProxyNetworkAlias,
                    AuthenticatedProxyNetworkAlias,
                    proxyIpAddress,
                    authenticatedProxyIpAddress);

                await WaitUntilProxyReady(connectionInfo);
                AppDomain.CurrentDomain.ProcessExit += DisposeResourcesOnProcessExit;
            }
            catch (Exception ex)
            {
                await DisposeResources();
                skipReason = $"Docker is unavailable for {ProxyImage}: {ex.GetType().Name}: {ex.Message}";
            }
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private static async Task WaitUntilHttpBinReady(string baseUrl)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };

        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                using var response = await httpClient.GetAsync($"{baseUrl}/anything", TestCancellationToken);
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

        throw new TimeoutException("Timed out waiting for the local proxy target container");
    }

    private static async Task WaitUntilProxyReady(ProxyServerConnectionInfo info)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                using var httpProxyClient = new TcpClient();
                await httpProxyClient.ConnectAsync(info.Host, info.HttpProxyPort, TestCancellationToken);

                using var socksProxyClient = new TcpClient();
                await socksProxyClient.ConnectAsync(info.Host, info.SocksProxyPort, TestCancellationToken);

                using var authenticatedHttpProxyClient = new TcpClient();
                await authenticatedHttpProxyClient.ConnectAsync(info.Host, info.AuthenticatedHttpProxyPort, TestCancellationToken);

                using var authenticatedSocksProxyClient = new TcpClient();
                await authenticatedSocksProxyClient.ConnectAsync(info.Host, info.AuthenticatedSocksProxyPort, TestCancellationToken);

                return;
            }
            catch
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(1), TestCancellationToken);
        }

        throw new TimeoutException("Timed out waiting for the local proxy container");
    }

    private static void DisposeResourcesOnProcessExit(object? sender, EventArgs e)
        => DisposeResources().GetAwaiter().GetResult();

    private static async Task DisposeResources()
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
            }
        }

        if (authenticatedProxyContainer is not null)
        {
            try
            {
                await authenticatedProxyContainer.DisposeAsync();
            }
            finally
            {
                authenticatedProxyContainer = null;
            }
        }

        if (targetContainer is not null)
        {
            try
            {
                await targetContainer.DisposeAsync();
            }
            finally
            {
                targetContainer = null;
            }
        }

        if (network is not null)
        {
            try
            {
                await network.DisposeAsync();
            }
            finally
            {
                network = null;
            }
        }

        connectionInfo = null;
        AppDomain.CurrentDomain.ProcessExit -= DisposeResourcesOnProcessExit;
    }
}

internal sealed record ProxyServerConnectionInfo(
    string Host,
    ushort HttpProxyPort,
    ushort SocksProxyPort,
    ushort AuthenticatedHttpProxyPort,
    ushort AuthenticatedSocksProxyPort,
    string Username,
    string Password,
    string TargetIpAddress,
    ushort TargetPort,
    INetwork Network,
    string TargetNetworkAlias,
    string ProxyNetworkAlias,
    string AuthenticatedProxyNetworkAlias,
    string ProxyIpAddress,
    string AuthenticatedProxyIpAddress)
{
    public Proxy CreateProxy(ProxyType type)
        => new(Host, type == ProxyType.Http ? HttpProxyPort : SocksProxyPort, type);

    public Proxy CreateContainerProxy(ProxyType type)
        => new(ProxyIpAddress, type == ProxyType.Http ? TestProxyServer.HttpProxyPort : TestProxyServer.SocksProxyPort, type);

    public Proxy CreateAuthenticatedProxy(ProxyType type)
        => new(
            Host,
            type == ProxyType.Http ? AuthenticatedHttpProxyPort : AuthenticatedSocksProxyPort,
            type,
            Username,
            Password);

    public Proxy CreateAuthenticatedContainerProxy(ProxyType type)
        => new(
            AuthenticatedProxyIpAddress,
            type == ProxyType.Http ? TestProxyServer.HttpProxyPort : TestProxyServer.SocksProxyPort,
            type,
            Username,
            Password);

    public string BuildTargetUrl(string relativePath)
        => $"http://{TargetIpAddress}:{TargetPort}/{relativePath.TrimStart('/')}";

    public string BuildContainerTargetUrl(string relativePath)
        => $"http://{TargetNetworkAlias}:{TargetPort}/{relativePath.TrimStart('/')}";
}
