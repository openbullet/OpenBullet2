using RuriLib.Attributes;
using RuriLib.Exceptions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Requests.WebSocket;

/// <summary>
/// Blocks for sending and receiving messages through web sockets.
/// </summary>
[BlockCategory("Web Sockets", "Blocks to send and receive messages through websockets", "#addfad")]
public static class Methods
{
    /// <summary>
    /// Connects to a Web Socket.
    /// </summary>
    [Block("Connects to a Web Socket", name = "WebSocket Connect",
        extraInfo = "Works with the proxy schemes supported by .NET ClientWebSocket")]
    public static async Task WsConnect(BotData data, string url, int keepAliveMilliseconds = 5000, Dictionary<string, string>? customHeaders = null)
    {
        data.Logger.LogHeader();

        var proxy = data is { UseProxy: true, Proxy: not null }
            ? CreateProxy(data.Proxy)
            : null;

        var wsMessages = new List<string>();
        data.SetObject("wsMessages", wsMessages);

        var ws = new WebSocketConnection(wsMessages);

        try
        {
            await ws.ConnectAsync(
                new Uri(url),
                keepAliveMilliseconds,
                proxy,
                customHeaders,
                data.CancellationToken).ConfigureAwait(false);
        }
        catch
        {
            ws.Dispose();
            throw;
        }

        data.SetObject("webSocket", ws);

        data.Logger.Log($"The Web Socket client connected to {url}", LogColors.MossGreen);
    }

    /// <summary>
    /// Sends a message on the Web Socket.
    /// </summary>
    // Compatibility wrapper for existing direct C# callers; blocks use WsSendAsync.
    public static void WsSend(BotData data, string message)
        => WsSendAsync(data, message).GetAwaiter().GetResult();

    /// <summary>
    /// Sends a message on the Web Socket.
    /// </summary>
    [Block("Sends a message on the Web Socket", name = "WebSocket Send", id = nameof(WsSend))]
    public static async Task WsSendAsync(BotData data, string message)
    {
        data.Logger.LogHeader();

        var ws = GetSocket(data);
        await ws.SendTextAsync(message, data.CancellationToken).ConfigureAwait(false);

        data.Logger.Log($"Sent {message} to the server", LogColors.MossGreen);
    }

    /// <summary>
    /// Sends a raw binary message on the Web Socket.
    /// </summary>
    // Compatibility wrapper for existing direct C# callers; blocks use WsSendRawAsync.
    public static void WsSendRaw(BotData data, byte[] message)
        => WsSendRawAsync(data, message).GetAwaiter().GetResult();

    /// <summary>
    /// Sends a raw binary message on the Web Socket.
    /// </summary>
    [Block("Sends a raw binary message on the Web Socket", name = "WebSocket Send Raw", id = nameof(WsSendRaw))]
    public static async Task WsSendRawAsync(BotData data, byte[] message)
    {
        data.Logger.LogHeader();

        var ws = GetSocket(data);
        await ws.SendBinaryAsync(message, data.CancellationToken).ConfigureAwait(false);

        data.Logger.Log($"Sent {message.Length} bytes to the server", LogColors.MossGreen);
    }

    /// <summary>
    /// Gets unread messages that the server sent since the last read.
    /// </summary>
    [Block("Gets unread messages that the server sent since the last read", name = "WebSocket Read")]
    public static async Task<List<string>> WsRead(BotData data, int pollIntervalInMilliseconds = 10, int timeoutMilliseconds = 10000)
    {
        data.Logger.LogHeader();

        var ws = GetSocket(data);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        var messages = new List<string>();

        // Wait until a message actually arrives otherwise it will be empty when the block is executed
        while (messages.Count == 0)
        {
            ws.ThrowIfFaulted();
            messages = GetMessages(data);
            await Task.Delay(pollIntervalInMilliseconds, cts.Token).ConfigureAwait(false);

            if (cts.IsCancellationRequested)
            {
                break;
            }
        }

        lock (messages)
        {
            var cloned = messages.Select(m => m).ToList();
            messages.Clear();

            data.Logger.Log("Unread messages from server", LogColors.MossGreen);
            data.Logger.Log(cloned, LogColors.MossGreen);

            return cloned;
        }
    }

    /// <summary>
    /// Disconnects the existing Web Socket.
    /// </summary>
    // Compatibility wrapper for existing direct C# callers; blocks use WsDisconnectAsync.
    public static void WsDisconnect(BotData data)
        => WsDisconnectAsync(data).GetAwaiter().GetResult();

    /// <summary>
    /// Disconnects the existing Web Socket.
    /// </summary>
    [Block("Disconnects the existing Web Socket", name = "WebSocket Disconnect", id = nameof(WsDisconnect))]
    public static async Task WsDisconnectAsync(BotData data)
    {
        data.Logger.LogHeader();

        var ws = GetSocket(data);
        await ws.DisconnectAsync(data.CancellationToken).ConfigureAwait(false);
        data.SetObject("webSocket", null, disposeExisting: false);

        data.Logger.Log("Closed the WebSocket", LogColors.MossGreen);
    }

    private static WebSocketConnection GetSocket(BotData data)
        => data.TryGetObject<WebSocketConnection>("webSocket") ?? throw new BlockExecutionException("You must open a websocket connection first");

    private static List<string> GetMessages(BotData data)
        => data.TryGetObject<List<string>>("wsMessages") ?? throw new BlockExecutionException("You must open a websocket connection first");

    private static IWebProxy CreateProxy(Models.Proxies.Proxy proxyModel)
    {
        var proxy = new WebProxy(new UriBuilder(GetProxyScheme(proxyModel.Type), proxyModel.Host, proxyModel.Port).Uri);

        if (proxyModel.NeedsAuthentication)
        {
            proxy.Credentials = new NetworkCredential(proxyModel.Username, proxyModel.Password);
        }

        return proxy;
    }

    private static string GetProxyScheme(Models.Proxies.ProxyType proxyType)
        => proxyType switch
        {
            Models.Proxies.ProxyType.Http => "http",
            Models.Proxies.ProxyType.Socks4 => "socks4",
            Models.Proxies.ProxyType.Socks5 => "socks5",
            Models.Proxies.ProxyType.Socks4a => "socks4a",
            _ => throw new ArgumentOutOfRangeException(nameof(proxyType), proxyType, "Unsupported proxy type")
        };
}
