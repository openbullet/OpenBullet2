using RuriLib.LS;
using RuriLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// Available commands for the TCP client.
    /// </summary>
    public enum TCPCommand
    {
        /// <summary>Connects the client to a host.</summary>
        Connect,

        /// <summary>Disconnects the client from the connected host.</summary>
        Disconnect,

        /// <summary>Sends a message to the connected host.</summary>
        Send
    }

    /// <summary>
    /// A block that can connect to a host over TCP and supports SSL.
    /// </summary>
    public class BlockTCP : BlockBase
    {
        private TCPCommand tcpCommand = TCPCommand.Connect;
        /// <summary>The command for the TCP client.</summary>
        public TCPCommand TCPCommand { get { return tcpCommand; } set { tcpCommand = value; OnPropertyChanged(); } }

        private string host = "";
        /// <summary>The host to connect to.</summary>
        public string Host { get { return host; } set { host = value; OnPropertyChanged(); } }

        private string port = "";
        /// <summary>The port to connect to.</summary>
        public string Port { get { return port; } set { port = value; OnPropertyChanged(); } }

        private bool useSSL = true;
        /// <summary>Whether the client will communicate over the Secure Sockets Layer.</summary>
        public bool UseSSL { get { return useSSL; } set { useSSL = value; OnPropertyChanged(); } }

        private bool webSocket = false;
        /// <summary>Whether to treat the message as a WebSocket payload (adds the frame overhead bytes).</summary>
        public bool WebSocket { get { return webSocket; } set { webSocket = value; OnPropertyChanged(); } }

        private bool waitForHello = true;
        /// <summary>Whether to wait for the server hello message once connected.</summary>
        public bool WaitForHello { get { return waitForHello; } set { waitForHello = value; OnPropertyChanged(); } }

        private string message = "";
        /// <summary>The message sent to the host.</summary>
        public string Message { get { return message; } set { message = value; OnPropertyChanged(); } }

        private string variableName = "";
        /// <summary>The name of the output variable where the TCP response will be stored.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private bool isCapture = false;
        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get { return isCapture; } set { isCapture = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates a TCP block.
        /// </summary>
        public BlockTCP()
        {
            Label = "TCP";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            // Parse the function
            TCPCommand = (TCPCommand)LineParser.ParseEnum(ref input, "Command", typeof(TCPCommand));

            // Parse specific function parameters
            switch (TCPCommand)
            {
                case TCPCommand.Connect:
                    Host = LineParser.ParseLiteral(ref input, "Host");
                    Port = LineParser.ParseLiteral(ref input, "Port");

                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);

                    break;

                case TCPCommand.Send:
                    Message = LineParser.ParseLiteral(ref input, "Message");

                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                default:
                    break;
            }

            // Try to parse the arrow, otherwise just return the block as is with default var name and var / cap choice
            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == string.Empty)
                return this;

            // Parse the VAR / CAP
            try
            {
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }

            // Parse the variable/capture name
            try { VariableName = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("TCP")
                .Token(TCPCommand);

            switch (TCPCommand)
            {
                case TCPCommand.Connect:
                    writer
                        .Literal(Host)
                        .Literal(Port)
                        .Boolean(UseSSL, "UseSSL")
                        .Boolean(WaitForHello, "WaitForHello");
                    break;

                case TCPCommand.Send:
                    writer
                        .Literal(Message)
                        .Boolean(WebSocket, "WebSocket");
                    break;
            }

            if (!writer.CheckDefault(VariableName, "VariableName"))
                writer
                    .Arrow()
                    .Token(IsCapture ? "CAP" : "VAR")
                    .Literal(VariableName);

            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            // Get easy handles
            var tcp = data.GetCustomObject("TCPClient") as TcpClient;
            var net = data.GetCustomObject("NETStream") as NetworkStream;
            var ssl = data.GetCustomObject("SSLStream") as SslStream;
            byte[] buffer = new byte[2048];
            int bytes = -1;
            string response = "";

            switch (TCPCommand)
            {
                case TCPCommand.Connect:
                    // Replace the Host and Port
                    var h = ReplaceValues(host, data);
                    var p = int.Parse(ReplaceValues(port, data));

                    // Initialize the TCP client, connect to the host and get the SSL stream
                    tcp = new TcpClient();
                    tcp.Connect(h, p);

                    if (tcp.Connected)
                    {
                        net = tcp.GetStream();

                        if (UseSSL)
                        {
                             ssl = new SslStream(net);
                             ssl.AuthenticateAsClient(h);
                        }

                        if (WaitForHello)
                        {
                            // Read the stream to make sure we are connected
                            if (UseSSL) bytes = ssl.Read(buffer, 0, buffer.Length);
                            else bytes = net.Read(buffer, 0, buffer.Length);

                            // Save the response as ASCII in the SOURCE variable
                            response = Encoding.ASCII.GetString(buffer, 0, bytes);
                        }

                        // Save the TCP client and the streams
                        data.CustomObjects["TCPClient"] = tcp;
                        data.CustomObjects["NETStream"] = net;
                        data.CustomObjects["SSLStream"] = ssl;
                        data.CustomObjects["TCPSSL"] = UseSSL;

                        data.Log(new LogEntry($"Succesfully connected to host {h} on port {p}. The server says:", Colors.Green));
                        data.Log(new LogEntry(response, Colors.GreenYellow));
                    }

                    if (VariableName != string.Empty)
                    {
                        data.Variables.Set(new CVar(VariableName, response, IsCapture));
                        data.Log(new LogEntry($"Saved Response in variable {VariableName}", Colors.White));
                    }
                    break;

                case TCPCommand.Disconnect:
                    if (tcp == null)
                    {
                        throw new Exception("Make a connection first!");
                    }

                    tcp.Close();
                    tcp = null;
                    if (net != null) net.Close();
                    if (ssl != null) ssl.Close();
                    data.Log(new LogEntry($"Succesfully closed the stream", Colors.GreenYellow));
                    break;

                case TCPCommand.Send:
                    if (tcp == null)
                    {
                        throw new Exception("Make a connection first!");
                    }

                    var msg = ReplaceValues(Message, data);
                    byte[] b = { };
                    var payload = Encoding.ASCII.GetBytes(msg.Unescape());

                    // Manual implementation of the WebSocket frame
                    if (WebSocket)
                    {
                        #region WebSocket
                        List<byte> bl = new List<byte>();

                        // (FIN=1) (RSV1=0) (RSV2=0) (RSV3=0) (OPCODE=0001) = 128 + 1 = 129
                        bl.Add(129);

                        ulong pllen = (ulong)payload.Length;

                        // We add 128 because the mask bit (MSB) is always 1. In this case the payload len is 7 bits long
                        if (pllen <= 125)
                        {
                            bl.Add((byte)(pllen + 128));
                        }

                        // Payload len set to 126 -> Next 2 bytes are payload len
                        else if (pllen <= ushort.MaxValue)
                        {
                            bl.Add(126 + 128);
                            bl.Add((byte)(pllen >> 8)); // Shift by 1 byte
                            bl.Add((byte)(pllen % 255)); // Take LSB
                        }

                        // Payload len set to 127 -> Next 4 bytes are payload len
                        else if (pllen <= ulong.MaxValue)
                        {
                            bl.Add(127 + 128);
                            bl.Add((byte)(pllen >> 24)); // Shift by 3 bytes
                            bl.Add((byte)((pllen >> 16) % 255)); // Shift by 2 bytes and take LSB
                            bl.Add((byte)((pllen >> 8) % 255)); // Shift by 1 byte and take LSB
                            bl.Add((byte)(pllen % 255)); // Take LSB
                        }

                        // Set the mask used for this message
                        byte[] mask = new byte[4] { 61, 84, 35, 6 };
                        bl.AddRange(mask);

                        // Finally we add the payload XORed with the mask
                        for (int i = 0; i < payload.Length; i++)
                        {
                            bl.Add((byte)(payload[i] ^ mask[i % 4]));
                        }

                        b = bl.ToArray();
                        #endregion
                    }
                    else
                    {
                        b = payload;
                    }
                    data.Log(new LogEntry("> " + msg, Colors.White));

                    var TCPSSL = data.GetCustomObject("TCPSSL") as bool?;
                    if (TCPSSL.HasValue && TCPSSL.Value)
                    {
                        ssl.Write(b);
                        bytes = ssl.Read(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        net.Write(b, 0, b.Length);
                        bytes = net.Read(buffer, 0, buffer.Length);
                    }

                    // Save the response as ASCII in the SOURCE variable and log it
                    response = Encoding.ASCII.GetString(buffer, 0, bytes);
                    data.Log(new LogEntry("> " + response, Colors.GreenYellow));

                    if (VariableName != string.Empty)
                    {
                        data.Variables.Set(new CVar(VariableName, response, IsCapture));
                        data.Log(new LogEntry($"Saved Response in variable {VariableName}.", Colors.White));
                    }
                    break;
            }
        }
    }
}
