using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Utils;

internal static class TestHttpBin
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    private const ushort ContainerPort = 80;
    private const string ContainerImage = "kennethreitz/httpbin:latest";
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static IContainer? container;
    private static string? baseUrl;
    private static string? skipReason;

    public static async Task<string> BuildUrl(string relativePath)
        => $"{(await GetBaseUrl()).TrimEnd('/')}/{relativePath.TrimStart('/')}";

    public static async Task<string> GetHostHeader()
        => new Uri(await GetBaseUrl()).Authority;

    private static async Task<string> GetBaseUrl()
    {
        await EnsureInitialized();
        if (skipReason is not null)
        {
            Assert.Skip(skipReason);
        }

        return baseUrl!;
    }

    private static async Task EnsureInitialized()
    {
        if (baseUrl is not null || skipReason is not null)
        {
            return;
        }

        await SyncLock.WaitAsync(TestCancellationToken);
        try
        {
            if (baseUrl is not null || skipReason is not null)
            {
                return;
            }

            try
            {
                container = new ContainerBuilder(ContainerImage)
                    .WithPortBinding(ContainerPort, true)
                    .Build();

                await container.StartAsync();

                var mappedPort = container.GetMappedPublicPort(ContainerPort);
                var candidateBaseUrl = $"http://127.0.0.1:{mappedPort}";
                await WaitUntilReady(candidateBaseUrl);

                baseUrl = candidateBaseUrl;
                AppDomain.CurrentDomain.ProcessExit += DisposeContainerOnProcessExit;
            }
            catch (Exception ex)
            {
                await DisposeContainer();
                skipReason = $"Docker is unavailable for {ContainerImage}: {ex.GetType().Name}";
            }
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private static async Task WaitUntilReady(string candidateBaseUrl)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };

        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                using var response = await httpClient.GetAsync($"{candidateBaseUrl}/anything", TestCancellationToken);
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

        throw new TimeoutException("Timed out waiting for the local httpbin container");
    }

    private static void DisposeContainerOnProcessExit(object? sender, EventArgs e)
        => DisposeContainer().GetAwaiter().GetResult();

    private static async Task DisposeContainer()
    {
        if (container is null)
        {
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
    }
}
