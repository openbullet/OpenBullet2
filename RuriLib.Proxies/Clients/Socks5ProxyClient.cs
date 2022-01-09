using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using RuriLib.Proxies.Helpers;
using RuriLib.Proxies.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using static RuriLib.Proxies.Clients.Socks5Constants;

namespace RuriLib.Proxies.Clients
{
    static internal class Socks5Constants
    {
        public const byte VersionNumber = 5;
        public const byte Reserved = 0x00;
        public const byte AuthMethodNoAuthenticationRequired = 0x00;
        public const byte AuthMethodGssapi = 0x01;
        public const byte AuthMethodUsernamePassword = 0x02;
        public const byte AuthMethodIanaAssignedRangeBegin = 0x03;
        public const byte AuthMethodIanaAssignedRangeEnd = 0x7f;
        public const byte AuthMethodReservedRangeBegin = 0x80;
        public const byte AuthMethodReservedRangeEnd = 0xfe;
        public const byte AuthMethodReplyNoAcceptableMethods = 0xff;
        public const byte CommandConnect = 0x01;
        public const byte CommandBind = 0x02;
        public const byte CommandUdpAssociate = 0x03;
        public const byte CommandReplySucceeded = 0x00;
        public const byte CommandReplyGeneralSocksServerFailure = 0x01;
        public const byte CommandReplyConnectionNotAllowedByRuleset = 0x02;
        public const byte CommandReplyNetworkUnreachable = 0x03;
        public const byte CommandReplyHostUnreachable = 0x04;
        public const byte CommandReplyConnectionRefused = 0x05;
        public const byte CommandReplyTTLExpired = 0x06;
        public const byte CommandReplyCommandNotSupported = 0x07;
        public const byte CommandReplyAddressTypeNotSupported = 0x08;
        public const byte AddressTypeIPV4 = 0x01;
        public const byte AddressTypeDomainName = 0x03;
        public const byte AddressTypeIPV6 = 0x04;
    }

    /// <summary>
    /// A client that provides proxies connections via SOCKS5 proxies.
    /// </summary>
    public class Socks5ProxyClient : ProxyClient
    {
        /// <summary>
        /// Creates an SOCKS5 proxy client given the proxy <paramref name="settings"/>.
        /// </summary>
        public Socks5ProxyClient(ProxySettings settings) : base(settings)
        {

        }

        /// <inheritdoc/>
        protected async override Task CreateConnectionAsync(TcpClient client, string destinationHost, int destinationPort,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(destinationHost))
            {
                throw new ArgumentException(null, nameof(destinationHost));
            }

            if (!PortHelper.ValidateTcpPort(destinationPort))
            {
                throw new ArgumentOutOfRangeException(nameof(destinationPort));
            }

            if (client == null || !client.Connected)
            {
                throw new SocketException();
            }

            try
            {
                var nStream = client.GetStream();

                await NegotiateAsync(nStream, cancellationToken).ConfigureAwait(false);
                await RequestConnectionAsync(nStream, CommandConnect, destinationHost, destinationPort, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                client.Close();

                if (ex is IOException || ex is SocketException)
                {
                    throw new ProxyException("Error while working with proxy", ex);
                }

                throw;
            }
        }

        private async Task NegotiateAsync(NetworkStream nStream, CancellationToken cancellationToken = default)
        {
            var authMethod = Settings.Credentials != null && !string.IsNullOrEmpty(Settings.Credentials.UserName)
                ? AuthMethodUsernamePassword
                : AuthMethodNoAuthenticationRequired;

            // INITIATE NEGOTIATION
            // +----+----------+----------+
            // |VER | NMETHODS | METHODS  |
            // +----+----------+----------+
            // | 1  |    1     | 1 to 255 |
            // +----+----------+----------+
            var request = new byte[3];

            request[0] = VersionNumber;
            request[1] = 1;
            request[2] = authMethod;

            await nStream.WriteAsync(request.AsMemory(0, request.Length), cancellationToken).ConfigureAwait(false);

            // READ RESPONSE
            // +----+--------+
            // |VER | METHOD |
            // +----+--------+
            // | 1  |   1    |
            // +----+--------+
            var response = new byte[2];

            await nStream.ReadAsync(response.AsMemory(0, response.Length), cancellationToken).ConfigureAwait(false);

            var reply = response[1];

            if (authMethod == AuthMethodUsernamePassword && reply == AuthMethodUsernamePassword)
            {
                await SendUsernameAndPasswordAsync(nStream, cancellationToken).ConfigureAwait(false);
            }
            else if (reply != CommandReplySucceeded)
            {
                HandleCommandError(reply);
            }
        }

