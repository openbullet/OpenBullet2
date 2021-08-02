using OpenBullet2.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Helpers
{
    public static class Firewall
    {
        // TODO: Write unit tests.
        /// <summary>
        /// Checks if an <paramref name="ip"/> is allowed according to a whitelist of <paramref name="allowed"/>
        /// IPs. Supports individual IPv4, individual IPv6, masked IPv4 range, dynamic DNS.
        /// </summary>
        public static async Task<bool> CheckIpValidity(IPAddress ip, IEnumerable<string> allowed)
        {
            foreach (var addr in allowed)
            {
                try
                {
                    // Check if standard IPv4 or IPv6
                    if (Regex.Match(addr, @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}$").Success ||
                        Regex.Match(addr, @"^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$").Success)
                    {
                        if (ip.Equals(IPAddress.Parse(addr)))
                            return true;
                    }

                    // Check if masked IPv4
                    if (addr.Contains('/'))
                    {
                        var split = addr.Split('/');
                        var maskLength = int.Parse(split[1]);
                        var toCompare = IPAddress.Parse(split[0]);
                        var mask = SubnetMask.CreateByNetBitLength(maskLength);

                        if (ip.IsInSameSubnet(toCompare, mask))
                            return true;
                    }

                    // Otherwise it must be a dynamic DNS
                    var resolved = await Dns.GetHostEntryAsync(addr);
                    if (resolved.AddressList.Any(a => a.Equals(ip)))
                        return true;
                }
                catch
                {

                }
            }

            return false;
        }
    }
}
