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
    public async Task ErisaCloudflareBotScore_ChromeHttp2_ReturnsScorePage()
    {
        var body = await SendAndDumpAsync("https://cf.erisa.uk/", HttpVersion.Version20);

        Assert.False(string.IsNullOrWhiteSpace(body));
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

    [Theory(Skip = ManualSkip)]
    [InlineData(CurlImpersonateBrowserProfile.Okhttp4Android10,
        "771,4865-4866-4867-49195-49196-52393-49199-49200-52392-49171-49172-156-157-47-53,0-23-65281-10-11-35-16-5-13-51-45-43-21,29-23-24,0",
        "4:16777216|16711681|0|m,p,a,s")]
    [InlineData(CurlImpersonateBrowserProfile.SafariIos170,
        "771,4865-4866-4867-49196-49195-52393-49200-49199-52392-49162-49161-49172-49171-157-156-53-47-49160-49170-10,0-23-65281-10-11-16-5-13-18-51-45-43-27-21,29-23-24-25,0",
        "2:0;4:2097152;3:100|10485760|0|m,s,p,a")]
    [InlineData(CurlImpersonateBrowserProfile.SafariIos185,
        "771,4865-4866-4867-49196-49195-52393-49200-49199-52392-49162-49161-49172-49171-157-156-53-47-49160-49170-10,0-23-65281-10-11-16-5-13-18-51-45-43-27-21,29-23-24-25,0",
        "2:0;3:100;4:2097152;9:1|10420225|0|m,s,a,p")]
    public async Task TlsPeetAll_CustomMobileHttp2_ReportsExpectedFingerprints(
        CurlImpersonateBrowserProfile profile, string expectedJa3, string expectedAkamai)
    {
        var body = await SendAndDumpAsync("https://tls.peet.ws/api/all", HttpVersion.Version20, profile);

        AssertBodyContains(body, "\"http_version\": \"h2\"");
        AssertBodyContains(body, expectedJa3);
        AssertBodyContains(body, expectedAkamai);
    }

    [Fact(Skip = ManualSkip)]
    public async Task BrowserLeaksQuic_ChromeHttp3_ReportsQuicSupport()
    {
        var body = await SendAndDumpAsync("https://browserleaks.com/quic", HttpVersion.Version30);

        AssertBodyContains(body, "QUIC Client Test");
    }

    private async Task<string> SendAndDumpAsync(string url, Version httpVersion,
        CurlImpersonateBrowserProfile profile = CurlImpersonateBrowserProfile.Chrome142)
    {
        using var handler = new CurlImpersonateHandler(new CurlImpersonateHandlerOptions
        {
            BrowserProfile = profile,
            UseBrowserHeaders = true,
            IgnoreCertificateValidation = false,
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
