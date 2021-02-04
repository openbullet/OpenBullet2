using RuriLib.Proxies.Exceptions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RuriLib.Proxies.Helpers
{
    static internal class HostHelper
    {
        public static byte[] GetPortBytes(int port)
        {
            var array = new byte[2];

            array[0] = (byte)(port / 256);
            array[1] = (byte)(port % 256);

            return array;
        }

        public static byte[] GetIPAddressBytes(string destinationHost, bool preferIpv4 = true)
        {
            if (!IPAddress.TryParse(destinationHost, out var ipAddr))
            {
                try
                {
                    var ips = Dns.GetHostAddresses(destinationHost);

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

                        ipAddr = ips[0];
                    }
                }
                catch (Exception ex)
                {
                    if (ex is SocketException || ex is ArgumentException)
                    {
                        throw new ProxyException("Failed to get host address", ex);
                    }

                    throw;
                }
            }

            return ipAddr.GetAddressBytes();
        }
    }
}
