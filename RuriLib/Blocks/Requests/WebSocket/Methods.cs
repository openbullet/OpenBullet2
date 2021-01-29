using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Blocks.Requests.WebSocket
{
    [BlockCategory("Web Sockets", "Blocks to send and receive messages through websockets", "#addfad")]
    public static class Methods
    {
        [Block("Connects to a Web Socket", name = "WebSocket Connect (Proxiless)")]
        public static void WsConnect(BotData data, string url)
        {
            data.Logger.LogHeader();

            var ws = new WebSocketSharp.WebSocket(url);

            // Set the proxy
            if (data.UseProxy)
            {
                if (data.Proxy.Type != Models.Proxies.ProxyType.Http)
                {
                    throw new NotSupportedException("Only http proxies are supported");
                }
                else
                {
                    ws.SetProxy($"http://{data.Proxy.Host}:{data.Proxy.Port}", data.Proxy.Username, data.Proxy.Password);
                }
            }

            data.Objects["webSocket"] = ws;

            var wsMessages = new List<string>();
            data.Objects["wsMessages"] = wsMessages;

            // Hook the listener
            ws.OnMessage += (sender, e) =>
            {
                lock (wsMessages)
                    wsMessages.Add(e.Data);
            };

            // Connect
            ws.Connect();

            data.Logger.Log($"The Web Socket client connected to {url}", LogColors.MossGreen);
        }

        [Block("Sends a message on the Web Socket", name = "WebSocket Send")]
        public static void WsSend(BotData data, string message)
        {
            data.Logger.LogHeader();

            var ws = GetSocket(data);
            ws.Send(message);

            data.Logger.Log($"Sent {message} to the server", LogColors.MossGreen);
        }

        [Block("Gets unread messages that the server sent since the last read", name = "WebSocket Read")]
        public static List<string> WsRead(BotData data)
        {
            data.Logger.LogHeader();

            var messages = GetMessages(data);
            var cloned = messages.Select(m => m).ToList();

            lock (messages)
                messages.Clear();

            data.Logger.Log($"Unread messages from server", LogColors.MossGreen);
            data.Logger.Log(cloned, LogColors.MossGreen);

            return cloned;
        }

        [Block("Disconnects the existing Web Socket", name = "WebSocket Disconnect")]
        public static void WsDisconnect(BotData data)
        {
            data.Logger.LogHeader();

            var ws = GetSocket(data);
            ws.Close();

            data.Logger.Log("Closed the WebSocket", LogColors.MossGreen);
        }

        private static WebSocketSharp.WebSocket GetSocket(BotData data)
        {
            if (!data.Objects.ContainsKey("webSocket"))
                throw new NullReferenceException("You must open a websocket connection first");

            return (WebSocketSharp.WebSocket)data.Objects["webSocket"];
        }

        private static List<string> GetMessages(BotData data)
        {
            if (!data.Objects.ContainsKey("wsMessages"))
                throw new NullReferenceException("You must open a websocket connection first");

            return (List<string>)data.Objects["wsMessages"];
        }
    }
}
