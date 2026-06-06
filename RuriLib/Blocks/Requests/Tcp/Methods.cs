using RuriLib.Attributes;
using RuriLib.Exceptions;
using RuriLib.Functions.Tcp;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Requests.Tcp;

/// <summary>
/// Blocks for sending and receiving data over TCP.
/// </summary>
[BlockCategory("TCP", "Blocks to send data over TCP", "#e0b0ff")]
public static class Methods
{
    internal static RemoteCertificateValidationCallback? ServerCertificateValidationCallback { get; set; }

    /// <summary>
    /// Establishes a TCP connection with the given server.
    /// </summary>
    [Block("Establishes a TCP connection with the given server")]
    public static async Task TcpConnect(BotData data, string host, int port, bool useSSL, int timeoutMilliseconds = 10000)
    {
        data.Logger.LogHeader();
        DisposeTcpObjects(data, clearReferences: true);

        var tcpClient = await TcpClientFactory.GetClientAsync(host, port,
                TimeSpan.FromMilliseconds(timeoutMilliseconds), data.UseProxy ? data.Proxy : null, data.CancellationToken)
            .ConfigureAwait(false);

        tcpClient.ReceiveTimeout = timeoutMilliseconds;
        tcpClient.SendTimeout = timeoutMilliseconds;

        var netStream = tcpClient.GetStream();

        data.SetObject("tcpClient", tcpClient);
        data.SetObject("netStream", netStream);

        if (useSSL)
        {
            var sslStream = new SslStream(netStream);
            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = host,
                RemoteCertificateValidationCallback = ServerCertificateValidationCallback
            }, data.CancellationToken).ConfigureAwait(false);

