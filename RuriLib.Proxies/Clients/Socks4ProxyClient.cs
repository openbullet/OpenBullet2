using System;
using System.IO;
using System.Text;
using System.Net.Sockets;

using static RuriLib.Proxies.Clients.Socks4Constants;
using RuriLib.Proxies.Helpers;
using RuriLib.Proxies.Exceptions;
using System.Threading.Tasks;
using System.Threading;

namespace RuriLib.Proxies.Clients;

internal static class Socks4Constants
{
    public const byte VersionNumber = 4;
    public const byte CommandConnect = 0x01;
    public const byte CommandBind = 0x02;
    public const byte CommandReplyRequestGranted = 0x5a;
    public const byte CommandReplyRequestRejectedOrFailed = 0x5b;
    public const byte CommandReplyRequestRejectedCannotConnectToIdentd = 0x5c;
    public const byte CommandReplyRequestRejectedDifferentIdentd = 0x5d;
}

/// <summary>
/// A client that provides proxies connections via SOCKS4 proxies.
/// </summary>
public class Socks4ProxyClient : ProxyClient
{
    /// <summary>
    /// Creates an SOCKS4 proxy client given the proxy <paramref name="settings"/>.
    /// </summary>
    public Socks4ProxyClient(ProxySettings settings) : base(settings)
    {

    }

    /// <inheritdoc/>
    protected override async Task CreateConnectionAsync(TcpClient client, string destinationHost, int destinationPort,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(destinationHost);

        if (!PortHelper.ValidateTcpPort(destinationPort))
        {
            throw new ArgumentOutOfRangeException(nameof(destinationPort));
        }

        if (client is not { Connected: true })
        {
            throw new SocketException();
        }

        try
        {
            await RequestConnectionAsync(client.GetStream(), CommandConnect, destinationHost, destinationPort, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            client.Close();

            if (ex is IOException or SocketException)
            {
                throw new ProxyException("Error while working with proxy", ex);
            }

            throw;
        }
    }

    /// <summary>
    /// Requests SOCKS4 connection.
    /// </summary>
    protected virtual async Task RequestConnectionAsync(NetworkStream nStream, byte command, string destinationHost, int destinationPort,
        CancellationToken cancellationToken = default)
    {
        var dstIp = await HostHelper.GetIpAddressBytesAsync(destinationHost);
        var dstPort = HostHelper.GetPortBytes(destinationPort);

        var userId = Array.Empty<byte>();

        // Set the credentials if needed
        if (Settings.Credentials != null && !string.IsNullOrEmpty(Settings.Credentials.UserName))
        {
            userId = Encoding.ASCII.GetBytes(Settings.Credentials.UserName);
        }

        // REQUEST GRANT
        // +----+----+----+----+----+----+----+----+----+----+....+----+
        // | VN | CD | DSTPORT |      DSTIP        | USERID       |NULL|
        // +----+----+----+----+----+----+----+----+----+----+....+----+
        //    1    1      2              4           variable       1
        var request = new byte[9 + userId.Length];

        request[0] = VersionNumber;
        request[1] = command;
        dstPort.CopyTo(request, 2);
        dstIp.CopyTo(request, 4);
        userId.CopyTo(request, 8);
        request[8 + userId.Length] = 0x00;

        await nStream.WriteAsync(request.AsMemory(0, request.Length), cancellationToken).ConfigureAwait(false);

        // READ RESPONSE
        // +----+----+----+----+----+----+----+----+
        // | VN | CD | DSTPORT |      DSTIP        |
        // +----+----+----+----+----+----+----+----+
        //   1    1       2              4
        var response = new byte[8];

        var bytesRead = await nStream.ReadAsync(response.AsMemory(0, response.Length), cancellationToken).ConfigureAwait(false);
        
        if (bytesRead != response.Length)
        {
            throw new ProxyException("The proxy server did not respond correctly");
        }

        var reply = response[1];

        if (reply != CommandReplyRequestGranted)
        {
            HandleCommandError(reply);
        }
    }

    /// <summary>
    /// Handles a command error.
    /// </summary>
    protected static void HandleCommandError(byte command)
    {
        var errorMessage = command switch
        {
            CommandReplyRequestRejectedOrFailed => "Request rejected or failed",
            CommandReplyRequestRejectedCannotConnectToIdentd => "Request rejected: cannot connect to identd",
            CommandReplyRequestRejectedDifferentIdentd => "Request rejected: different identd",
            _ => "Unknown socks error"
        };

        throw new ProxyException(errorMessage);
    }
}
