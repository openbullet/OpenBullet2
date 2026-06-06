using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Http.Tests;

internal sealed class LocalHttpResponseServer : IAsyncDisposable
{
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly Func<TcpClient, CancellationToken, Task> handler;
    private readonly Task acceptTask;

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    private LocalHttpResponseServer(Func<TcpClient, CancellationToken, Task> handler)
    {
        this.handler = handler ?? throw new ArgumentNullException(nameof(handler));

        listener.Start();
        acceptTask = Task.Run(AcceptClientAsync, CancellationToken.None);
    }

    public Uri Uri => new($"http://127.0.0.1:{Port}/");

    private int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

    public static LocalHttpResponseServer Create(byte[] payload, params string[] headers)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(headers);

        return new(async (client, cancellationToken) =>
        {
            await using var networkStream = client.GetStream();
            await ReadHeadersAsync(networkStream, cancellationToken);

            var responseHeaders = new StringBuilder()
                .Append("HTTP/1.1 200 OK\r\n")
                .Append($"Content-Length: {payload.Length}\r\n")
                .Append("Connection: close\r\n");

            foreach (var header in headers)
            {
                responseHeaders.Append(header).Append("\r\n");
            }

            responseHeaders.Append("\r\n");

            await networkStream.WriteAsync(Encoding.ASCII.GetBytes(responseHeaders.ToString()), cancellationToken);
            await networkStream.WriteAsync(payload, cancellationToken);
        });
    }

    public static byte[] Gzip(byte[] payload)
    {
        using var ms = new MemoryStream();

        using (var gzip = new GZipStream(ms, CompressionMode.Compress, leaveOpen: true))
        {
            gzip.Write(payload, 0, payload.Length);
        }

        return ms.ToArray();
    }

    public static byte[] Brotli(byte[] payload)
    {
        using var ms = new MemoryStream();

        using (var brotli = new BrotliStream(ms, CompressionMode.Compress, leaveOpen: true))
        {
            brotli.Write(payload, 0, payload.Length);
        }

        return ms.ToArray();
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
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationTokenSource.Token,
            TestCancellationToken);

        using var client = await listener.AcceptTcpClientAsync(linkedCts.Token);
        await handler(client, linkedCts.Token);
    }

    private static async Task ReadHeadersAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        using var ms = new MemoryStream();

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
            {
                throw new InvalidOperationException("The client closed the TCP stream before sending HTTP headers");
            }

            await ms.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

            if (Encoding.ASCII.GetString(ms.ToArray()).Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                return;
            }
        }
    }
}
