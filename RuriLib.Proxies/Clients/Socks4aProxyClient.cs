using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using RuriLib.Proxies.Helpers;
using static RuriLib.Proxies.Clients.Socks4Constants;

namespace RuriLib.Proxies.Clients
{
    /// <summary>
    /// A client that provides proxies connections via SOCKS4a proxies.
    /// </summary>
    public class Socks4aProxyClient : Socks4ProxyClient
    {
        /// <summary>
        /// Creates an SOCKS4a proxy client given the proxy <paramref name="settings"/>.
        /// </summary>
        public Socks4aProxyClient(ProxySettings settings) : base(settings)
        {

        }

        /// <inheritdoc/>
        protected async override Task RequestConnectionAsync(NetworkStream nStream, byte command, string destinationHost, int destinationPort,
            CancellationToken cancellationToken = default)
        {
            var dstPort = HostHelper.GetPortBytes(destinationPort);
            var userId = Array.Empty<byte>();
            
            if (Settings.Credentials != null && !string.IsNullOrEmpty(Settings.Credentials.UserName))
            {
                userId = Encoding.ASCII.GetBytes(Settings.Credentials.UserName);
            }

            var dstAddr = Encoding.ASCII.GetBytes(destinationHost);

            // REQUEST GRANT
            // +----+----+----+----+----+----+----+----+----+----+....+----+----+----+....+----+
            // | VN | CD | DSTPORT |      DSTIP        | USERID       |NULL| DSTADDR      |NULL|
            // +----+----+----+----+----+----+----+----+----+----+....+----+----+----+....+----+
            //    1    1      2              4           variable       1    variable        1 
            var request = new byte[10 + userId.Length + dstAddr.Length];

            request[0] = VersionNumber;
            request[1] = command;
            dstPort.CopyTo(request, 2);
            byte[] dstIp = { 0, 0, 0, 1 };
            dstIp.CopyTo(request, 4);
            userId.CopyTo(request, 8);
            request[8 + userId.Length] = 0x00;
            dstAddr.CopyTo(request, 9 + userId.Length);
            request[9 + userId.Length + dstAddr.Length] = 0x00;

            await nStream.WriteAsync(request.AsMemory(0, request.Length), cancellationToken).ConfigureAwait(false);

            // READ RESPONSE
            // +----+----+----+----+----+----+----+----+
            // | VN | CD | DSTPORT |      DSTIP        |
            // +----+----+----+----+----+----+----+----+
            //    1    1      2              4
            var response = new byte[8];

            await nStream.ReadAsync(response.AsMemory(0, 8), cancellationToken).ConfigureAwait(false);

            var reply = response[1];

            // Если запрос не выполнен.
            if (reply != CommandReplyRequestGranted)
            {
                HandleCommandError(reply);
            }
        }
    }
}
