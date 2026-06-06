using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Tests.Utils;

internal sealed class LocalHttpResponseServer : IAsyncDisposable
{
    private const string ClientClosedBeforeHeadersMessage = "The client closed the TCP stream before sending HTTP headers";
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly Func<TcpClient, CancellationToken, Task> handler;
    private readonly Task acceptTask;

    private LocalHttpResponseServer(Func<TcpClient, CancellationToken, Task> handler)
    {
        this.handler = handler;
        listener.Start();
        acceptTask = Task.Run(AcceptClientAsync, CancellationToken.None);
    }

    public Uri Uri => new($"http://127.0.0.1:{Port}/");

    private int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

    public static LocalHttpResponseServer CreateDelayed(TimeSpan delay, byte[] payload, params string[] headers)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(headers);

        return new(async (client, cancellationToken) =>
        {
            await using var stream = client.GetStream();
            await ReadHeadersAsync(stream, cancellationToken);
            await Task.Delay(delay, cancellationToken);

            var responseHeaders = new StringBuilder()
                .Append("HTTP/1.1 200 OK\r\n")
                .Append($"Content-Length: {payload.Length}\r\n")
                .Append("Connection: close\r\n");

            foreach (var header in headers)
            {
                responseHeaders.Append(header).Append("\r\n");
            }

            responseHeaders.Append("\r\n");

            await stream.WriteAsync(Encoding.ASCII.GetBytes(responseHeaders.ToString()), cancellationToken);
            await stream.WriteAsync(payload, cancellationToken);
        });
    }

    public async ValueTask DisposeAsync()
    {
        cancellationTokenSource.Cancel();
        listener.Stop();

        try
        {
            await acceptTask;
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SocketException)
        {
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }
    }

    private async Task AcceptClientAsync()
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            using var client = await listener.AcceptTcpClientAsync(cancellationTokenSource.Token);

            try
            {
                await handler(client, cancellationTokenSource.Token);
                return;
            }
            catch (InvalidOperationException ex) when (
                ex.Message == ClientClosedBeforeHeadersMessage
                && !cancellationTokenSource.IsCancellationRequested)
            {
                // Under heavy parallel test load a client can connect and close before sending
                // request headers. Ignore that transient connection and keep waiting for the real one.
            }
        }
    }

    private static async Task ReadHeadersAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var builder = new StringBuilder();

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
            {
                throw new InvalidOperationException(ClientClosedBeforeHeadersMessage);
            }

            builder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));

            if (builder.ToString().Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                return;
            }
        }
    }
}
