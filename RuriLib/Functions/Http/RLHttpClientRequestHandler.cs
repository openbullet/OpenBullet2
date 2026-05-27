using RuriLib.Extensions;
using RuriLib.Functions.Files;
using RuriLib.Functions.Http.Options;
using RuriLib.Helpers;
using RuriLib.Http;
using RuriLib.Http.Models;
using RuriLib.Logging;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Functions.Http;

internal class RLHttpClientRequestHandler : HttpRequestHandler
{
    public override async Task HttpRequestStandard(BotData data, StandardHttpRequestOptions options)
    {
        var clientOptions = GetClientOptions(data, options);
        using var client = HttpFactory.GetRLHttpClient(data.UseProxy ? data.Proxy : null, clientOptions);

        foreach (var cookie in options.CustomCookies)
        {
            data.COOKIES[cookie.Key] = cookie.Value;
        }

        using var request = new HttpRequest
        {
            Method = new System.Net.Http.HttpMethod(options.Method.ToString()),
            Uri = new Uri(options.Url),
            Version = Version.Parse(options.HttpVersion),
            Headers = options.CustomHeaders,
            Cookies = data.COOKIES,
            AbsoluteUriInFirstLine = options.AbsoluteUriInFirstLine
        };

        if (!string.IsNullOrEmpty(options.Content) || options.AlwaysSendContent)
        {
            var content = options.Content;

            if (options.UrlEncodeContent)
            {
                content = string.Join("", content.SplitInChunks(2080)
                    .Select(Uri.EscapeDataString))
                    .Replace($"%26", "&").Replace($"%3D", "=");
            }

            request.Content = new StringContent(content.Unescape());
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(options.ContentType);
        }

        data.Logger.LogHeader();

        try
        {
            Activity.Current = null;
            using var timeoutCts = new CancellationTokenSource(options.TimeoutMilliseconds);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(data.CancellationToken, timeoutCts.Token);
            using var response = await client.SendAsync(request, linkedCts.Token).ConfigureAwait(false);

            LogHttpRequestData(data, client);
            await LogHttpResponseData(data, response, request, options).ConfigureAwait(false);
        }
        catch
        {
            await LogHttpRequestData(data, request).ConfigureAwait(false);
            throw;
        }
    }

    public override async Task HttpRequestRaw(BotData data, RawHttpRequestOptions options)
    {
        var clientOptions = GetClientOptions(data, options);
        using var client = HttpFactory.GetRLHttpClient(data.UseProxy ? data.Proxy : null, clientOptions);

        foreach (var cookie in options.CustomCookies)
        {
            data.COOKIES[cookie.Key] = cookie.Value;
        }

        using var request = new HttpRequest
        {
            Method = new System.Net.Http.HttpMethod(options.Method.ToString()),
            Uri = new Uri(options.Url),
            Version = Version.Parse(options.HttpVersion),
            Headers = options.CustomHeaders,
            Cookies = data.COOKIES,
            AbsoluteUriInFirstLine = options.AbsoluteUriInFirstLine,
            Content = new ByteArrayContent(options.Content)
        };

        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(options.ContentType);

        data.Logger.LogHeader();

        try
        {
            Activity.Current = null;
            using var timeoutCts = new CancellationTokenSource(options.TimeoutMilliseconds);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(data.CancellationToken, timeoutCts.Token);
            using var response = await client.SendAsync(request, linkedCts.Token).ConfigureAwait(false);

            LogHttpRequestData(data, client);
            await LogHttpResponseData(data, response, request, options).ConfigureAwait(false);
        }
        catch
        {
            await LogHttpRequestData(data, request).ConfigureAwait(false);
            throw;
        }
    }

    public override async Task HttpRequestBasicAuth(BotData data, BasicAuthHttpRequestOptions options)
    {
        var clientOptions = GetClientOptions(data, options);
        using var client = HttpFactory.GetRLHttpClient(data.UseProxy ? data.Proxy : null, clientOptions);

        foreach (var cookie in options.CustomCookies)
        {
            data.COOKIES[cookie.Key] = cookie.Value;
        }

        using var request = new HttpRequest
        {
            Method = new System.Net.Http.HttpMethod(options.Method.ToString()),
            Uri = new Uri(options.Url),
            Version = Version.Parse(options.HttpVersion),
            Headers = options.CustomHeaders,
            Cookies = data.COOKIES,
            AbsoluteUriInFirstLine = options.AbsoluteUriInFirstLine
        };

        // Add the basic auth header
        request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{options.Username}:{options.Password}")));

        data.Logger.LogHeader();

        try
        {
            Activity.Current = null;
            using var timeoutCts = new CancellationTokenSource(options.TimeoutMilliseconds);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(data.CancellationToken, timeoutCts.Token);
            using var response = await client.SendAsync(request, linkedCts.Token).ConfigureAwait(false);

            LogHttpRequestData(data, client);
            await LogHttpResponseData(data, response, request, options).ConfigureAwait(false);
        }
        catch
        {
            await LogHttpRequestData(data, request).ConfigureAwait(false);
            throw;
        }
    }

