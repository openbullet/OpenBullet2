using RuriLib.Exceptions;
using RuriLib.Functions.Conversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Requests.WebSocket;

internal sealed class WebSocketConnection(List<string> messageStore) : IDisposable, IAsyncDisposable
{
    private readonly ClientWebSocket client = new();
    private readonly List<string> messages = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
    private readonly CancellationTokenSource receiveLoopCancellation = new();
    private Task receiveLoopTask = Task.CompletedTask;
    private Exception? receiveException;
    private bool disposed;

    public async Task ConnectAsync(Uri url, int keepAliveMilliseconds, IWebProxy? proxy,
        Dictionary<string, string>? customHeaders, CancellationToken cancellationToken)
    {
        client.Options.KeepAliveInterval = TimeSpan.FromMilliseconds(keepAliveMilliseconds);
        client.Options.Proxy = proxy;

        if (customHeaders is not null)
        {
            foreach (var header in customHeaders)
            {
                client.Options.SetRequestHeader(header.Key, header.Value);
            }
        }

        await client.ConnectAsync(url, cancellationToken).ConfigureAwait(false);

        if (client.State != WebSocketState.Open)
        {
            throw new BlockExecutionException("Failed to connect to the websocket");
        }

        receiveLoopTask = ReceiveLoopAsync(receiveLoopCancellation.Token);
    }

    public async Task SendTextAsync(string message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        ThrowIfFaulted();
        ThrowIfNotOpen();
        await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task SendBinaryAsync(byte[] message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        ThrowIfFaulted();
        ThrowIfNotOpen();
        await client.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Binary, true,
            cancellationToken).ConfigureAwait(false);
    }

    public void ThrowIfFaulted()
    {
        if (receiveException is not null)
        {
            ExceptionDispatchInfo.Capture(receiveException).Throw();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        try
        {
            if (client.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await client.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "User requested",
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (WebSocketException)
        {
        }
        finally
        {
            receiveLoopCancellation.Cancel();

            try
            {
                await receiveLoopTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (WebSocketException)
            {
            }

            client.Dispose();
            receiveLoopCancellation.Dispose();
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        receiveLoopCancellation.Cancel();
        client.Abort();
        client.Dispose();
        receiveLoopCancellation.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private void ThrowIfNotOpen()
    {
        if (client.State != WebSocketState.Open)
        {
            throw new BlockExecutionException("The websocket connection is not open");
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var messageBuffer = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken)
                        .ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        if (client.State == WebSocketState.CloseReceived)
                        {
                            await client.CloseOutputAsync(
                                    WebSocketCloseStatus.NormalClosure,
                                    "Server requested closure",
                                    CancellationToken.None)
                                .ConfigureAwait(false);
                        }

                        return;
                    }

                    messageBuffer.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                lock (messages)
                {
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            messages.Add(Encoding.UTF8.GetString(messageBuffer.ToArray()));
                            break;

                        case WebSocketMessageType.Binary:
                            messages.Add(Base64Converter.ToBase64String(messageBuffer.ToArray()));
                            break;
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (ObjectDisposedException) when (disposed)
        {
        }
        catch (Exception ex)
        {
            receiveException = ex;
        }
    }
}
