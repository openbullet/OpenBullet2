using RuriLib.Attributes;
using RuriLib.Functions.Tcp;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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

            var netStream = await Task.Run(() => TcpFactory.GetNetworkStream(host, port,
                TimeSpan.FromMilliseconds(timeoutMilliseconds), data.Proxy), data.CancellationToken);

            data.Objects["netStream"] = netStream;

            if (useSSL)
            {
                var sslStream = new SslStream(netStream);
                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions 
                {
                    TargetHost = host
                }, data.CancellationToken);

                data.Objects["sslStream"] = sslStream;
            }

            data.Logger.Log($"The client connected to {host} on port {port}", LogColors.Mauve);
        }

        [Block("Sends a message (ASCII) on the previously opened socket and read the response (ASCII) right after")]
        public static async Task<string> TcpSendRead(BotData data, string message, bool unescape = true, bool terminateWithCRLF = true,
            int bytesToRead = 4096)
        {
            data.Logger.LogHeader();

            var netStream = GetStream(data);

            // Unescape codes like \r\n
            if (unescape)
                message = Regex.Unescape(message);

            // Append \r\n at the end if not present
            if (terminateWithCRLF && !message.EndsWith("\r\n"))
                message += "\r\n";
            
            // Send the message
            byte[] txBytes = Encoding.ASCII.GetBytes(message);
            await netStream.WriteAsync(txBytes, 0, txBytes.Length, data.CancellationToken);

            // Read the response
            byte[] buffer = new byte[bytesToRead];
            var rxBytes = await netStream.ReadAsync(buffer, 0, buffer.Length);
            var response = Encoding.ASCII.GetString(buffer, 0, rxBytes);

            data.Logger.Log($"Sent message\r\n{message}", LogColors.Mauve);
            data.Logger.Log($"The server says\r\n{response}", LogColors.Mauve);
            return response;
        }

        [Block("Sends a message(ASCII) on the previously opened socket")]
        public static async Task TcpSend(BotData data, string message, bool unescape = true, bool terminateWithCRLF = true)
        {
            data.Logger.LogHeader();

            var netStream = GetStream(data);

            // Unescape codes like \r\n
            if (unescape)
                message = Regex.Unescape(message);

            // Append \r\n at the end if not present
            if (terminateWithCRLF && !message.EndsWith("\r\n"))
                message += "\r\n";

            byte[] bytes = Encoding.ASCII.GetBytes(message);
            await netStream.WriteAsync(bytes, 0, bytes.Length, data.CancellationToken);

            data.Logger.Log($"Sent message\r\n{message}", LogColors.Mauve);
        }

        [Block("Reads a message (ASCII) from the previously opened socket")]
        public static async Task<string> TcpRead(BotData data, int bytesToRead = 4096)
        {
            data.Logger.LogHeader();

            var netStream = GetStream(data);

            // Read the response
            byte[] buffer = new byte[bytesToRead];
            var rxBytes = await netStream.ReadAsync(buffer, 0, buffer.Length);
            var response = Encoding.ASCII.GetString(buffer, 0, rxBytes);

            data.Logger.Log($"The server says\r\n{response}", LogColors.Mauve);
            return response;
        }


        [Block("Sends an HTTP message on the previously opened socket and reads the response",
            extraInfo = "Use this block instead of SendMessage or it will only read the headers")]
        public static async Task<string> TcpSendReadHttp(BotData data, string message, bool unescape = true)
        {
            data.Logger.LogHeader();

            var netStream = GetStream(data);

            // Unescape codes like \r\n
            if (unescape)
                message = Regex.Unescape(message);

            // Append \r\n at the end if not present
            if (!message.EndsWith("\r\n"))
                message += "\r\n";

            // Send the message
            byte[] txBytes = Encoding.ASCII.GetBytes(message);
            await netStream.WriteAsync(txBytes, 0, txBytes.Length, data.CancellationToken);

            // Read the headers (4096 bytes should be enough for anything)
            byte[] buffer = new byte[4096];
            var rxBytes = await netStream.ReadAsync(buffer, 0, buffer.Length);
            var headers = Encoding.ASCII.GetString(buffer, 0, rxBytes);

            // Try to parse the Content-Length of the payload
            var contentLength = Regex.Match(headers, $"Content-Length: ([0-9]+)");
            var bufferSize = contentLength.Success ? int.Parse(contentLength.Groups[1].Value) : 4096;

            buffer = new byte[bufferSize];
            await Task.Delay(100);

            // Read the payload
            rxBytes = await netStream.ReadAsync(buffer, 0, buffer.Length);
            var payload = Encoding.ASCII.GetString(buffer, 0, rxBytes);

            var response = $"{headers}\r\n{payload}";

            data.Logger.Log($"Sent message\r\n{message}", LogColors.Mauve);
            data.Logger.Log($"The server says\r\n{response}", LogColors.Mauve);
            
            return response;
        }

        [Block("Closes the previously opened socket")]
        public static void TcpDisconnect(BotData data)
        {
            data.Logger.LogHeader();

            if (data.Objects.ContainsKey("sslStream"))
                ((SslStream)data.Objects["sslStream"]).Close();

            if (data.Objects.ContainsKey("netStream"))
                ((NetworkStream)data.Objects["netStream"]).Close();

            data.Logger.Log("Disconnected", LogColors.Mauve);
        }

        private static Stream GetStream(BotData data)
        {
            if (data.Objects.ContainsKey("sslStream"))
                return (SslStream)data.Objects["sslStream"];

            if (data.Objects.ContainsKey("netStream"))
                return (NetworkStream)data.Objects["netStream"];

            throw new NullReferenceException("You have to create a connection first!");
        }
    }
}
