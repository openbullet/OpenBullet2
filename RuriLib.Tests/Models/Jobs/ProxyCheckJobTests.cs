using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Jobs;

public class ProxyCheckJobTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public void Defaults_AreSafe()
    {
        var job = CreateJob();

        Assert.Equal(1, job.Bots);
        Assert.Equal("https://google.com", job.Url);
        Assert.Equal("title>Google", job.SuccessKey);
        Assert.Null(job.Proxies);
        Assert.Null(job.ProxyOutput);
        Assert.Null(job.GeoProvider);
        Assert.Equal(TimeSpan.FromSeconds(10), job.Timeout);
        Assert.True(job.UseProxyJudge);
    }

    [Fact]
    public async Task Start_WithoutProxies_Throws()
    {
        var job = CreateJob();
        job.ProxyOutput = new NullProxyCheckOutput();

        var exception = await Assert.ThrowsAsync<NullReferenceException>(() => job.Start(TestCancellationToken));

        Assert.Equal("The proxy list cannot be null", exception.Message);
    }

    [Fact]
    public async Task Start_WithoutOutput_Throws()
    {
        var job = CreateJob();
        job.Proxies = new List<Proxy>();

        var exception = await Assert.ThrowsAsync<NullReferenceException>(() => job.Start(TestCancellationToken));

        Assert.Equal("The proxy check output cannot be null", exception.Message);
    }

    [Fact]
    public async Task ChangeBots_WithoutParallelizer_StillUpdatesBots()
    {
        var job = CreateJob();

        await job.ChangeBots(5);

        Assert.Equal(5, job.Bots);
    }

    [Fact]
    public void AzenvProxyJudge_Transparent_WhenForwardedIpIsPresent()
    {
        var judge = new AzenvProxyJudge();
        const string content = """
            HTTP_X_FORWARDED_FOR = 198.51.100.10
            HTTP_VIA = 1.1 proxy
            REMOTE_ADDR = 203.0.113.5
            """;

        var success = judge.TryClassify(content, out var quality);

        Assert.True(success);
        Assert.Equal(ProxyQuality.Transparent, quality);
    }

    [Fact]
    public void AzenvProxyJudge_Anonymous_WhenViaIsPresentWithoutIpLeak()
    {
        var judge = new AzenvProxyJudge();
        const string content = """
            HTTP_VIA = 1.1 proxy
            REMOTE_ADDR = 203.0.113.5
            """;

        var success = judge.TryClassify(content, out var quality);

        Assert.True(success);
        Assert.Equal(ProxyQuality.Anonymous, quality);
    }

    [Fact]
    public void AzenvProxyJudge_Elite_WhenOnlyRemoteAddrIsPresent()
    {
        var judge = new AzenvProxyJudge();
        const string content = """
            HTTP_HOST = azenv.net
            REMOTE_ADDR = 203.0.113.5
            REQUEST_METHOD = GET
            """;

        var success = judge.TryClassify(content, out var quality);

        Assert.True(success);
        Assert.Equal(ProxyQuality.Elite, quality);
    }

    private static ProxyCheckJob CreateJob()
        => new(CreateSettingsService(), CreatePluginRepository());

    private static global::RuriLib.Services.RuriLibSettingsService CreateSettingsService()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-proxycheck-settings-{Guid.NewGuid():N}"));

    private static global::RuriLib.Services.PluginRepository CreatePluginRepository()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-proxycheck-plugins-{Guid.NewGuid():N}"));

    private sealed class NullProxyCheckOutput : IProxyCheckOutput
    {
        public Task StoreAsync(Proxy proxy) => Task.CompletedTask;
    }
}