        private async Task SendUsernameAndPasswordAsync(NetworkStream nStream, CancellationToken cancellationToken = default)
        {
            var uname = string.IsNullOrEmpty(Settings.Credentials.UserName)
                ? Array.Empty<byte>()
                : Encoding.ASCII.GetBytes(Settings.Credentials.UserName);

            var passwd = string.IsNullOrEmpty(Settings.Credentials.Password)
                ? Array.Empty<byte>()
                : Encoding.ASCII.GetBytes(Settings.Credentials.Password);

            // SEND CREDENTIALS
            // +----+------+----------+------+----------+
            // |VER | ULEN |  UNAME   | PLEN |  PASSWD  |
            // +----+------+----------+------+----------+
            // | 1  |  1   | 1 to 255 |  1   | 1 to 255 |
            // +----+------+----------+------+----------+
            var request = new byte[uname.Length + passwd.Length + 3];

            request[0] = 1;
            request[1] = (byte)uname.Length;
            uname.CopyTo(request, 2);
            request[2 + uname.Length] = (byte)passwd.Length;
            passwd.CopyTo(request, 3 + uname.Length);

            await nStream.WriteAsync(request.AsMemory(0, request.Length), cancellationToken).ConfigureAwait(false);

            // READ RESPONSE
            // +----+--------+
            // |VER | STATUS |
            // +----+--------+
            // | 1  |   1    |
            // +----+--------+
            var response = new byte[2];

            await nStream.ReadAsync(response.AsMemory(0, response.Length), cancellationToken).ConfigureAwait(false);

            var reply = response[1];

            if (reply != CommandReplySucceeded)
            {
                throw new ProxyException("Unable to authenticate proxy-server");
            }
        }

        private static async Task RequestConnectionAsync(NetworkStream nStream, byte command, string destinationHost, int destinationPort,
            CancellationToken cancellationToken = default)
        {
            var aTyp = GetAddressType(destinationHost);
            var dstAddr = GetHostAddressBytes(aTyp, destinationHost);
            var dstPort = HostHelper.GetPortBytes(destinationPort);

            // REQUEST GRANT
            // +----+-----+-------+------+----------+----------+
            // |VER | CMD |  RSV  | ATYP | DST.ADDR | DST.PORT |
            // +----+-----+-------+------+----------+----------+
            // | 1  |  1  | X'00' |  1   | Variable |    2     |
            // +----+-----+-------+------+----------+----------+
            var request = new byte[4 + dstAddr.Length + 2];

            request[0] = VersionNumber;
            request[1] = command;
            request[2] = Reserved;
            request[3] = aTyp;
            dstAddr.CopyTo(request, 4);
            dstPort.CopyTo(request, 4 + dstAddr.Length);

            await nStream.WriteAsync(request.AsMemory(0, request.Length), cancellationToken).ConfigureAwait(false);

            // READ RESPONSE
            // +----+-----+-------+------+----------+----------+
            // |VER | REP |  RSV  | ATYP | BND.ADDR | BND.PORT |
            // +----+-----+-------+------+----------+----------+
            // | 1  |  1  | X'00' |  1   | Variable |    2     |
            // +----+-----+-------+------+----------+----------+
            var response = new byte[255];

            await nStream.ReadAsync(response.AsMemory(0, response.Length), cancellationToken).ConfigureAwait(false);

            var reply = response[1];
            if (reply != CommandReplySucceeded)
            {
                HandleCommandError(reply);
            }
        }

        private static byte GetAddressType(string host)
        {

            if (!IPAddress.TryParse(host, out var ipAddr))
            {
                return AddressTypeDomainName;
            }

            return ipAddr.AddressFamily switch
            {
                AddressFamily.InterNetwork => AddressTypeIPV4,
                AddressFamily.InterNetworkV6 => AddressTypeIPV6,
                _ => throw new ProxyException(string.Format("Not supported address type {0}", host))
            };
        }

        private static void HandleCommandError(byte command)
        {
            var errorMessage = command switch
            {
                AuthMethodReplyNoAcceptableMethods => "Auth failed: not acceptable method",
                CommandReplyGeneralSocksServerFailure => "General socks server failure",
                CommandReplyConnectionNotAllowedByRuleset => "Connection not allowed by ruleset",
                CommandReplyNetworkUnreachable => "Network unreachable",
                CommandReplyHostUnreachable => "Host unreachable",
                CommandReplyConnectionRefused => "Connection refused",
                CommandReplyTTLExpired => "TTL Expired",
                CommandReplyCommandNotSupported => "Command not supported",
                CommandReplyAddressTypeNotSupported => "Address type not supported",
                _ => "Unknown socks error"
            };

            throw new ProxyException(errorMessage);
        }

        private static byte[] GetHostAddressBytes(byte addressType, string host)
        {
            switch (addressType)
            {
                case AddressTypeIPV4:
                case AddressTypeIPV6:
                    return IPAddress.Parse(host).GetAddressBytes();

                case AddressTypeDomainName:
                    var bytes = new byte[host.Length + 1];

                    bytes[0] = (byte)host.Length;
                    Encoding.ASCII.GetBytes(host).CopyTo(bytes, 1);

                    return bytes;

                default:
                    return null;
            }
        }
    }
}
