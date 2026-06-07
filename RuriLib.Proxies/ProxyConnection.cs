using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RuriLib.Proxies;

/// <summary>
/// Represents an established proxied connection and the stream that should be
/// used to exchange bytes with the destination.
/// </summary>
public sealed class ProxyConnection : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The underlying TCP client.
    /// </summary>
    public TcpClient TcpClient { get; }

    /// <summary>
    /// The stream to use for the proxied connection.
    /// </summary>
    public Stream Stream { get; }

    /// <summary>
    /// Creates an established proxied connection.
    /// </summary>
    public ProxyConnection(TcpClient tcpClient, Stream stream)
    {
        TcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Stream.Dispose();
        TcpClient.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Stream.DisposeAsync().ConfigureAwait(false);
        TcpClient.Dispose();
    }
}
