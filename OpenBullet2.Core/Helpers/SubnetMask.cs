using System;
using System.Net;

namespace OpenBullet2.Core.Helpers
{
    public static class SubnetMask
    {
        public static readonly IPAddress ClassA = IPAddress.Parse("255.0.0.0");
        public static readonly IPAddress ClassB = IPAddress.Parse("255.255.0.0");
        public static readonly IPAddress ClassC = IPAddress.Parse("255.255.255.0");

        public static IPAddress CreateByHostBitLength(int hostpartLength)
        {
            var hostPartLength = hostpartLength;
            var netPartLength = 32 - hostPartLength;

            if (netPartLength < 2)
            {
                throw new ArgumentException("Number of hosts is too large for IPv4");
            }

            var binaryMask = new byte[4];

            for (var i = 0; i < 4; i++)
            {
                if (i * 8 + 8 <= netPartLength)
                {
                    binaryMask[i] = 255;
                }
                else if (i * 8 > netPartLength)
                {
                    binaryMask[i] = 0;
                }
                else
                {
                    var oneLength = netPartLength - i * 8;
                    var binaryDigit = string.Empty.PadLeft(oneLength, '1').PadRight(8, '0');
                    binaryMask[i] = Convert.ToByte(binaryDigit, 2);
                }
            }
            return new IPAddress(binaryMask);
        }

        public static IPAddress CreateByNetBitLength(int netpartLength)
        {
            var hostPartLength = 32 - netpartLength;
            return CreateByHostBitLength(hostPartLength);
        }

        public static IPAddress CreateByHostNumber(int numberOfHosts)
        {
            var maxNumber = numberOfHosts + 1;
            var b = Convert.ToString(maxNumber, 2);
            return CreateByHostBitLength(b.Length);
        }
    }
}
