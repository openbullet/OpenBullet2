using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RuriLib.Proxies.Helpers;
using RuriLib.Proxies.Exceptions;

namespace RuriLib.Proxies.Clients;

/// <summary>
/// A client that provides proxies connections via HTTP proxies.
/// </summary>
public partial class HttpProxyClient : ProxyClient
{
    /// <summary>
    /// The HTTP version to send in the first line of the request to the proxy.
    /// By default, it's 1.1
    /// </summary>
    public string ProtocolVersion { get; set; } = "1.1";

    /// <summary>
    /// Creates an HTTP proxy client given the proxy <paramref name="settings"/>.
    /// </summary>
    public HttpProxyClient(ProxySettings settings) : base(settings)
    {

    }

    /// <inheritdoc/>
    protected override async Task CreateConnectionAsync(TcpClient client, string destinationHost, int destinationPort,
        CancellationToken cancellationToken = default)
    {
        await CreateConnectionStreamAsync(client, client.GetStream(), destinationHost, destinationPort, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task<Stream> CreateConnectionStreamAsync(TcpClient client, Stream stream, string destinationHost,
        int destinationPort, CancellationToken cancellationToken = default)
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

        HttpStatusCode statusCode;
        byte[] trailingBytes;

        try
        {
            await RequestConnectionAsync(stream, destinationHost, destinationPort, cancellationToken).ConfigureAwait(false);
            (statusCode, trailingBytes) = await ReceiveResponseAsync(stream, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            client.Close();

            if (ex is IOException or SocketException)
            {
                throw new BadProxyException("Error while working with proxy", ex);
            }

            throw;
        }

        if (statusCode != HttpStatusCode.OK)
        {
            client.Close();
            throw new BadProxyException("The proxy didn't reply with 200 OK");
        }

        // A proxy can send the CONNECT response and the first tunneled bytes in the same TCP/TLS record.
        // Preserve those bytes so callers read the exact destination stream instead of losing early data.
        return trailingBytes.Length == 0
            ? stream
            : new PrefixedStream(stream, trailingBytes);
    }

    /// <summary>
    /// Requests a CONNECT tunnel from the proxy.
    /// </summary>
    protected async Task RequestConnectionAsync(Stream nStream, string destinationHost, int destinationPort,
        CancellationToken cancellationToken = default)
    {
        var commandBuilder = new StringBuilder()
            .AppendFormat("CONNECT {0}:{1} HTTP/{2}\r\n", destinationHost, destinationPort, ProtocolVersion)
            .AppendFormat("Host: {0}:{1}\r\n", destinationHost, destinationPort)
            .Append(GenerateAuthorizationHeader())
            .Append("Proxy-Connection: Keep-Alive\r\n")
            .Append("\r\n");

        var buffer = Encoding.ASCII.GetBytes(commandBuilder.ToString());
        await nStream.WriteAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
        await nStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private string GenerateAuthorizationHeader()
    {
        if (Settings.Credentials == null || string.IsNullOrEmpty(Settings.Credentials.UserName))
        {
            return string.Empty;
        }

        var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{Settings.Credentials.UserName}:{Settings.Credentials.Password}"));

        return $"Proxy-Authorization: Basic {data}\r\n";
    }

    /// <summary>
    /// Gets the value that should be sent in the <c>Proxy-Authorization</c> header
    /// for plain HTTP requests forwarded through this proxy.
    /// </summary>
    public bool TryGetProxyAuthorizationHeaderValue(out string? value)
    {
        value = null;

        if (Settings.Credentials == null || string.IsNullOrEmpty(Settings.Credentials.UserName))
        {
            return false;
        }

        value = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{Settings.Credentials.UserName}:{Settings.Credentials.Password}"));

        value = $"Basic {value}";
        return true;
    }

    /// <summary>
    /// Receives the proxy response to a CONNECT request.
    /// </summary>
    protected static async Task<(HttpStatusCode StatusCode, byte[] TrailingBytes)> ReceiveResponseAsync(
        Stream stream, CancellationToken cancellationToken = default)
    {
        var buffer = new byte[512];
        using var memory = new MemoryStream();
        var headerEnd = -1;

        while (headerEnd < 0)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);

            if (bytesRead == 0)
            {
                break;
            }

            memory.Write(buffer, 0, bytesRead);
            headerEnd = FindHeaderEnd(memory.GetBuffer().AsSpan(0, (int)memory.Length));

            if (memory.Length > 64 * 1024)
            {
                throw new BadProxyException("The proxy response headers are too large");
            }
        }

        if (headerEnd < 0)
        {
            throw new BadProxyException("Received incomplete HTTP response from proxy");
        }

        var responseBytes = memory.GetBuffer().AsSpan(0, headerEnd);
        var response = Encoding.ASCII.GetString(responseBytes);

        if (response.Length == 0)
        {
            throw new BadProxyException("Received empty response");
        }

        // Check if the response is a correct HTTP response
        var match = HttpResponseRegex().Match(response);

        if (!match.Success)
        {
            throw new BadProxyException("Received wrong HTTP response from proxy");
        }

        if (!Enum.TryParse(match.Groups[1].Value, out HttpStatusCode statusCode))
        {
            throw new BadProxyException("Invalid HTTP status code");
        }

        var trailingBytes = memory.GetBuffer().AsSpan(headerEnd, (int)memory.Length - headerEnd).ToArray();
        return (statusCode, trailingBytes);
    }

    private static int FindHeaderEnd(ReadOnlySpan<byte> buffer)
    {
        for (var i = 0; i <= buffer.Length - 4; i++)
        {
            if (buffer[i] == '\r'
                && buffer[i + 1] == '\n'
                && buffer[i + 2] == '\r'
                && buffer[i + 3] == '\n')
            {
                return i + 4;
            }
        }

        return -1;
    }

    [GeneratedRegex("HTTP/[0-9\\.]* ([0-9]{3})")]
    private static partial Regex HttpResponseRegex();
}