    public override async Task HttpRequestMultipart(BotData data, MultipartHttpRequestOptions options)
    {
        var clientOptions = GetClientOptions(data, options);
        using var client = HttpFactory.GetRLHttpClient(data.UseProxy ? data.Proxy : null, clientOptions);

        foreach (var cookie in options.CustomCookies)
        {
            data.COOKIES[cookie.Key] = cookie.Value;
        }

        if (string.IsNullOrWhiteSpace(options.Boundary))
        {
            options.Boundary = GenerateMultipartBoundary();
        }

        // Rewrite the value of the Content-Type header otherwise it will add double quotes around it like
        // Content-Type: multipart/form-data; boundary="------WebKitFormBoundaryewozmkbxwbblilpm"
        var multipartContent = new MultipartFormDataContent(options.Boundary);
        var boundaryParameter = multipartContent.Headers.ContentType?.Parameters
            .FirstOrDefault(o => o.Name == "boundary");

        if (boundaryParameter is not null)
        {
            boundaryParameter.Value = options.Boundary;
        }

        FileStream? fileStream = null;

        foreach (var c in options.Contents)
        {
            switch (c)
            {
                case StringHttpContent x:
                    multipartContent.Add(CreateMultipartContent(x), x.Name);
                    break;

                case RawHttpContent x:
                    multipartContent.Add(CreateMultipartContent(x), x.Name);
                    break;

                case FileHttpContent x:
                    lock (FileLocker.GetHandle(x.FileName))
                    {
                        if (data.Providers.Security.RestrictBlocksToCWD)
                        {
                            FileUtils.ThrowIfNotInCWD(x.FileName);
                        }

                        fileStream = new FileStream(x.FileName, FileMode.Open);
                        var fileContent = CreateMultipartContent(x, fileStream);
                        multipartContent.Add(fileContent, x.Name);
                    }
                    break;
            }
        }

        using var request = new HttpRequest
        {
            Method = new System.Net.Http.HttpMethod(options.Method.ToString()),
            Uri = new Uri(options.Url),
            Version = Version.Parse(options.HttpVersion),
            Headers = options.CustomHeaders,
            Cookies = data.COOKIES,
            AbsoluteUriInFirstLine = options.AbsoluteUriInFirstLine,
            Content = multipartContent
        };

        data.Logger.LogHeader();

        try
        {
            Activity.Current = null;
            using var timeoutCts = new CancellationTokenSource(options.TimeoutMilliseconds);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(data.CancellationToken, timeoutCts.Token);
            using var response = await client.SendAsync(request, linkedCts.Token).ConfigureAwait(false);

            LogHttpRequestData(data, client);
            await LogHttpResponseData(data, response, request, options).ConfigureAwait(false);
        }
        catch
        {
            await LogHttpRequestData(data, request, options.Boundary, options.Contents).ConfigureAwait(false);
            throw;
        }
        finally
        {
            if (fileStream is not null)
            {
                await fileStream.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private static async Task LogHttpRequestData(BotData data, HttpRequest request,
        string? boundary = null, List<MyHttpContent>? multipartContents = null)
    {
        await using var writer = new StringWriter();

        // Log the method, uri and http version
        var uri = request.Uri ?? throw new InvalidOperationException("Request URI cannot be null.");
        await writer.WriteLineAsync($"{request.Method.Method} {uri.PathAndQuery} HTTP/{request.Version.Major}.{request.Version.Minor}");

        // Log the headers
        if (!request.HeaderExists("Host", out _))
        {
            await writer.WriteLineAsync($"Host: {uri.Host}");
        }

        foreach (var header in request.Headers)
        {
            var separator = commaHeaders.Contains(header.Key) ? ", " : " ";
            await writer.WriteLineAsync($"{header.Key}: {string.Join(separator, header.Value)}");
        }

        // Log the cookie header
        var cookies = data.COOKIES.Select(c => $"{c.Key}={c.Value}");

        var cookiesArray = cookies as string[] ?? cookies.ToArray();
        if (cookiesArray.Length != 0)
        {
            await writer.WriteLineAsync($"Cookie: {string.Join("; ", cookiesArray)}");
        }

        if (request.Content != null)
        {
            switch (request.Content)
            {
                case StringContent x:
                    await writer.WriteLineAsync($"Content-Type: {x.Headers.ContentType}");
                    await writer.WriteLineAsync($"Content-Length: {x.Headers.ContentLength}");
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync(await x.ReadAsStringAsync(data.CancellationToken).ConfigureAwait(false));
                    break;

                case ByteArrayContent x:
                    await writer.WriteLineAsync($"Content-Type: {x.Headers.ContentType}");
                    await writer.WriteLineAsync($"Content-Length: {x.Headers.ContentLength}");
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync(Conversion.HexConverter.ToHexString(
                        await x.ReadAsByteArrayAsync(data.CancellationToken).ConfigureAwait(false)));
                    break;

                case MultipartFormDataContent x:
                    await writer.WriteLineAsync($"Content-Type: multipart/form-data; boundary=\"{boundary}\"");
                    var serializedMultipart = SerializeMultipart(boundary ?? string.Empty, multipartContents ?? []);
                    await writer.WriteLineAsync("Content-Length: (not calculated)");
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync(serializedMultipart);
                    break;
            }
        }

        data.Logger.Log(writer.ToString(), LogColors.NonPhotoBlue);
    }

    private static void LogHttpRequestData(BotData data, RLHttpClient client)
    {
        for (var i = 0; i < client.RawRequests.Count; i++)
        {
            if (i > 0)
            {
                data.Logger.Log($"Redirect {i}", LogColors.Beige);
            }

            data.Logger.Log(Encoding.UTF8.GetString(client.RawRequests[i]), LogColors.NonPhotoBlue);
        }
    }

    private static async Task LogHttpResponseData(BotData data, HttpResponse response, HttpRequest request,
        Options.HttpRequestOptions requestOptions)
    {
        // Try to read the raw source for Content-Length calculation
        var responseContent = response.Content;
        if (responseContent is null)
        {
            data.RAWSOURCE = [];
        }
        else
        {
            data.RAWSOURCE = await responseContent.ReadAsByteArrayAsync(data.CancellationToken).ConfigureAwait(false);
        }

        // Address
        var requestUri = request.Uri ?? throw new InvalidOperationException("Request URI cannot be null.");
        var responseRequestUri = response.Request?.Uri ?? requestUri;
        var resolvedUri = responseRequestUri.IsAbsoluteUri
            ? responseRequestUri
            : new Uri(requestUri, responseRequestUri);

        data.ADDRESS = resolvedUri.AbsoluteUri;
        data.Logger.Log($"Address: {data.ADDRESS}", LogColors.DodgerBlue);

        // Response code
        data.RESPONSECODE = (int)response.StatusCode;
        data.Logger.Log($"Response code: {data.RESPONSECODE}", LogColors.Citrine);
        data.Logger.Log($"Response HTTP version: HTTP/{response.Version.Major}.{response.Version.Minor}",
            LogColors.Citrine);

        // Headers
        data.HEADERS = response.Headers;
        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                data.HEADERS[header.Key] = header.Value.First();
            }
        }

        if (!data.HEADERS.ContainsKey("Content-Length"))
        {
            data.HEADERS["Content-Length"] = data.RAWSOURCE.Length.ToString();
        }

        data.Logger.Log("Received Headers:", LogColors.MediumPurple);
        data.Logger.Log(data.HEADERS.Select(h => $"{h.Key}: {h.Value}"), LogColors.Violet);

        // Cookies
        data.Logger.Log("Received Cookies:", LogColors.MikadoYellow);
        data.Logger.Log(data.COOKIES.Select(h => $"{h.Key}: {h.Value}"), LogColors.Khaki);

        // Unzip the GZipped content if still gzipped (after Content-Length calculation)
        if (data.RAWSOURCE.Length > 1 && data.RAWSOURCE[0] == 0x1F && data.RAWSOURCE[1] == 0x8B)
        {
            try
            {
                data.RAWSOURCE = GZip.Unzip(data.RAWSOURCE);
            }
            catch
            {
                data.Logger.Log("Tried to unzip but failed", LogColors.DarkOrange);
            }
        }

        // Source
        if (!string.IsNullOrWhiteSpace(requestOptions.CodePagesEncoding))
        {
            var encoding = CodePagesEncodingProvider.Instance
                .GetEncoding(requestOptions.CodePagesEncoding) ?? throw new NotSupportedException(
                    $"Encoding {requestOptions.CodePagesEncoding} is not supported");
            data.SOURCE = encoding.GetString(data.RAWSOURCE);
        }
        else
        {
            data.SOURCE = Encoding.UTF8.GetString(data.RAWSOURCE);
        }

        if (requestOptions.DecodeHtml)
        {
            data.SOURCE = WebUtility.HtmlDecode(data.SOURCE);
        }

        data.Logger.Log("Received Payload:", LogColors.ForestGreen);
        data.Logger.Log(data.SOURCE, LogColors.GreenYellow, true);
    }
}
