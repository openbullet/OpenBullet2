using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Proxies
{
    public class ProxyPoolTests
    {
        [Fact]
        public async Task RemoveDuplicates_ListWithDuplicates_ReturnDistinct()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000),
                new Proxy("127.0.0.1", 8000)
            });

            using var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAllAsync();
            pool.RemoveDuplicates();
            Assert.Single(pool.Proxies);
        }

        [Fact]
        public async Task GetProxy_Available_ReturnValidProxy()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000)
            });

            using var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAllAsync();
            Assert.NotNull(pool.GetProxy());
        }

        [Fact]
        public async Task GetProxy_AllBusy_ReturnNull()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { ProxyStatus = ProxyStatus.Busy }
            });

            using var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAllAsync();
            Assert.Null(pool.GetProxy());
        }

        [Fact]
        public async Task GetProxy_EvenBusy_ReturnValidProxy()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { ProxyStatus = ProxyStatus.Busy }
            });

            using var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAllAsync();
            Assert.NotNull(pool.GetProxy(true));
        }

        [Fact]
        public async Task GetProxy_MaxUses_ReturnNull()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { TotalUses = 3 }
            });

            using var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAllAsync();
            Assert.Null(pool.GetProxy(true, 3));
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
", Encoding.UTF8);
            using FileProxySource source = new(tmpBatchFilePath);

            using var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAllAsync(false);
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

        [Fact(Timeout = 10000)]
        public async Task GetProxy_PowershellFile_ReturnValidProxy()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Well, Only Windows contains Powershell.
                return;
            }

            var tmpBatchFilePath = Path.GetTempFileName() + ".ps1";
            // Setting Execution Policy is needed both in the test and real-world use cases of the functionality.
            // users can use "Set-ExecutionPolicy unrestricted -Scope CurrentUser" apply for all scripts.
            string command = $"/c powershell -executionpolicy unrestricted \"${tmpBatchFilePath}\"";
            System.Diagnostics.Process.Start("cmd.exe", command);

            await File.WriteAllTextAsync(tmpBatchFilePath, @"
Write-Output 127.0.0.1:1111
Write-Output 127.0.0.1:2222
Write-Output ""(Socks5)127.0.0.1:3333""
", Encoding.UTF8);
            using FileProxySource source = new(tmpBatchFilePath);

            using var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAllAsync(false);
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

        [Fact(Timeout = 10000)]
        public async Task GetProxy_BashFile_ReturnValidProxy()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Well, Windows doesn't have shell.
                return;
            }
            var tmpBatchFilePath = Path.GetTempFileName() + ".sh";
            await File.WriteAllTextAsync(tmpBatchFilePath, @"#!/bin/bash
echo 127.0.0.1:1111
echo 127.0.0.1:2222
echo ""(Socks5)127.0.0.1:3333""
", Encoding.UTF8);
            using FileProxySource source = new(tmpBatchFilePath);

            using var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAllAsync(false);
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
    }
}
