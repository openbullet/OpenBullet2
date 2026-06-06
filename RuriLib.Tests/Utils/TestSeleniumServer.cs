using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Utils;

public enum TestSeleniumBrowser
{
    Chromium,
    Firefox
}

internal static class TestSeleniumServer
{
    private const ushort WebDriverPort = 4444;
    private static readonly SemaphoreSlim SyncLock = new(1, 1);

    private static readonly Dictionary<string, BrowserState> States = [];

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    public static async Task<Uri> GetServerUrl(INetwork network, TestSeleniumBrowser browser, string pool = "default")
    {
        var stateKey = GetStateKey(browser, pool);
        if (!States.TryGetValue(stateKey, out var state))
        {
            state = new(GetContainerImage(browser));
            States[stateKey] = state;
        }

        await EnsureInitialized(network, state);
        if (state.SkipReason is not null)
        {
            Assert.Skip(state.SkipReason);
        }

        return state.ServerUrl!;
    }

    private static string GetStateKey(TestSeleniumBrowser browser, string pool)
        => $"{browser}:{pool}";

    private static string GetContainerImage(TestSeleniumBrowser browser)
        => browser switch
        {
            TestSeleniumBrowser.Chromium => "selenium/standalone-chromium:4.43.0-20260404",
            TestSeleniumBrowser.Firefox => "selenium/standalone-firefox:4.43.0-20260404",
            _ => throw new NotSupportedException($"Unsupported Selenium browser {browser}")
        };

    private static async Task EnsureInitialized(INetwork network, BrowserState state)
    {
        if (state.ServerUrl is not null || state.SkipReason is not null)
        {
            return;
        }

        await SyncLock.WaitAsync(TestCancellationToken);
        try
        {
            if (state.ServerUrl is not null || state.SkipReason is not null)
            {
                return;
            }

            try
            {
                state.Container = new ContainerBuilder(state.ContainerImage)
                    .WithNetwork(network)
                    .WithPortBinding(WebDriverPort, true)
                    .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(WebDriverPort))
                    .Build();

                await state.Container.StartAsync(TestCancellationToken);

                var candidateServerUrl = new Uri($"http://127.0.0.1:{state.Container.GetMappedPublicPort(WebDriverPort)}/wd/hub");
                await WaitUntilReady(candidateServerUrl);

                state.ServerUrl = candidateServerUrl;
                AppDomain.CurrentDomain.ProcessExit += state.DisposeContainerOnProcessExit;
            }
            catch (Exception ex)
            {
                await state.DisposeContainer();
                state.SkipReason = $"Docker is unavailable for {state.ContainerImage}: {ex.GetType().Name}: {ex.Message}";
            }
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private static async Task WaitUntilReady(Uri candidateServerUrl)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
        var statusUrl = new Uri(candidateServerUrl, "/status");

        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                using var response = await httpClient.GetAsync(statusUrl, TestCancellationToken);
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

        throw new TimeoutException("Timed out waiting for the Selenium container");
    }

    private sealed class BrowserState(string containerImage)
    {
        public string ContainerImage { get; } = containerImage;
        public IContainer? Container { get; set; }
        public Uri? ServerUrl { get; set; }
        public string? SkipReason { get; set; }

        public void DisposeContainerOnProcessExit(object? sender, EventArgs e)
            => DisposeContainer().GetAwaiter().GetResult();

        public async Task DisposeContainer()
        {
            if (Container is null)
            {
                return;
            }

            try
            {
                await Container.DisposeAsync();
            }
            finally
            {
                AppDomain.CurrentDomain.ProcessExit -= DisposeContainerOnProcessExit;
                Container = null;
            }
        }
    }
}
