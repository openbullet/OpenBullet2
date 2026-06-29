using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Proxies;

public class ProxyPoolTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task RemoveDuplicates_ListWithDuplicates_ReturnDistinct()
    {
        ListProxySource source = new(new Proxy[]
        {
            new("127.0.0.1", 8000),
            new("127.0.0.1", 8000)
        });

        using var pool = new ProxyPool(new ProxySource[] { source });

        await pool.ReloadAllAsync(cancellationToken: TestCancellationToken);
        pool.RemoveDuplicates();
        Assert.Single(pool.Proxies);
    }

    [Fact]
    public async Task RemoveDuplicates_SameIdentityDifferentRuntimeState_ReturnDistinct()
    {
        ListProxySource source = new([
            new("127.0.0.1", 8000, username: "user", password: "pass")
            {
                ProxyStatus = ProxyStatus.Busy,
                WorkingStatus = ProxyWorkingStatus.Working,
                Country = "IT",
                Ping = 42
            },
            new("127.0.0.1", 8000, username: "user", password: "pass")
            {
                ProxyStatus = ProxyStatus.Banned,
                WorkingStatus = ProxyWorkingStatus.NotWorking,
                Country = "US",
                Ping = 999
            }
        ]);

        using var pool = new ProxyPool([source]);

        await pool.ReloadAllAsync(false, TestCancellationToken);
        pool.RemoveDuplicates();

        Assert.Single(pool.Proxies);
    }

    [Fact]
    public async Task RemoveDuplicates_DifferentIdentity_KeepsDistinct()
    {
        ListProxySource source = new([
            new("127.0.0.1", 8000, ProxyType.Http, "user", "pass"),
            new("127.0.0.1", 8000, ProxyType.Socks5, "user", "pass"),
            new("127.0.0.1", 8000, ProxyType.Http, "other-user", "pass")
        ]);

        using var pool = new ProxyPool([source]);

        await pool.ReloadAllAsync(false, TestCancellationToken);
        pool.RemoveDuplicates();

        Assert.Equal(3, pool.Proxies.Count());
    }

    [Fact]
    public async Task ReloadAllAsync_RemovesDuplicatesAcrossSources()
    {
        ListProxySource firstSource = new([
            new("127.0.0.1", 8000),
            new("127.0.0.2", 8001)
        ]);

        ListProxySource secondSource = new([
            new("127.0.0.1", 8000),
            new("127.0.0.3", 8002)
        ]);

        using var pool = new ProxyPool([firstSource, secondSource]);

        await pool.ReloadAllAsync(false, TestCancellationToken);

        var proxies = pool.Proxies.ToArray();
        Assert.Equal(3, proxies.Length);
        Assert.Single(proxies, p => p.Host == "127.0.0.1" && p.Port == 8000);
    }

    [Fact]
    public async Task GetProxy_Available_ReturnValidProxy()
    {
        ListProxySource source = new(new Proxy[]
        {
            new("127.0.0.1", 8000)
        });

        using var pool = new ProxyPool(new ProxySource[] { source });

        await pool.ReloadAllAsync(cancellationToken: TestCancellationToken);
        Assert.NotNull(pool.GetProxy());
    }

    [Fact]
    public async Task GetProxy_AllBusy_ReturnNull()
    {
        ListProxySource source = new(new Proxy[]
        {
            new("127.0.0.1", 8000) { ProxyStatus = ProxyStatus.Busy }
        });

        using var pool = new ProxyPool(new ProxySource[] { source });

        await pool.ReloadAllAsync(cancellationToken: TestCancellationToken);
        Assert.Null(pool.GetProxy());
    }

    [Fact]
    public async Task GetProxy_EvenBusy_ReturnValidProxy()
    {
        ListProxySource source = new(new Proxy[]
        {
            new("127.0.0.1", 8000) { ProxyStatus = ProxyStatus.Busy }
        });

        using var pool = new ProxyPool(new ProxySource[] { source });

        await pool.ReloadAllAsync(cancellationToken: TestCancellationToken);
        Assert.NotNull(pool.GetProxy(true));
    }

    [Fact]
    public async Task GetProxy_MaxUses_ReturnNull()
    {
        ListProxySource source = new(new Proxy[]
        {
            new("127.0.0.1", 8000) { TotalUses = 3 }
        });

        using var pool = new ProxyPool(new ProxySource[] { source });

        await pool.ReloadAllAsync(cancellationToken: TestCancellationToken);
        Assert.Null(pool.GetProxy(true, 3));
    }

    [Fact]
    public async Task ReloadAllAsync_FiltersOutDisallowedProxyTypes()
    {
        ListProxySource source = new([
            new("127.0.0.1", 8000, ProxyType.Http),
            new("127.0.0.1", 9000, ProxyType.Socks5)
        ]);

        using var pool = new ProxyPool([source], new ProxyPoolOptions
        {
            AllowedTypes = [ProxyType.Socks5]
        });

        await pool.ReloadAllAsync(false, TestCancellationToken);

        var proxies = pool.Proxies.ToArray();
        Assert.Single(proxies);
        Assert.Equal(ProxyType.Socks5, proxies[0].Type);
    }

    [Fact]
    public async Task ReloadAllOnceAsync_EmptySource_ReturnsFalseWithoutBackoff()
    {
        ListProxySource source = new([]);

        using var pool = new ProxyPool([source]);

        var reloaded = await pool.ReloadAllOnceAsync(false, TestCancellationToken);

        Assert.False(reloaded);
        Assert.Empty(pool.Proxies);
    }

    [Fact]
    public async Task Proxies_SnapshotEnumeration_RemainsValidAfterShuffle()
    {
        ListProxySource source = new([
            new("127.0.0.1", 8000),
            new("127.0.0.2", 8001),
            new("127.0.0.3", 8002)
        ]);

        using var pool = new ProxyPool([source]);

        await pool.ReloadAllAsync(false, TestCancellationToken);

        var snapshot = pool.Proxies;
        pool.Shuffle();

        var proxies = snapshot.ToArray();

        Assert.Equal(3, proxies.Length);
        Assert.Contains(proxies, p => p.Host == "127.0.0.1" && p.Port == 8000);
        Assert.Contains(proxies, p => p.Host == "127.0.0.2" && p.Port == 8001);
        Assert.Contains(proxies, p => p.Host == "127.0.0.3" && p.Port == 8002);
    }

    [Fact]
    public async Task ReleaseProxy_Ban_SetsBannedStatusAndTimestamp()
    {
        ListProxySource source = new([new Proxy("127.0.0.1", 8000)]);

        using var pool = new ProxyPool([source]);

        await pool.ReloadAllAsync(false, TestCancellationToken);
        var proxy = pool.GetProxy();

        Assert.NotNull(proxy);

        pool.ReleaseProxy(proxy, ban: true);

        Assert.Equal(ProxyStatus.Banned, proxy.ProxyStatus);
        Assert.NotNull(proxy.LastBanned);
        Assert.Equal(1, proxy.TotalUses);
    }

    [Fact]
    public async Task UnbanAll_BadProxy_RemainsBad()
    {
        var proxy = new Proxy("127.0.0.1", 8000)
        {
            ProxyStatus = ProxyStatus.Bad,
            LastBanned = DateTime.Now.Subtract(TimeSpan.FromMinutes(5))
        };
        ListProxySource source = new([proxy]);

        using var pool = new ProxyPool([source]);

        await pool.ReloadAllAsync(false, TestCancellationToken);
        pool.UnbanAll(TimeSpan.Zero);

        Assert.Equal(ProxyStatus.Bad, proxy.ProxyStatus);
    }

    [Fact(Timeout = 10000)]
    public async Task GetProxy_BatchFile_ReturnValidProxy()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Well, Only Windows contains Powershell.
            return;
        }

        var tmpBatchFilePath = Path.GetTempFileName() + ".bat";
        await File.WriteAllTextAsync(tmpBatchFilePath, @"@echo off

echo 127.0.0.1:1111
echo 127.0.0.1:2222
echo (Socks5)127.0.0.1:3333
", Encoding.UTF8, TestCancellationToken);
        using FileProxySource source = new(tmpBatchFilePath);

        using var pool = new ProxyPool(new ProxySource[] { source });

        await pool.ReloadAllAsync(false, TestCancellationToken);
        File.Delete(tmpBatchFilePath);
        Assert.Equal(3, pool.Proxies.Count());
        var proxy = pool.GetProxy();
        Assert.NotNull(proxy);
        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(1111, proxy.Port);
        proxy = pool.GetProxy();
        Assert.NotNull(proxy);
        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(2222, proxy.Port);
        proxy = pool.GetProxy();
        Assert.NotNull(proxy);
        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(3333, proxy.Port);
        Assert.Equal(ProxyType.Socks5, proxy.Type);
    }

    [Fact(Timeout = 30000)]
    public async Task GetProxy_PowershellFile_ReturnValidProxy()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Well, Only Windows contains Powershell.
            return;
        }

        var tmpPowerShellFilePath = Path.GetTempFileName() + ".ps1";
        await File.WriteAllTextAsync(tmpPowerShellFilePath, @"
Write-Output 127.0.0.1:1111
Write-Output 127.0.0.1:2222
Write-Output ""(Socks5)127.0.0.1:3333""
", Encoding.UTF8, TestCancellationToken);
        using FileProxySource source = new(tmpPowerShellFilePath);

        using var pool = new ProxyPool([source]);

        await pool.ReloadAllAsync(false, TestCancellationToken);
        File.Delete(tmpPowerShellFilePath);
        Assert.Equal(3, pool.Proxies.Count());
        var proxy = pool.GetProxy();
        Assert.NotNull(proxy);
        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(1111, proxy.Port);
        proxy = pool.GetProxy();
        Assert.NotNull(proxy);
        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(2222, proxy.Port);
        proxy = pool.GetProxy();
        Assert.NotNull(proxy);
        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(3333, proxy.Port);
        Assert.Equal(ProxyType.Socks5, proxy.Type);
    }

    [Fact(Timeout = 10000, Skip = "Randomly fails on CI")]
    public async Task GetProxy_BashFile_ReturnValidProxy()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Well, Windows doesn't have bash by default.
            return;
        }

        var tmpBashFilePath = Path.GetTempFileName() + ".sh";
        await File.WriteAllTextAsync(tmpBashFilePath, @"#!/bin/bash
echo 127.0.0.1:1111
echo 127.0.0.1:2222
echo ""(Socks5)127.0.0.1:3333""
", Encoding.UTF8, TestCancellationToken);
        using FileProxySource source = new(tmpBashFilePath);

        using var pool = new ProxyPool([source]);

        await pool.ReloadAllAsync(false, TestCancellationToken);
        File.Delete(tmpBashFilePath);
        Assert.Equal(3, pool.Proxies.Count());
        var proxy = pool.GetProxy();
        Assert.NotNull(proxy);
        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(1111, proxy.Port);
        proxy = pool.GetProxy();
        Assert.NotNull(proxy);
        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(2222, proxy.Port);
        proxy = pool.GetProxy();
        Assert.NotNull(proxy);
        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(3333, proxy.Port);
        Assert.Equal(ProxyType.Socks5, proxy.Type);
    }
}
