using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Http.Tests.Utils;

internal sealed class TlsClientHelloCaptureServer : IAsyncDisposable
{
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource cts = new();
    private readonly CancellationToken testCancellationToken;
    private readonly Task acceptTask;
    private readonly TaskCompletionSource<byte[]> clientHello =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public TlsClientHelloCaptureServer(CancellationToken testCancellationToken)
    {
        this.testCancellationToken = testCancellationToken;
        listener.Start();
        acceptTask = Task.Run(AcceptAsync);
    }

    public Uri Uri => new($"https://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/");

    public Task<byte[]> ClientHello => clientHello.Task;

    public async ValueTask DisposeAsync()
    {
        await cts.CancelAsync();
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
            cts.Dispose();
        }
    }

    private async Task AcceptAsync()
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, testCancellationToken);
        using var client = await listener.AcceptTcpClientAsync(linkedCts.Token);
        await using var stream = client.GetStream();

        try
        {
            clientHello.TrySetResult(await ReadClientHelloAsync(stream, linkedCts.Token));
        }
        catch (Exception ex)
        {
            clientHello.TrySetException(ex);
        }
    }

    private static async Task<byte[]> ReadClientHelloAsync(Stream stream, CancellationToken cancellationToken)
    {
        var header = await ReadExactAsync(stream, 5, cancellationToken);
        Assert.Equal((byte)0x16, header[0]);

        var recordLength = BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan(3, 2));
        return await ReadExactAsync(stream, recordLength, cancellationToken);
    }

    private static async Task<byte[]> ReadExactAsync(Stream stream, int length, CancellationToken cancellationToken)
    {
        var buffer = new byte[length];
        var offset = 0;

        while (offset < length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, length - offset), cancellationToken);

            if (read == 0)
            {
                throw new EndOfStreamException();
            }

            offset += read;
        }

        return buffer;
    }
}
