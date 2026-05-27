using RuriLib.Http.Curl;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Http.Tests;

public class CurlImpersonateManualTargetTests(ITestOutputHelper output)
{
    private const string ManualSkip =
        "Manual network fingerprint test. Remove Skip locally when validating curl-impersonate against real sites.";

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact(Skip = ManualSkip)]
    public async Task CloudflareQuic_ChromeHttp3_ReportsExpectedBrowserSignals()
    {
        var body = await SendAndDumpAsync("https://cloudflare-quic.com/", HttpVersion.Version30);

        AssertBodyContains(body, "Cloudflare Supports QUIC");
    }

    [Fact(Skip = ManualSkip)]
    public async Task CloudflareTrace_ChromeHttp2_ReportsBrowserTrace()
    {
        var body = await SendAndDumpAsync("https://www.cloudflare.com/cdn-cgi/trace", HttpVersion.Version20);

        AssertBodyContains(body, "uag=Mozilla/5.0");
        AssertBodyContains(body, "Chrome/142.0.0.0");
        AssertBodyContains(body, "http=http/2");
        AssertBodyContains(body, "tls=TLSv1.3");
    }

    [Fact(Skip = ManualSkip)]
    public async Task DeviceAndBrowserInfoHeaders_ChromeHttp2_ReportsBrowserHeaders()
    {
        var body = await SendAndDumpAsync("https://deviceandbrowserinfo.com/api/http_headers",
            HttpVersion.Version20);

        AssertBodyContains(body, "sec-ch-ua");
        AssertBodyContains(body, "Chrome\\\";v=\\\"142");
        AssertBodyContains(body, "User-Agent");
        AssertBodyContains(body, "Chrome/142.0.0.0");
        AssertBodyContains(body, "Sec-Fetch-Mode");
    }

    [Fact(Skip = ManualSkip)]
    public async Task TlsPeetTls_ChromeHttp2_ReportsBrowserTlsFingerprint()
    {
        var body = await SendAndDumpAsync("https://tls.peet.ws/api/tls", HttpVersion.Version20);

        AssertBodyContains(body, "TLS_GREASE");
        AssertBodyContains(body, "application_layer_protocol_negotiation");
        AssertBodyContains(body, "\"h2\"");
    }
    
    [Fact(Skip = ManualSkip)]
    public async Task TlsPeetAll_ChromeHttp2_ReportsTlsAndHttp2Fingerprints()
    {
        var body = await SendAndDumpAsync("https://tls.peet.ws/api/all", HttpVersion.Version20);

        AssertBodyContains(body, "\"http_version\": \"h2\"");
        AssertBodyContains(body, "Chrome/142.0.0.0");
        AssertBodyContains(body, "application_layer_protocol_negotiation");
        AssertBodyContains(body, "application_settings");
    }

    [Fact(Skip = ManualSkip)]
    public async Task BrowserLeaksQuic_ChromeHttp3_ReportsQuicSupport()
    {
        var body = await SendAndDumpAsync("https://browserleaks.com/quic", HttpVersion.Version30);

        AssertBodyContains(body, "QUIC Client Test");
    }

    private async Task<string> SendAndDumpAsync(string url, Version httpVersion)
    {
        using var handler = new CurlImpersonateHandler(new CurlImpersonateHandlerOptions
        {
            BrowserProfile = CurlImpersonateBrowserProfile.Chrome142,
            UseBrowserHeaders = true,
            // The Windows curl-impersonate build uses its bundled TLS stack and
            // may not have access to the OS CA store. These manual tests validate
            // fingerprint/browser classification, not certificate chain handling.
            IgnoreCertificateValidation = true,
            AllowAutoRedirect = true,
            ConnectTimeout = TimeSpan.FromSeconds(10),
            Timeout = TimeSpan.FromSeconds(30)
        });
        using var client = new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
        using var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Version = httpVersion
        };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead,
            TestCancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestCancellationToken);

        DumpResponse(url, httpVersion, response, body);

        Assert.True(response.IsSuccessStatusCode,
            $"Expected success status, got {(int)response.StatusCode} {response.ReasonPhrase}.\n{TrimForAssertion(body)}");
        Assert.False(string.IsNullOrWhiteSpace(body));

        return body;
    }

    private void DumpResponse(string url, Version requestedVersion, HttpResponseMessage response, string body)
    {
        output.WriteLine($"URL: {url}");
        output.WriteLine($"Requested HTTP version: {requestedVersion}");
        output.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        output.WriteLine($"Response HTTP version: {response.Version}");

        foreach (var header in response.Headers.OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase))
        {
            output.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        foreach (var header in response.Content.Headers.OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase))
        {
            output.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        output.WriteLine("Body:");
        output.WriteLine(TrimForAssertion(body));
    }

    private static void AssertBodyContains(string body, string expected)
        => Assert.True(body.Contains(expected, StringComparison.OrdinalIgnoreCase),
            $"Expected response body to contain '{expected}'.\n{TrimForAssertion(body)}");

    private static string TrimForAssertion(string body)
        => body.Length <= 4096 ? body : $"{body[..4096]}\n... truncated ...";
}
