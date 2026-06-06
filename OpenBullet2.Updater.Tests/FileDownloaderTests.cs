using System.Net;
using OpenBullet2.Updater.Core.Helpers;
using Xunit;

namespace OpenBullet2.Updater.Tests;

public class FileDownloaderTests
{
    [Fact]
    public async Task DownloadAsync_WritesContentToFileBackedStream()
    {
        var payload = "downloaded content"u8.ToArray();
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(payload)
            }));
        var progress = new ProgressCollector();

        await using var stream = await FileDownloader.DownloadAsync(client, "https://example.com/file.zip", progress);

        Assert.IsNotType<MemoryStream>(stream);
        Assert.Equal(payload, await ReadAllBytesAsync(stream));
        Assert.NotEmpty(progress.Values);
        Assert.Equal(100, progress.Values.Last(), precision: 5);
    }

    [Fact]
    public async Task DownloadAsync_DoesNotRequireContentLength()
    {
        var payload = "downloaded content without length"u8.ToArray();
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new UnknownLengthContent(payload)
            }));
        var progress = new ProgressCollector();

        await using var stream = await FileDownloader.DownloadAsync(client, "https://example.com/file.zip", progress);

        Assert.Equal(payload, await ReadAllBytesAsync(stream));
        Assert.Empty(progress.Values);
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private sealed class ProgressCollector : IProgress<double>
    {
        public List<double> Values { get; } = [];

        public void Report(double value) => Values.Add(value);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }

    private sealed class UnknownLengthContent(byte[] payload) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => stream.WriteAsync(payload).AsTask();

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
