using OpenBullet2.Core.Helpers;
using System.Net;

namespace OpenBullet2.Web.Tests.Unit.Core;

public class FirewallTests
{
    [Fact]
    public async Task CheckIpValidityAsync_ExactIpv4Match_ReturnsTrue()
    {
        var isValid = await Firewall.CheckIpValidityAsync(
            IPAddress.Parse("192.168.1.1"),
            ["192.168.1.1"]);

        Assert.True(isValid);
    }

    [Fact]
    public async Task CheckIpValidityAsync_ExactIpv4Mismatch_ReturnsFalse()
    {
        var isValid = await Firewall.CheckIpValidityAsync(
            IPAddress.Parse("192.168.1.2"),
            ["192.168.1.1"]);

        Assert.False(isValid);
    }

    [Fact]
    public async Task CheckIpValidityAsync_ExactIpv6Match_ReturnsTrue()
    {
        var isValid = await Firewall.CheckIpValidityAsync(
            IPAddress.IPv6Loopback,
            ["::1"]);

        Assert.True(isValid);
    }

    [Fact]
    public async Task CheckIpValidityAsync_Ipv4SubnetMatch_ReturnsTrue()
    {
        var isValid = await Firewall.CheckIpValidityAsync(
            IPAddress.Parse("10.0.0.42"),
            ["10.0.0.0/24"]);

        Assert.True(isValid);
    }

    [Fact]
    public async Task CheckIpValidityAsync_Ipv4SubnetMismatch_ReturnsFalse()
    {
        var isValid = await Firewall.CheckIpValidityAsync(
            IPAddress.Parse("10.0.1.42"),
            ["10.0.0.0/24"]);

        Assert.False(isValid);
    }

    [Fact]
    public async Task CheckIpValidityAsync_InvalidEntriesAreIgnoredAndLaterValidEntryMatches_ReturnsTrue()
    {
        var isValid = await Firewall.CheckIpValidityAsync(
            IPAddress.Parse("10.0.0.42"),
            ["not-a-host", "300.300.300.300", "10.0.0.0/not-a-mask", "10.0.0.0/24"]);

        Assert.True(isValid);
    }

    [Fact]
    public async Task CheckIpValidityAsync_InvalidEntriesOnly_ReturnsFalse()
    {
        var isValid = await Firewall.CheckIpValidityAsync(
            IPAddress.Parse("10.0.0.1"),
            ["not-a-host", "300.300.300.300", "192.168.1.0/not-a-mask"]);

        Assert.False(isValid);
    }

    [Fact]
    public async Task CheckIpValidityAsync_DynamicDnsMatch_ReturnsTrue()
    {
        var localhost = await Dns.GetHostEntryAsync("localhost", TestContext.Current.CancellationToken);
        var ip = localhost.AddressList.First(a =>
            a.AddressFamily is System.Net.Sockets.AddressFamily.InterNetwork
            or System.Net.Sockets.AddressFamily.InterNetworkV6);

        var isValid = await Firewall.CheckIpValidityAsync(ip, ["localhost"]);

        Assert.True(isValid);
    }

    [Fact]
    public async Task CheckIpValidityAsync_DynamicDnsMismatch_ReturnsFalse()
    {
        var isValid = await Firewall.CheckIpValidityAsync(
            IPAddress.Parse("203.0.113.10"),
            ["localhost"]);

        Assert.False(isValid);
    }
}
