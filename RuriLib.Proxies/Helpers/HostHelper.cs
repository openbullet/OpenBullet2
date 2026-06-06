using RuriLib.Proxies.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Proxies.Helpers;

internal static class HostHelper
{
    public static byte[] GetPortBytes(int port)
    {
        var array = new byte[2];

        array[0] = (byte)(port / 256);
        array[1] = (byte)(port % 256);

        return array;
    }

    public static async Task<byte[]> GetIpAddressBytesAsync(string destinationHost, bool preferIpv4 = true,
        CancellationToken cancellationToken = default)
    {
        var ips = await GetHostAddressesAsync(destinationHost, cancellationToken).ConfigureAwait(false);

        try
        {
            if (ips.Length > 0)
            {
                if (preferIpv4)
                {
                    foreach (var ip in ips)
                    {
                        var ipBytes = ip.GetAddressBytes();
                        if (ipBytes.Length == 4)
                        {
                            return ipBytes;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is SocketException or ArgumentException)
            {
                throw new ProxyException("Failed to get host address", ex);
            }

            throw;
        }

        throw new ProxyException("Failed to get host address");
    }

    public static async Task<IPAddress[]> GetHostAddressesAsync(string host, CancellationToken cancellationToken = default)
    {
        if (IPAddress.TryParse(host, out var ipAddr))
        {
            return [ipAddr];
        }

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false);
            return OrderAddresses(addresses);
        }
        catch (Exception ex)
        {
            if (ex is SocketException or ArgumentException)
            {
                throw new ProxyException("Failed to get host address", ex);
            }

            throw;
        }
    }

    internal static IPAddress[] OrderAddresses(IEnumerable<IPAddress> addresses)
        => addresses
            .OrderBy(GetAddressPriority)
            .ToArray();

    private static int GetAddressPriority(IPAddress address)
        => address.AddressFamily switch
        {
            AddressFamily.InterNetwork => 0,
            AddressFamily.InterNetworkV6 => 1,
            _ => 2
        };
}
