using System;
using RuriLib.Http.Models;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Http.Tests;

public class ContentEncodingTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(TestClientKind.RLHttpClient)]
    [InlineData(TestClientKind.ProxyClientHandler)]
    public async Task SendAsync_ContentEncoding_IgnoresIdentityLikeValues(TestClientKind clientKind)
    {
        const string expected = "plain response";

        await using var server = LocalHttpResponseServer.Create(
            Encoding.UTF8.GetBytes(expected),
            "Content-Type: text/plain; charset=utf-8",
            "Content-Encoding: none, true");

        var actual = await SendAsync(clientKind, server.Uri);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(TestClientKind.RLHttpClient)]
    [InlineData(TestClientKind.ProxyClientHandler)]
    public async Task SendAsync_ContentEncoding_IgnoresUtf8PseudoEncoding(TestClientKind clientKind)
    {
        const string expected = "utf8 body";

        await using var server = LocalHttpResponseServer.Create(
            Encoding.UTF8.GetBytes(expected),
            "Content-Type: text/plain; charset=utf-8",
            "Content-Encoding: utf-8");

        var actual = await SendAsync(clientKind, server.Uri);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(TestClientKind.RLHttpClient)]
    [InlineData(TestClientKind.ProxyClientHandler)]
    public async Task SendAsync_ContentEncoding_DecodesMultipleValuesInReverseOrder(TestClientKind clientKind)
    {
        const string expected = "encoded twice";

        var payload = Encoding.UTF8.GetBytes(expected);
        payload = LocalHttpResponseServer.Gzip(payload);
        payload = LocalHttpResponseServer.Brotli(payload);

        await using var server = LocalHttpResponseServer.Create(
            payload,
            "Content-Type: text/plain; charset=utf-8",
            "Content-Encoding: gzip, br");

        var actual = await SendAsync(clientKind, server.Uri);

        Assert.Equal(expected, actual);
    }

    private static async Task<string> SendAsync(TestClientKind clientKind, Uri uri)
        => clientKind switch
        {
            TestClientKind.RLHttpClient => await SendWithRlHttpClientAsync(uri),
            TestClientKind.ProxyClientHandler => await SendWithProxyClientHandlerAsync(uri),
            _ => throw new System.NotSupportedException($"Unsupported test client kind {clientKind}")
        };

    private static async Task<string> SendWithRlHttpClientAsync(Uri uri)
    {
        using var client = new RLHttpClient(new NoProxyClient(new ProxySettings()));
        using var response = await client.SendAsync(new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = uri
        }, TestCancellationToken);

        Assert.NotNull(response.Content);
        return await response.Content.ReadAsStringAsync(TestCancellationToken);
    }

    private static async Task<string> SendWithProxyClientHandlerAsync(Uri uri)
    {
        using var handler = new ProxyClientHandler(new NoProxyClient(new ProxySettings()))
        {
            CookieContainer = new CookieContainer()
        };

        using var client = new HttpClient(handler);
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri), TestCancellationToken);
        return await response.Content.ReadAsStringAsync(TestCancellationToken);
    }

    public enum TestClientKind
    {
        RLHttpClient,
        ProxyClientHandler
    }
}
