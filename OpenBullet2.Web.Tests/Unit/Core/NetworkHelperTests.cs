using OpenBullet2.Core.Extensions;
using OpenBullet2.Core.Helpers;
using System.Net;

namespace OpenBullet2.Web.Tests.Unit.Core;

public class NetworkHelperTests
{
    [Fact]
    public void SubnetMask_CreateByLengths_ReturnsExpectedMasks()
    {
        Assert.Equal(IPAddress.Parse("255.255.255.0"), SubnetMask.CreateByNetBitLength(24));
        Assert.Equal(IPAddress.Parse("255.255.255.0"), SubnetMask.CreateByHostBitLength(8));
        Assert.Equal(IPAddress.Parse("255.255.255.252"), SubnetMask.CreateByHostNumber(2));
    }

    [Fact]
    public void SubnetMask_CreateByTooManyHosts_Throws()
        => Assert.Throws<ArgumentException>(() => SubnetMask.CreateByHostBitLength(31));

    [Fact]
    public void IpAddressExtensions_CalculateNetworkAndBroadcast()
    {
        var address = IPAddress.Parse("192.168.1.10");
        var mask = IPAddress.Parse("255.255.255.0");

        Assert.Equal(IPAddress.Parse("192.168.1.0"), address.GetNetworkAddress(mask));
        Assert.Equal(IPAddress.Parse("192.168.1.255"), address.GetBroadcastAddress(mask));
        Assert.True(IPAddress.Parse("192.168.1.200").IsInSameSubnet(address, mask));
        Assert.False(IPAddress.Parse("192.168.2.1").IsInSameSubnet(address, mask));
    }

    [Fact]
    public void IpAddressExtensions_MismatchedAddressFamilies_Throw()
    {
        var address = IPAddress.Parse("192.168.1.10");
        var ipv6Mask = IPAddress.Parse("ffff:ffff:ffff:ffff::");

        Assert.Throws<ArgumentException>(() => address.GetNetworkAddress(ipv6Mask));
        Assert.Throws<ArgumentException>(() => address.GetBroadcastAddress(ipv6Mask));
    }
}
