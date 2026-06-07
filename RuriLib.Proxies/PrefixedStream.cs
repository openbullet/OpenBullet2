using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Proxies;

internal sealed class PrefixedStream : Stream
{
    private readonly Stream innerStream;
    private readonly byte[] prefix;
    private int prefixOffset;

    public override bool CanRead => innerStream.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => innerStream.CanWrite;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public PrefixedStream(Stream innerStream, byte[] prefix)
    {
        this.innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        this.prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
    }

    public override void Flush() => innerStream.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken)
        => innerStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count)
    {
        var prefixedBytes = ReadPrefix(buffer.AsSpan(offset, count));
        return prefixedBytes > 0
            ? prefixedBytes
            : innerStream.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
        var prefixedBytes = ReadPrefix(buffer);
        return prefixedBytes > 0
            ? prefixedBytes
            : innerStream.Read(buffer);
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
        CancellationToken cancellationToken)
    {
        var prefixedBytes = ReadPrefix(buffer.AsSpan(offset, count));
        return prefixedBytes > 0
            ? prefixedBytes
            : await innerStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        var prefixedBytes = ReadPrefix(buffer.Span);
        return prefixedBytes > 0
            ? ValueTask.FromResult(prefixedBytes)
            : innerStream.ReadAsync(buffer, cancellationToken);
    }

    public override void Write(byte[] buffer, int offset, int count)
        => innerStream.Write(buffer, offset, count);

    public override void Write(ReadOnlySpan<byte> buffer)
        => innerStream.Write(buffer);

    public override Task WriteAsync(byte[] buffer, int offset, int count,
        CancellationToken cancellationToken)
        => innerStream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default)
        => innerStream.WriteAsync(buffer, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => innerStream.SetLength(value);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            innerStream.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await innerStream.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private int ReadPrefix(Span<byte> buffer)
    {
        var remaining = prefix.Length - prefixOffset;

        if (remaining <= 0 || buffer.Length == 0)
        {
            return 0;
        }

        var bytesToCopy = Math.Min(remaining, buffer.Length);
        prefix.AsSpan(prefixOffset, bytesToCopy).CopyTo(buffer);
        prefixOffset += bytesToCopy;
        return bytesToCopy;
    }
}
