using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Tests.Utils;

internal sealed class TestDnsServer : IAsyncDisposable
{
    private readonly UdpClient udpClient = new(new IPEndPoint(IPAddress.Loopback, 0));
    private readonly TcpListener tcpListener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly Task udpTask;
    private readonly Task tcpTask;
    private readonly byte[] answerBytes;

    private TestDnsServer(byte[] answerBytes)
    {
        this.answerBytes = answerBytes;
        tcpListener.Start();
        udpTask = Task.Run(() => RunUdpAsync(cancellationTokenSource.Token), CancellationToken.None);
        tcpTask = Task.Run(() => RunTcpAsync(cancellationTokenSource.Token), CancellationToken.None);
    }

    public string Host => "127.0.0.1";

    public int UdpPort => ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;

    public int TcpPort => ((IPEndPoint)tcpListener.LocalEndpoint).Port;

    public static TestDnsServer CreateARecord(IPAddress address)
        => new(address.GetAddressBytes());

    public async ValueTask DisposeAsync()
    {
        cancellationTokenSource.Cancel();
        udpClient.Dispose();
        tcpListener.Stop();

        try
        {
            await Task.WhenAll(udpTask, tcpTask);
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

    private async Task RunUdpAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await udpClient.ReceiveAsync(cancellationToken);
            var response = BuildAResponse(result.Buffer, answerBytes);
            await udpClient.SendAsync(response, result.RemoteEndPoint, cancellationToken);
        }
    }

    private async Task RunTcpAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var client = await tcpListener.AcceptTcpClientAsync(cancellationToken);
            await using var stream = client.GetStream();

            var lengthBuffer = new byte[2];
            await ReadExactlyAsync(stream, lengthBuffer, cancellationToken);
            var messageLength = BinaryPrimitives.ReadUInt16BigEndian(lengthBuffer);

            var request = new byte[messageLength];
            await ReadExactlyAsync(stream, request, cancellationToken);

            var response = BuildAResponse(request, answerBytes);
            var prefix = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(prefix, (ushort)response.Length);

            await stream.WriteAsync(prefix, cancellationToken);
            await stream.WriteAsync(response, cancellationToken);
        }
    }

    private static async Task ReadExactlyAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;

        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken);
            if (read == 0)
            {
                throw new EndOfStreamException("Unexpected end of DNS TCP stream");
            }

            offset += read;
        }
    }

    private static byte[] BuildAResponse(byte[] request, byte[] answerBytes)
    {
        var questionLength = 12;

        while (questionLength < request.Length && request[questionLength] != 0)
        {
            questionLength += request[questionLength] + 1;
        }

        questionLength += 1 + 4;

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write(request[0]);
        writer.Write(request[1]);
        writer.Write((byte)0x81);
        writer.Write((byte)0x80);
        writer.Write((byte)0x00);
        writer.Write((byte)0x01);
        writer.Write((byte)0x00);
        writer.Write((byte)0x01);
        writer.Write((byte)0x00);
        writer.Write((byte)0x00);
        writer.Write((byte)0x00);
        writer.Write((byte)0x00);
        writer.Write(request, 12, questionLength - 12);
        writer.Write((byte)0xC0);
        writer.Write((byte)0x0C);
        writer.Write((byte)0x00);
        writer.Write((byte)0x01);
        writer.Write((byte)0x00);
        writer.Write((byte)0x01);
        writer.Write((byte)0x00);
        writer.Write((byte)0x00);
        writer.Write((byte)0x00);
        writer.Write((byte)0x3C);
        writer.Write((byte)0x00);
        writer.Write((byte)answerBytes.Length);
        writer.Write(answerBytes);

        return ms.ToArray();
    }
}
