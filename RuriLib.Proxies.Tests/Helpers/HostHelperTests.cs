using RuriLib.Proxies.Helpers;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Proxies.Tests.Helpers;

public class HostHelperTests
{
    [Fact]
    public async Task GetHostAddressesAsync_Cancelled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await HostHelper.GetHostAddressesAsync("localhost", cts.Token));
    }

    [Fact]
    public async Task GetIpAddressBytesAsync_Cancelled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await HostHelper.GetIpAddressBytesAsync("localhost", cancellationToken: cts.Token));
    }

    [Fact]
    public void OrderAddresses_PrefersIpv4BeforeIpv6()
    {
        var ipv6 = IPAddress.Parse("2001:db8::1");
        var ipv4 = IPAddress.Parse("192.0.2.10");

        var ordered = HostHelper.OrderAddresses([ipv6, ipv4]);

        Assert.Equal([ipv4, ipv6], ordered);
    }

    [Fact]
    public void OrderAddresses_PreservesTheOriginalOrderWithinTheSameAddressFamily()
    {
        var ipv4A = IPAddress.Parse("192.0.2.10");
        var ipv6 = IPAddress.Parse("2001:db8::1");
        var ipv4B = IPAddress.Parse("192.0.2.20");

        var ordered = HostHelper.OrderAddresses([ipv4A, ipv6, ipv4B]);

        Assert.Equal([ipv4A, ipv4B, ipv6], ordered);
    }
}