            data.SetObject("sslStream", sslStream);
        }

        data.Logger.Log($"The client connected to {host} on port {port}", LogColors.Mauve);
    }

    /// <summary>
    /// Sends a message on the previously opened socket and read the response right after.
    /// </summary>
    [Block("Sends a message on the previously opened socket and read the response right after")]
    public static async Task<string> TcpSendRead(BotData data, string message, bool unescape = true, bool terminateWithCRLF = true,
        int bytesToRead = 4096, int timeoutMilliseconds = 60000, [BlockParam("Use UTF8", "Enable to use UTF-8 encoding instead of ASCII")] bool useUTF8 = false)
    {
        data.Logger.LogHeader();

        var netStream = GetStream(data);
        var encoding = useUTF8 ? Encoding.UTF8 : Encoding.ASCII;

        // Unescape codes like \r\n
        if (unescape)
        {
            message = Regex.Unescape(message);
        }

        // Append \r\n at the end if not present
        if (terminateWithCRLF && !message.EndsWith("\r\n"))
        {
            message += "\r\n";
        }

        using var cts = new CancellationTokenSource(timeoutMilliseconds);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, data.CancellationToken);

        // Send the message
        var txBytes = encoding.GetBytes(message);
        await netStream.WriteAsync(txBytes.AsMemory(0, txBytes.Length), linkedCts.Token).ConfigureAwait(false);

        // Read the response
        var buffer = new byte[bytesToRead];
        var rxBytes = await netStream.ReadAsync(buffer.AsMemory(0, buffer.Length), linkedCts.Token).ConfigureAwait(false);
        var response = encoding.GetString(buffer, 0, rxBytes);

        data.Logger.Log($"Sent message\r\n{message}", LogColors.Mauve);
        data.Logger.Log($"The server says\r\n{response}", LogColors.Mauve);
        return response;
    }

    /// <summary>
    /// Sends a message on the previously opened socket and read the response right after.
    /// </summary>
    [Block("Sends a message on the previously opened socket and read the response right after")]
    public static async Task<byte[]> TcpSendReadBytes(BotData data, byte[] bytes,
        int bytesToRead = 4096, int timeoutMilliseconds = 60000)
    {
        data.Logger.LogHeader();

        var netStream = GetStream(data);

        using var cts = new CancellationTokenSource(timeoutMilliseconds);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, data.CancellationToken);

        // Send the message
        await netStream.WriteAsync(bytes.AsMemory(0, bytes.Length), linkedCts.Token).ConfigureAwait(false);

        // Read the response
        var buffer = new byte[bytesToRead];
        var rxBytes = await netStream.ReadAsync(buffer.AsMemory(0, buffer.Length), linkedCts.Token).ConfigureAwait(false);

        // Copy only the bytes that were actually received
        var bytesRead = new ArraySegment<byte>(buffer, 0, rxBytes).ToArray();

        data.Logger.Log($"Sent {bytes.Length} bytes", LogColors.Mauve);
        data.Logger.Log($"Read {bytesRead.Length} bytes", LogColors.Mauve);
        return bytesRead;
    }

    /// <summary>
    /// Sends a message on the previously opened socket.
    /// </summary>
    [Block("Sends a message on the previously opened socket")]
    public static async Task TcpSend(BotData data, string message, bool unescape = true, bool terminateWithCRLF = true,
        [BlockParam("Use UTF8", "Enable to use UTF-8 encoding instead of ASCII")] bool useUTF8 = false)
    {
        data.Logger.LogHeader();

        var netStream = GetStream(data);
        var encoding = useUTF8 ? Encoding.UTF8 : Encoding.ASCII;

        // Unescape codes like \r\n
        if (unescape)
        {
            message = Regex.Unescape(message);
        }

        // Append \r\n at the end if not present
        if (terminateWithCRLF && !message.EndsWith("\r\n"))
        {
            message += "\r\n";
        }

        var bytes = encoding.GetBytes(message);
        await netStream.WriteAsync(bytes.AsMemory(0, bytes.Length), data.CancellationToken).ConfigureAwait(false);

        data.Logger.Log($"Sent message\r\n{message}", LogColors.Mauve);
    }

    /// <summary>
    /// Sends a message on the previously opened socket.
    /// </summary>
    [Block("Sends a message on the previously opened socket")]
    public static async Task TcpSendBytes(BotData data, byte[] bytes)
    {
        data.Logger.LogHeader();

        var netStream = GetStream(data);
        await netStream.WriteAsync(bytes.AsMemory(0, bytes.Length), data.CancellationToken).ConfigureAwait(false);

        data.Logger.Log($"Sent {bytes.Length} bytes", LogColors.Mauve);
    }

    /// <summary>
    /// Reads a message from the previously opened socket.
    /// </summary>
    [Block("Reads a message from the previously opened socket")]
    public static async Task<string> TcpRead(BotData data, int bytesToRead = 4096,
        int timeoutMilliseconds = 60000, [BlockParam("Use UTF8", "Enable to use UTF-8 encoding instead of ASCII")] bool useUTF8 = false)
    {
        data.Logger.LogHeader();

        using var cts = new CancellationTokenSource(timeoutMilliseconds);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, data.CancellationToken);
        var netStream = GetStream(data);
        var encoding = useUTF8 ? Encoding.UTF8 : Encoding.ASCII;

        // Read the response
        var buffer = new byte[bytesToRead];
        var rxBytes = await netStream.ReadAsync(buffer.AsMemory(0, buffer.Length), linkedCts.Token).ConfigureAwait(false);
        var response = encoding.GetString(buffer, 0, rxBytes);

        data.Logger.Log($"The server says\r\n{response}", LogColors.Mauve);
        return response;
    }

    /// <summary>
    /// Reads a raw message from the previously opened socket.
    /// </summary>
    [Block("Reads a raw message from the previously opened socket")]
    public static async Task<byte[]> TcpReadBytes(BotData data, int bytesToRead = 4096,
        int timeoutMilliseconds = 60000)
    {
        data.Logger.LogHeader();

        using var cts = new CancellationTokenSource(timeoutMilliseconds);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, data.CancellationToken);
        var netStream = GetStream(data);

        // Read the response
        var buffer = new byte[bytesToRead];
        var rxBytes = await netStream.ReadAsync(buffer.AsMemory(0, buffer.Length), linkedCts.Token).ConfigureAwait(false);

        // Copy only the bytes that were actually received
        var bytesRead = new ArraySegment<byte>(buffer, 0, rxBytes).ToArray();

        data.Logger.Log($"Read {bytesRead.Length} bytes", LogColors.Mauve);
        return bytesRead;
    }

    /// <summary>
    /// Sends an HTTP message on the previously opened socket and reads the response.
    /// </summary>
    [Block("Sends an HTTP message on the previously opened socket and reads the response",
        extraInfo = "Use this block instead of SendMessage for HTTP or it will only read the headers")]
    public static async Task<string> TcpSendReadHttp(BotData data, string message, bool unescape = true,
        int timeoutMilliseconds = 60000)
    {
        data.Logger.LogHeader();

        using var cts = new CancellationTokenSource(timeoutMilliseconds);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, data.CancellationToken);
        var stream = GetStream(data);

        // Unescape codes like \r\n
        if (unescape)
        {
            message = Regex.Unescape(message);
        }

        // Append \r\n at the end if not present
        if (!message.EndsWith("\r\n"))
        {
            message += "\r\n";
        }

        // Send the message
        var txBytes = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(txBytes.AsMemory(0, txBytes.Length), linkedCts.Token).ConfigureAwait(false);

        // Receive data
        var rxBytes = await ReadHttpResponseAsync(stream, linkedCts.Token).ConfigureAwait(false);
        var index = BinaryMatch(rxBytes, "\r\n\r\n"u8.ToArray());
        if (index < 0)
        {
            throw new InvalidOperationException("The server sent an invalid HTTP response");
        }

        index += 4;
        var headers = Encoding.UTF8.GetString(rxBytes, 0, index);

        string payload;
        var bodyBytes = new ArraySegment<byte>(rxBytes, index, rxBytes.Length - index).ToArray();

        // If gzip, decompress
        if (headers.IndexOf("Content-Encoding: gzip", StringComparison.OrdinalIgnoreCase) > 0)
        {
            await using var ms = new MemoryStream(bodyBytes);
            await using var decompressionStream = new GZipStream(ms, CompressionMode.Decompress);
            using var decompressedMemory = new MemoryStream();
            await decompressionStream.CopyToAsync(decompressedMemory, linkedCts.Token).ConfigureAwait(false);
            decompressedMemory.Position = 0;
            payload = Encoding.UTF8.GetString(decompressedMemory.ToArray());
        }
        else
        {
            payload = Encoding.UTF8.GetString(bodyBytes);
        }

        var response = $"{headers}{payload}";

        data.Logger.Log($"Sent message\r\n{message}", LogColors.Mauve);
        data.Logger.Log($"The server says\r\n{response}", LogColors.Mauve);

        return response;
    }

    /// <summary>
    /// Closes the previously opened socket.
    /// </summary>
    [Block("Closes the previously opened socket")]
    public static void TcpDisconnect(BotData data)
    {
        data.Logger.LogHeader();

        DisposeTcpObjects(data, clearReferences: false);

        data.Logger.Log("Disconnected", LogColors.Mauve);
    }

    private static Stream GetStream(BotData data)
    {
        var sslStream = data.TryGetObject<SslStream>("sslStream");

        if (sslStream is not null)
        {
            return sslStream;
        }

        return data.TryGetObject<NetworkStream>("netStream")
               ?? throw new BlockExecutionException("You have to create a connection first!");
    }

    private static void DisposeTcpObjects(BotData data, bool clearReferences)
    {
        TryDispose(data.TryGetObject<SslStream>("sslStream"));
        TryDispose(data.TryGetObject<NetworkStream>("netStream"));
        TryDispose(data.TryGetObject<TcpClient>("tcpClient"));

        if (clearReferences)
        {
            data.SetObject("sslStream", null, disposeExisting: false);
            data.SetObject("netStream", null, disposeExisting: false);
            data.SetObject("tcpClient", null, disposeExisting: false);
        }
    }

    private static void TryDispose(IDisposable? disposable)
    {
        try
        {
            disposable?.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    private static async Task<byte[]> ReadHttpResponseAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        using var responseStream = new MemoryStream();

        var headersEndIndex = -1;
        while (headersEndIndex < 0)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
                .ConfigureAwait(false);

            if (bytesRead == 0)
            {
                throw new InvalidOperationException("The server closed the TCP stream before sending HTTP headers");
            }

            await responseStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            headersEndIndex = BinaryMatch(responseStream.ToArray(), "\r\n\r\n"u8.ToArray());
        }

        headersEndIndex += 4;
        var partialResponse = responseStream.ToArray();
        var headers = Encoding.UTF8.GetString(partialResponse, 0, headersEndIndex);
        var bodyPrefix = new ArraySegment<byte>(partialResponse, headersEndIndex, partialResponse.Length - headersEndIndex)
            .ToArray();

        if (ResponseHasNoBody(headers))
        {
            return partialResponse[..headersEndIndex];
        }

        if (HasChunkedTransferEncoding(headers))
        {
            var chunkedBody = await ReadChunkedBodyAsync(stream, bodyPrefix, cancellationToken).ConfigureAwait(false);
            return ConcatBytes(partialResponse[..headersEndIndex], chunkedBody);
        }

        if (TryGetContentLength(headers, out var contentLength))
        {
            var body = await ReadFixedLengthBodyAsync(stream, bodyPrefix, contentLength, cancellationToken)
                .ConfigureAwait(false);
            return ConcatBytes(partialResponse[..headersEndIndex], body);
        }

        await stream.CopyToAsync(responseStream, cancellationToken).ConfigureAwait(false);
        return responseStream.ToArray();
    }

    private static async Task<byte[]> ReadFixedLengthBodyAsync(Stream stream, byte[] bodyPrefix, int contentLength,
        CancellationToken cancellationToken)
    {
        if (contentLength < 0)
        {
            throw new InvalidOperationException("The server sent an invalid Content-Length header");
        }

        using var bodyStream = new MemoryStream(contentLength);

        var prefetchedBytes = Math.Min(bodyPrefix.Length, contentLength);
        if (prefetchedBytes > 0)
        {
            await bodyStream.WriteAsync(bodyPrefix.AsMemory(0, prefetchedBytes), cancellationToken).ConfigureAwait(false);
        }

        var remainingBytes = contentLength - prefetchedBytes;
        if (remainingBytes == 0)
        {
            return bodyStream.ToArray();
        }

        var buffer = new byte[Math.Min(4096, remainingBytes)];
        while (remainingBytes > 0)
        {
            var bytesToRead = Math.Min(buffer.Length, remainingBytes);
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bytesToRead), cancellationToken)
                .ConfigureAwait(false);

            if (bytesRead == 0)
            {
                throw new InvalidOperationException("The server closed the TCP stream before sending the full HTTP body");
            }

            await bodyStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            remainingBytes -= bytesRead;
        }

        return bodyStream.ToArray();
    }

    private static async Task<byte[]> ReadChunkedBodyAsync(Stream stream, byte[] bodyPrefix,
        CancellationToken cancellationToken)
    {
        var reader = new PrefetchedBodyReader(stream, bodyPrefix);
        using var bodyStream = new MemoryStream();

        while (true)
        {
            var chunkLengthLine = await ReadAsciiLineAsync(reader, cancellationToken)
                .ConfigureAwait(false);
            var separatorIndex = chunkLengthLine.IndexOf(';');
            var chunkLengthValue = separatorIndex >= 0
                ? chunkLengthLine[..separatorIndex]
                : chunkLengthLine;

            if (!int.TryParse(chunkLengthValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var chunkLength))
            {
                throw new InvalidOperationException("The server sent an invalid chunked HTTP response");
            }

            if (chunkLength == 0)
            {
                while (!string.IsNullOrEmpty(await ReadAsciiLineAsync(reader, cancellationToken)
                           .ConfigureAwait(false)))
                {
                }

                return bodyStream.ToArray();
            }

            await CopyExactBytesAsync(reader, bodyStream, chunkLength, cancellationToken).ConfigureAwait(false);
            await ConsumeExpectedCrlfAsync(reader, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<string> ReadAsciiLineAsync(PrefetchedBodyReader reader, CancellationToken cancellationToken)
    {
        using var lineStream = new MemoryStream();
        var singleByte = new byte[1];

        while (true)
        {
            var bytesRead = await reader.ReadAsync(singleByte.AsMemory(0, 1), cancellationToken).ConfigureAwait(false);

            if (bytesRead == 0)
            {
                throw new InvalidOperationException("The server closed the TCP stream unexpectedly");
            }

            lineStream.WriteByte(singleByte[0]);

            var lineBytes = lineStream.GetBuffer();
            var lineLength = (int)lineStream.Length;
            if (lineLength >= 2 && lineBytes[lineLength - 2] == '\r' && lineBytes[lineLength - 1] == '\n')
            {
                return Encoding.ASCII.GetString(lineBytes, 0, lineLength - 2);
            }
        }
    }

    private static async Task ConsumeExpectedCrlfAsync(PrefetchedBodyReader reader, CancellationToken cancellationToken)
    {
        var crlf = new byte[2];
        await ReadExactBytesAsync(reader, crlf, cancellationToken).ConfigureAwait(false);

        if (crlf[0] != '\r' || crlf[1] != '\n')
        {
            throw new InvalidOperationException("The server sent an invalid chunked HTTP response");
        }
    }

    private static async Task CopyExactBytesAsync(PrefetchedBodyReader reader, MemoryStream destination, int bytesToCopy,
        CancellationToken cancellationToken)
    {
        if (bytesToCopy == 0)
        {
            return;
        }

        var buffer = new byte[Math.Min(4096, bytesToCopy)];
        var remainingBytes = bytesToCopy;

        while (remainingBytes > 0)
        {
            var bytesRead = await reader.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, remainingBytes)),
                cancellationToken).ConfigureAwait(false);

            if (bytesRead == 0)
            {
                throw new InvalidOperationException("The server closed the TCP stream before sending the full HTTP body");
            }

            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            remainingBytes -= bytesRead;
        }
    }

    private static async Task ReadExactBytesAsync(PrefetchedBodyReader reader, byte[] destination,
        CancellationToken cancellationToken)
    {
        var totalBytesRead = 0;

        while (totalBytesRead < destination.Length)
        {
            var bytesRead = await reader.ReadAsync(destination.AsMemory(totalBytesRead, destination.Length - totalBytesRead),
                cancellationToken).ConfigureAwait(false);

            if (bytesRead == 0)
            {
                throw new InvalidOperationException("The server closed the TCP stream before sending the full HTTP body");
            }

            totalBytesRead += bytesRead;
        }
    }

    private static bool ResponseHasNoBody(string headers)
    {
        var firstLineEnd = headers.IndexOf("\r\n", StringComparison.Ordinal);
        var firstLine = firstLineEnd >= 0 ? headers[..firstLineEnd] : headers;
        var parts = firstLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2 || !int.TryParse(parts[1], out var statusCode))
        {
            return false;
        }

        return statusCode is >= 100 and < 200 or 204 or 304;
    }

    private static bool HasChunkedTransferEncoding(string headers)
        => TryGetHeaderValue(headers, "Transfer-Encoding", out var transferEncoding)
           && transferEncoding.Contains("chunked", StringComparison.OrdinalIgnoreCase);

    private static bool TryGetContentLength(string headers, out int contentLength)
    {
        contentLength = 0;

        return TryGetHeaderValue(headers, "Content-Length", out var value)
               && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out contentLength);
    }

    private static bool TryGetHeaderValue(string headers, string headerName, out string value)
    {
        foreach (var line in headers.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            if (line[..separatorIndex].Equals(headerName, StringComparison.OrdinalIgnoreCase))
            {
                value = line[(separatorIndex + 1)..].Trim();
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static byte[] ConcatBytes(byte[] first, byte[] second)
    {
        var result = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        return result;
    }

    private static int BinaryMatch(byte[] input, byte[] pattern)
    {
        var sLen = input.Length - pattern.Length + 1;
        for (var i = 0; i < sLen; ++i)
        {
            var match = true;
            for (var j = 0; j < pattern.Length; ++j)
            {
                if (input[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                return i;
            }
        }
        return -1;
    }

    private sealed class PrefetchedBodyReader(Stream stream, byte[] bodyPrefix)
    {
        private readonly Stream stream = stream;
        private readonly byte[] bodyPrefix = bodyPrefix;
        private int prefixOffset;

        public async Task<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken)
        {
            if (prefixOffset < bodyPrefix.Length)
            {
                var bytesFromPrefix = Math.Min(destination.Length, bodyPrefix.Length - prefixOffset);
                bodyPrefix.AsMemory(prefixOffset, bytesFromPrefix).CopyTo(destination);
                prefixOffset += bytesFromPrefix;
                return bytesFromPrefix;
            }

            return await stream.ReadAsync(destination, cancellationToken).ConfigureAwait(false);
        }
    }
}
