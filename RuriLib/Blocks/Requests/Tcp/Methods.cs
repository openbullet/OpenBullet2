using RuriLib.Attributes;
using RuriLib.Functions.Tcp;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Requests.Tcp
{
    [BlockCategory("TCP", "Blocks to send data over TCP", "#e0b0ff")]
    public static class Methods
    {
        [Block("Establishes a TCP connection with the given server")]
        public static async Task TcpConnect(BotData data, string host, int port, bool useSSL, int timeoutMilliseconds = 10000)
        {
            data.Logger.LogHeader();

            var tcpClient = await TcpClientFactory.GetClientAsync(host, port,
                TimeSpan.FromMilliseconds(timeoutMilliseconds), data.UseProxy ? data.Proxy : null, data.CancellationToken)
                .ConfigureAwait(false);

            var netStream = tcpClient.GetStream();

            data.SetObject("netStream", netStream);

            if (useSSL)
            {
                var sslStream = new SslStream(netStream);
                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions 
                {
                    TargetHost = host,
                }, data.CancellationToken).ConfigureAwait(false);

                data.SetObject("sslStream", sslStream);
            }

            data.Logger.Log($"The client connected to {host} on port {port}", LogColors.Mauve);
        }

        [Block("Sends a message (ASCII) on the previously opened socket and read the response (ASCII) right after")]
        public static async Task<string> TcpSendRead(BotData data, string message, bool unescape = true, bool terminateWithCRLF = true,
            int bytesToRead = 4096, int timeoutMilliseconds = 60000, [BlockParam("Use UTF8", "Enable to use UTF-8 encoding")] bool useUTF8 = false)
        {
            data.Logger.LogHeader();

            var netStream = GetStream(data);
            var encoding = useUTF8 ? Encoding.UTF8 : Encoding.ASCII;

            // Unescape codes like \r\n
            if (unescape)
                message = Regex.Unescape(message);

            // Append \r\n at the end if not present
            if (terminateWithCRLF && !message.EndsWith("\r\n"))
                message += "\r\n";

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

        [Block("Sends a message(ASCII) on the previously opened socket")]
        public static async Task TcpSend(BotData data, string message, bool unescape = true, bool terminateWithCRLF = true,
            [BlockParam("Use UTF8", "Enable to use UTF-8 encoding")] bool useUTF8 = false)
        {
            data.Logger.LogHeader();

            var netStream = GetStream(data);
            var encoding = useUTF8 ? Encoding.UTF8 : Encoding.ASCII;

            // Unescape codes like \r\n
            if (unescape)
                message = Regex.Unescape(message);

            // Append \r\n at the end if not present
            if (terminateWithCRLF && !message.EndsWith("\r\n"))
                message += "\r\n";

            var bytes = encoding.GetBytes(message);
            await netStream.WriteAsync(bytes.AsMemory(0, bytes.Length), data.CancellationToken).ConfigureAwait(false);

            data.Logger.Log($"Sent message\r\n{message}", LogColors.Mauve);
        }

        [Block("Sends a message on the previously opened socket")]
        public static async Task TcpSendBytes(BotData data, byte[] bytes)
        {
            data.Logger.LogHeader();

            var netStream = GetStream(data);
            await netStream.WriteAsync(bytes.AsMemory(0, bytes.Length), data.CancellationToken).ConfigureAwait(false);

            data.Logger.Log($"Sent {bytes.Length} bytes", LogColors.Mauve);
        }

        [Block("Reads a message (ASCII) from the previously opened socket")]
        public static async Task<string> TcpRead(BotData data, int bytesToRead = 4096, 
            int timeoutMilliseconds = 60000, [BlockParam("Use UTF8", "Enable to use UTF-8 encoding")] bool useUTF8 = false)
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


        [Block("Sends an HTTP message on the previously opened socket and reads the response",
            extraInfo = "Use this block instead of SendMessage or it will only read the headers")]
        public static async Task<string> TcpSendReadHttp(BotData data, string message, bool unescape = true,
            int timeoutMilliseconds = 60000)
        {
            data.Logger.LogHeader();

            using var cts = new CancellationTokenSource(timeoutMilliseconds);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, data.CancellationToken);
            var netStream = GetStream(data);

            // Unescape codes like \r\n
            if (unescape)
                message = Regex.Unescape(message);

            // Append \r\n at the end if not present
            if (!message.EndsWith("\r\n"))
                message += "\r\n";

            // Send the message
            var txBytes = Encoding.UTF8.GetBytes(message);
            await netStream.WriteAsync(txBytes.AsMemory(0, txBytes.Length), linkedCts.Token).ConfigureAwait(false);

            // Receive data
            var payload = string.Empty;
            using var ms = new MemoryStream();
            await netStream.CopyToAsync(ms, linkedCts.Token).ConfigureAwait(false); // Read the whole response stream
            ms.Position = 0;
            var rxBytes = ms.ToArray();

            // Find where the headers are finished
            var index = BinaryMatch(rxBytes, Encoding.UTF8.GetBytes("\r\n\r\n")) + 4;
            var headers = Encoding.UTF8.GetString(rxBytes, 0, index);
            ms.Position = index;

            // If gzip, decompress
            if (headers.IndexOf("Content-Encoding: gzip", StringComparison.OrdinalIgnoreCase) > 0)
            {
                await using var decompressionStream = new GZipStream(ms, CompressionMode.Decompress);
                using var decompressedMemory = new MemoryStream();
                await decompressionStream.CopyToAsync(decompressedMemory, linkedCts.Token).ConfigureAwait(false);
                decompressedMemory.Position = 0;
                payload = Encoding.UTF8.GetString(decompressedMemory.ToArray());
            }
            else
            {
                payload = Encoding.UTF8.GetString(rxBytes, index, rxBytes.Length - index);
            }

            var response = $"{headers}{payload}";

            data.Logger.Log($"Sent message\r\n{message}", LogColors.Mauve);
            data.Logger.Log($"The server says\r\n{response}", LogColors.Mauve);
            
            return response;
        }

        [Block("Closes the previously opened socket")]
        public static void TcpDisconnect(BotData data)
        {
            data.Logger.LogHeader();

            data.TryGetObject<SslStream>("sslStream")?.Dispose();
            data.TryGetObject<NetworkStream>("netStream")?.Dispose();

            data.Logger.Log("Disconnected", LogColors.Mauve);
        }

        private static Stream GetStream(BotData data)
        {
            var sslStream = data.TryGetObject<SslStream>("sslStream");

            if (sslStream is not null)
            {
                return sslStream;
            }

            return data.TryGetObject<NetworkStream>("netStream") ?? throw new NullReferenceException("You have to create a connection first!");
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
    }
}
