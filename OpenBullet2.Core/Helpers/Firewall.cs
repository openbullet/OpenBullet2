using OpenBullet2.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Helpers;

public static class Firewall
{
    /// <summary>
    /// Checks if an <paramref name="ip"/> is allowed according to a whitelist of <paramref name="allowed"/>
    /// IPs. Supports individual IPv4, individual IPv6, masked IPv4 range, dynamic DNS.
    /// </summary>
    public static async Task<bool> CheckIpValidityAsync(
        IPAddress ip, IEnumerable<string> allowed)
    {
        foreach (var addr in allowed)
        {
            try
            {
                if (IPAddress.TryParse(addr, out var parsed) && ip.Equals(parsed))
                {
                    return true;
                }

                // Check if masked IPv4
                if (addr.Contains('/'))
                {
                    var split = addr.Split('/');

                    if (split.Length == 2
                        && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                        && IPAddress.TryParse(split[0], out var toCompare)
                        && toCompare.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                        && int.TryParse(split[1], out var maskLength)
                        && maskLength is >= 2 and <= 32)
                    {
                        var mask = SubnetMask.CreateByNetBitLength(maskLength);

                        if (ip.IsInSameSubnet(toCompare, mask))
                        {
                            return true;
                        }
                    }

                    continue;
                }

                // Otherwise it must be a dynamic DNS
                var resolved = await Dns.GetHostEntryAsync(addr);
                if (resolved.AddressList.Any(a => a.Equals(ip)))
                {
                    return true;
                }
            }
            catch
            {
                // Skip invalid whitelist entries and continue checking the rest.
            }
        }

        return false;
    }
}
