using RuriLib.Exceptions;
using RuriLib.Blocks.Requests.Tcp;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Proxies;
using RuriLib.Tests.Utils;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TcpMethods = RuriLib.Blocks.Requests.Tcp.Methods;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;

namespace RuriLib.Tests.Blocks.Requests;

[Collection(nameof(TcpRequestBlocksCollection))]
public class TcpRequestBlocksTests
{
    [Fact]
    public async Task TcpRead_WithoutConnection_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            TcpMethods.TcpRead(data));

        Assert.Equal("You have to create a connection first!", ex.Message);
    }

    [Fact]
    public async Task TcpConnect_SendRead_EchoesAsciiMessage()
    {
        await using var server = TestTcpServer.CreateEchoServer();
        var data = NewBotData();

        try
        {
            await TcpMethods.TcpConnect(data, server.Host, server.Port, useSSL: false);

            var response = await TcpMethods.TcpSendRead(data, "PING");

            Assert.Equal("PING\r\n", response);
        }
        finally
        {
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Fact]
    public async Task TcpConnect_SendRead_WithUtf8_EchoesMessage()
    {
        await using var server = TestTcpServer.CreateEchoServer();
        var data = NewBotData();

        try
        {
            await TcpMethods.TcpConnect(data, server.Host, server.Port, useSSL: false);

            var response = await TcpMethods.TcpSendRead(
                data,
                "ciao \u00E8",
                terminateWithCRLF: false,
                useUTF8: true);

            Assert.Equal("ciao \u00E8", response);
        }
        finally
        {
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Fact]
    public async Task TcpConnect_SendBytes_ThenReadBytes_Verify()
    {
        await using var server = TestTcpServer.CreateEchoServer();
        var data = NewBotData();
        var payload = new byte[] { 1, 2, 3, 127, 255 };

        try
        {
            await TcpMethods.TcpConnect(data, server.Host, server.Port, useSSL: false);

            await TcpMethods.TcpSendBytes(data, payload);
            var response = await TcpMethods.TcpReadBytes(data, bytesToRead: payload.Length);

            Assert.Equal(payload, response);
        }
        finally
        {
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Fact]
    public async Task TcpConnect_SendReadBytes_EchoesBinaryPayload()
    {
        await using var server = TestTcpServer.CreateEchoServer();
        var data = NewBotData();
        var payload = new byte[] { 0, 10, 13, 200, 255 };

        try
        {
            await TcpMethods.TcpConnect(data, server.Host, server.Port, useSSL: false);

            var response = await TcpMethods.TcpSendReadBytes(data, payload, bytesToRead: payload.Length);

            Assert.Equal(payload, response);
        }
        finally
        {
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Fact]
    public async Task TcpRead_ReadsGreetingFromServer()
    {
        await using var server = TestTcpServer.CreateResponseServer("HELLO", Encoding.ASCII);
        var data = NewBotData();

        try
        {
            await TcpMethods.TcpConnect(data, server.Host, server.Port, useSSL: false);

            var response = await TcpMethods.TcpRead(data, bytesToRead: 5);

            Assert.Equal("HELLO", response);
        }
        finally
        {
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Fact]
    public async Task TcpSendReadHttp_ReadsWholeGzipResponse()
    {
        await using var server = TestTcpServer.CreateHttpServer("tcp-http-ok", gzip: true);
        var data = NewBotData();

        try
        {
            await TcpMethods.TcpConnect(data, server.Host, server.Port, useSSL: false);

            var response = await TcpMethods.TcpSendReadHttp(
                data,
                $"GET / HTTP/1.1\\r\\nHost: {server.Host}:{server.Port}\\r\\nConnection: close\\r\\n\\r\\n");

            Assert.Contains("HTTP/1.1 200 OK", response, StringComparison.Ordinal);
            Assert.Contains("Content-Encoding: gzip", response, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith("tcp-http-ok", response, StringComparison.Ordinal);
        }
        finally
        {
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Fact]
    public async Task TcpSendReadHttp_ContentLengthKeepAlive_DoesNotWaitForSocketClose()
    {
        await using var server = TestTcpServer.CreateHttpServer(
            "tcp-http-keepalive",
            keepAlive: true,
            keepAliveDuration: TimeSpan.FromSeconds(5));
        var data = NewBotData();

        try
        {
            await TcpMethods.TcpConnect(data, server.Host, server.Port, useSSL: false);

            var response = await TcpMethods.TcpSendReadHttp(
                data,
                $"GET / HTTP/1.1\\r\\nHost: {server.Host}:{server.Port}\\r\\n\\r\\n",
                timeoutMilliseconds: 1000);

            Assert.Contains("HTTP/1.1 200 OK", response, StringComparison.Ordinal);
            Assert.Contains("Connection: keep-alive", response, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith("tcp-http-keepalive", response, StringComparison.Ordinal);
        }
        finally
        {
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Fact]
    public async Task TcpSendReadHttp_ChunkedKeepAlive_DoesNotWaitForSocketClose()
    {
        await using var server = TestTcpServer.CreateHttpServer(
            "tcp-http-chunked",
            keepAlive: true,
            chunked: true,
            keepAliveDuration: TimeSpan.FromSeconds(5));
        var data = NewBotData();

        try
        {
            await TcpMethods.TcpConnect(data, server.Host, server.Port, useSSL: false);

            var response = await TcpMethods.TcpSendReadHttp(
                data,
                $"GET / HTTP/1.1\\r\\nHost: {server.Host}:{server.Port}\\r\\n\\r\\n",
                timeoutMilliseconds: 1000);

            Assert.Contains("HTTP/1.1 200 OK", response, StringComparison.Ordinal);
            Assert.Contains("Transfer-Encoding: chunked", response, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith("tcp-http-chunked", response, StringComparison.Ordinal);
        }
        finally
        {
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Fact]
    public async Task TcpDisconnect_MakesFurtherSocketReadsFail()
    {
        await using var server = TestTcpServer.CreateResponseServer("BYE", Encoding.ASCII);
        var data = NewBotData();

        await TcpMethods.TcpConnect(data, server.Host, server.Port, useSSL: false);

        TcpMethods.TcpDisconnect(data);

        var ex = await Record.ExceptionAsync(() => TcpMethods.TcpRead(data));

        Assert.NotNull(ex);
        Assert.True(
            ex is ObjectDisposedException or IOException,
            $"Unexpected exception type: {ex.GetType().FullName}");
    }

    [Fact]
    public async Task TcpConnect_WithSsl_AndTrustedTestCertificate_Succeeds()
    {
        using var certificate = TestTcpServer.CreateSelfSignedCertificate("localhost");
        await using var server = TestTcpServer.CreateTlsEchoServer(certificate);
        var data = NewBotData();
        Methods.ServerCertificateValidationCallback = (_, _, _, _) => true;

        try
        {
            await TcpMethods.TcpConnect(data, "localhost", server.Port, useSSL: true);

            var response = await TcpMethods.TcpSendRead(data, "TLS");

            Assert.Equal("TLS\r\n", response);
        }
        finally
        {
            Methods.ServerCertificateValidationCallback = null;
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Fact]
    public async Task TcpConnect_WithSsl_SendReadBytes_EchoesBinaryPayload()
    {
        using var certificate = TestTcpServer.CreateSelfSignedCertificate("localhost");
        await using var server = TestTcpServer.CreateTlsEchoServer(certificate);
        var data = NewBotData();
        var payload = new byte[] { 5, 10, 15, 20, 25 };
        Methods.ServerCertificateValidationCallback = (_, _, _, _) => true;

        try
        {
            await TcpMethods.TcpConnect(data, "localhost", server.Port, useSSL: true);

            var response = await TcpMethods.TcpSendReadBytes(data, payload, bytesToRead: payload.Length);

            Assert.Equal(payload, response);
        }
        finally
        {
            Methods.ServerCertificateValidationCallback = null;
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Fact]
    public async Task TcpConnect_ReconnectFromSslToPlainWithoutDisconnect_UsesNewConnection()
    {
        using var certificate = TestTcpServer.CreateSelfSignedCertificate("localhost");
        await using var tlsServer = TestTcpServer.CreateTlsEchoServer(certificate);
        await using var plainServer = TestTcpServer.CreateEchoServer();
        var data = NewBotData();
        Methods.ServerCertificateValidationCallback = (_, _, _, _) => true;

        try
        {
            await TcpMethods.TcpConnect(data, "localhost", tlsServer.Port, useSSL: true);
            var tlsResponse = await TcpMethods.TcpSendRead(data, "TLS");

            await TcpMethods.TcpConnect(data, plainServer.Host, plainServer.Port, useSSL: false);
            var plainResponse = await TcpMethods.TcpSendRead(data, "PLAIN");

            Assert.Equal("TLS\r\n", tlsResponse);
            Assert.Equal("PLAIN\r\n", plainResponse);
        }
        finally
        {
            Methods.ServerCertificateValidationCallback = null;
            TcpMethods.TcpDisconnect(data);
        }
    }

    [Theory]
    [InlineData(ProxyType.Http)]
    [InlineData(ProxyType.Socks4)]
    [InlineData(ProxyType.Socks4a)]
    [InlineData(ProxyType.Socks5)]
    public async Task TcpConnect_ThroughProxy_SendReadHttp_Verify(ProxyType proxyType)
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var data = NewBotData(connection.CreateProxy(proxyType));
        var request = $"GET /anything?tcpProxy={proxyType} HTTP/1.1\\r\\nHost: {connection.TargetIpAddress}:{connection.TargetPort}\\r\\nConnection: close\\r\\n\\r\\n";

        try
        {
            await TcpMethods.TcpConnect(data, connection.TargetIpAddress, connection.TargetPort, useSSL: false);

            var response = await TcpMethods.TcpSendReadHttp(data, request);

            Assert.Contains("HTTP/1.1 200 OK", response, StringComparison.Ordinal);
            Assert.Contains($"\"tcpProxy\": \"{proxyType}\"", response, StringComparison.Ordinal);
        }
        finally
        {
            TcpMethods.TcpDisconnect(data);
        }
    }

    private static BotData NewBotData(Proxy? proxy = null)
        => new(
            new BotProviders(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("hello", new WordlistType()),
            proxy,
            proxy is not null);
}

[CollectionDefinition(nameof(TcpRequestBlocksCollection), DisableParallelization = true)]
public class TcpRequestBlocksCollection;
