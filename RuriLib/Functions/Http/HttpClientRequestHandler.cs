using RuriLib.Extensions;
using RuriLib.Functions.Conversion;
using RuriLib.Functions.Files;
using RuriLib.Functions.Http.Options;
using RuriLib.Helpers;
using RuriLib.Logging;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Functions.Http
{
    internal class HttpClientRequestHandler : HttpRequestHandler
    {
        public async override Task HttpRequestStandard(BotData data, StandardHttpRequestOptions options)
        {
            foreach (var cookie in options.CustomCookies)
                data.COOKIES[cookie.Key] = cookie.Value;

            var cookieContainer = new CookieContainer();

            foreach (var cookie in data.COOKIES)
            {
                cookieContainer.Add(new Uri(options.Url), new Cookie(cookie.Key, cookie.Value));
            }

            var clientOptions = GetClientOptions(data, options);
            using var client = HttpFactory.GetHttpClient(data.UseProxy ? data.Proxy : null, clientOptions, cookieContainer);

            using var request = new HttpRequestMessage
            {
                Method = new System.Net.Http.HttpMethod(options.Method.ToString()),
                RequestUri = new Uri(options.Url),
                Version = Version.Parse(options.HttpVersion)
            };

            foreach (var header in options.CustomHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            string content = null;

            if (!string.IsNullOrEmpty(options.Content) || options.AlwaysSendContent)
            {
                content = options.Content;

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
            LogHttpRequestData(data, request, content);

            Activity.Current = null;
            using var timeoutCts = new CancellationTokenSource(options.TimeoutMilliseconds);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(data.CancellationToken, timeoutCts.Token);
            using var response = await client.SendAsync(request, linkedCts.Token).ConfigureAwait(false);

            await LogHttpResponseData(data, response, cookieContainer, options).ConfigureAwait(false);
        }

        public async override Task HttpRequestRaw(BotData data, RawHttpRequestOptions options)
        {
            foreach (var cookie in options.CustomCookies)
                data.COOKIES[cookie.Key] = cookie.Value;

            var cookieContainer = new CookieContainer();

            foreach (var cookie in data.COOKIES)
            {
                cookieContainer.Add(new Uri(options.Url), new Cookie(cookie.Key, cookie.Value));
            }

            var clientOptions = GetClientOptions(data, options);
            using var client = HttpFactory.GetHttpClient(data.UseProxy ? data.Proxy : null, clientOptions, cookieContainer);

            using var request = new HttpRequestMessage
            {
                Method = new System.Net.Http.HttpMethod(options.Method.ToString()),
                RequestUri = new Uri(options.Url),
                Version = Version.Parse(options.HttpVersion),
                Content = new ByteArrayContent(options.Content)
            };

            foreach (var header in options.CustomHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(options.ContentType);

            data.Logger.LogHeader();
            LogHttpRequestData(data, request, Base64Converter.ToBase64String(options.Content));

            Activity.Current = null;
            using var timeoutCts = new CancellationTokenSource(options.TimeoutMilliseconds);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(data.CancellationToken, timeoutCts.Token);
            using var response = await client.SendAsync(request, linkedCts.Token).ConfigureAwait(false);

            await LogHttpResponseData(data, response, cookieContainer, options).ConfigureAwait(false);
        }

        public async override Task HttpRequestBasicAuth(BotData data, BasicAuthHttpRequestOptions options)
        {
            foreach (var cookie in options.CustomCookies)
                data.COOKIES[cookie.Key] = cookie.Value;

            var cookieContainer = new CookieContainer();

            foreach (var cookie in data.COOKIES)
            {
                cookieContainer.Add(new Uri(options.Url), new Cookie(cookie.Key, cookie.Value));
            }

            var clientOptions = GetClientOptions(data, options);
            using var client = HttpFactory.GetHttpClient(data.UseProxy ? data.Proxy : null, clientOptions, cookieContainer);

            using var request = new HttpRequestMessage
            {
                Method = new System.Net.Http.HttpMethod(options.Method.ToString()),
                RequestUri = new Uri(options.Url),
                Version = Version.Parse(options.HttpVersion)
            };

            foreach (var header in options.CustomHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Add the basic auth header
            request.Headers.TryAddWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{options.Username}:{options.Password}")));

            data.Logger.LogHeader();
            LogHttpRequestData(data, request);

            Activity.Current = null;
            using var timeoutCts = new CancellationTokenSource(options.TimeoutMilliseconds);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(data.CancellationToken, timeoutCts.Token);
            using var response = await client.SendAsync(request, linkedCts.Token).ConfigureAwait(false);

            await LogHttpResponseData(data, response, cookieContainer, options).ConfigureAwait(false);
        }
        public async override Task HttpRequestMultipart(BotData data, MultipartHttpRequestOptions options)
        {
            foreach (var cookie in options.CustomCookies)
                data.COOKIES[cookie.Key] = cookie.Value;

            var cookieContainer = new CookieContainer();

            foreach (var cookie in data.COOKIES)
            {
                cookieContainer.Add(new Uri(options.Url), new Cookie(cookie.Key, cookie.Value));
            }

            var clientOptions = GetClientOptions(data, options);
            using var client = HttpFactory.GetHttpClient(data.UseProxy ? data.Proxy : null, clientOptions, cookieContainer);

            if (string.IsNullOrWhiteSpace(options.Boundary))
                options.Boundary = GenerateMultipartBoundary();

            // Rewrite the value of the Content-Type header otherwise it will add double quotes around it like
            // Content-Type: multipart/form-data; boundary="------WebKitFormBoundaryewozmkbxwbblilpm"
            var multipartContent = new MultipartFormDataContent(options.Boundary);
            multipartContent.Headers.ContentType.Parameters.First(o => o.Name == "boundary").Value = options.Boundary;

            FileStream fileStream = null;

            foreach (var c in options.Contents)
            {
                switch (c)
                {
                    case StringHttpContent x:
                        multipartContent.Add(new StringContent(x.Data, Encoding.UTF8, x.ContentType), x.Name);
                        break;

                    case RawHttpContent x:
                        var byteContent = new ByteArrayContent(x.Data);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue(x.ContentType);
                        multipartContent.Add(byteContent, x.Name);
                        break;

                    case FileHttpContent x:
                        lock (FileLocker.GetHandle(x.FileName))
                        {
                            if (data.Providers.Security.RestrictBlocksToCWD)
                                FileUtils.ThrowIfNotInCWD(x.FileName);

                            fileStream = new FileStream(x.FileName, FileMode.Open);
                            var fileContent = CreateFileContent(fileStream, x.Name, Path.GetFileName(x.FileName), x.ContentType);
                            multipartContent.Add(fileContent, x.Name);
                        }
                        break;
                }
            }

            using var request = new HttpRequestMessage
            {
                Method = new System.Net.Http.HttpMethod(options.Method.ToString()),
                RequestUri = new Uri(options.Url),
                Version = Version.Parse(options.HttpVersion),
                Content = multipartContent
            };

            foreach (var header in options.CustomHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            data.Logger.LogHeader();
            LogHttpRequestData(data, request, SerializeMultipart(options.Boundary, options.Contents), options.Boundary);

            try
            {
                Activity.Current = null;
                using var timeoutCts = new CancellationTokenSource(options.TimeoutMilliseconds);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(data.CancellationToken, timeoutCts.Token);
                using var response = await client.SendAsync(request, linkedCts.Token).ConfigureAwait(false);

                await LogHttpResponseData(data, response, cookieContainer, options).ConfigureAwait(false);
            }
            finally
            {
                if (fileStream != null)
                    await fileStream.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static void LogHttpRequestData(BotData data, HttpRequestMessage request, string content = null, string boundary = null)
        {
            using var writer = new StringWriter();

            // Log the method, uri and http version
            writer.WriteLine($"{request.Method.Method} {request.RequestUri.PathAndQuery} HTTP/{request.Version.Major}.{request.Version.Minor}");

            // Log the headers
            writer.WriteLine($"Host: {request.RequestUri.Host}");

            foreach (var header in request.Headers)
            {
                var separator = commaHeaders.Contains(header.Key) ? ", " : " ";
                writer.WriteLine($"{header.Key}: {string.Join(separator, header.Value)}");
            }

            // Log the cookie header
            var cookies = data.COOKIES.Select(c => $"{c.Key}={c.Value}");

            if (cookies.Any())
                writer.WriteLine($"Cookie: {string.Join("; ", cookies)}");

            if (request.Content != null && content != null)
            {
                switch (request.Content)
                {
                    case StringContent x:
                        writer.WriteLine($"Content-Type: {x.Headers.ContentType}");
                        writer.WriteLine($"Content-Length: {x.Headers.ContentLength}");
                        writer.WriteLine();
                        writer.WriteLine(content);
                        break;

                    case ByteArrayContent x:
                        writer.WriteLine($"Content-Type: {x.Headers.ContentType}");
                        writer.WriteLine($"Content-Length: {x.Headers.ContentLength}");
                        writer.WriteLine();
                        writer.WriteLine(content);
                        break;

                    case MultipartFormDataContent x:
                        writer.WriteLine($"Content-Type: multipart/form-data; boundary=\"{boundary}\"");
                        writer.WriteLine($"Content-Length: (not calculated)");
                        writer.WriteLine();
                        writer.WriteLine(content);
                        break;
                }
            }

            data.Logger.Log(writer.ToString(), LogColors.NonPhotoBlue);
        }

        private static async Task LogHttpResponseData(BotData data, HttpResponseMessage response,
            CookieContainer cookieContainer, Options.HttpRequestOptions requestOptions)
        {
            // Try to read the raw source for Content-Length calculation
            try
            {
                data.RAWSOURCE = await response.Content.ReadAsByteArrayAsync(data.CancellationToken).ConfigureAwait(false);
            }
            catch (NullReferenceException)
            {
                // Thrown when there is no content (204) or we decided to not read it
                data.RAWSOURCE = Array.Empty<byte>();
            }

            // Address
            data.ADDRESS = response.RequestMessage.RequestUri.AbsoluteUri;
            data.Logger.Log($"Address: {data.ADDRESS}", LogColors.DodgerBlue);

            // Response code
            data.RESPONSECODE = (int)response.StatusCode;
            data.Logger.Log($"Response code: {data.RESPONSECODE}", LogColors.Citrine);

            // Headers
            static string GetHeaderValue(KeyValuePair<string, IEnumerable<string>> header)
            {
                var separator = commaHeaders.Contains(header.Key) ? ", " : " ";
                return string.Join(separator, header.Value);
            }

            data.HEADERS = response.Headers.ToDictionary(h => h.Key, GetHeaderValue);

            if (response.Content != null)
            {
                foreach (var header in response.Content.Headers)
                {
                    data.HEADERS[header.Key] = GetHeaderValue(header);
                }
            }

            if (!data.HEADERS.ContainsKey("Content-Length"))
                data.HEADERS["Content-Length"] = data.RAWSOURCE.Length.ToString();

            data.Logger.Log("Received Headers:", LogColors.MediumPurple);
            data.Logger.Log(data.HEADERS.Select(h => $"{h.Key}: {h.Value}"), LogColors.Violet);

            // Cookies
            var cookies = Http.GetAllCookies(cookieContainer);
            data.COOKIES.Clear();
            foreach (Cookie cookie in cookies)
            {
                data.COOKIES[cookie.Name] = cookie.Value;
            }

            data.Logger.Log("Received Cookies:", LogColors.MikadoYellow);
            data.Logger.Log(data.COOKIES.Select(h => $"{h.Key}: {h.Value}"), LogColors.Khaki);

            // Decode brotli if still compressed
            if (data.HEADERS.ContainsKey("Content-Encoding") && data.HEADERS["Content-Encoding"].Contains("br"))
            {
                try
                {
                    using var inputStream = new MemoryStream(data.RAWSOURCE);
                    using var outputStream = new MemoryStream();
                    await using var brotli = new BrotliStream(inputStream, CompressionMode.Decompress, false);
                    await brotli.CopyToAsync(outputStream);
                    data.RAWSOURCE = outputStream.ToArray();
                }
                catch
                {
                    data.Logger.Log("[WARNING] Tried to decompress brotli but failed", LogColors.DarkOrange);
                }
            }

            // Unzip the GZipped content if still gzipped (after Content-Length calculation)
            if (data.RAWSOURCE.Length > 1 && data.RAWSOURCE[0] == 0x1F && data.RAWSOURCE[1] == 0x8B)
            {
                try
                {
                    data.RAWSOURCE = GZip.Unzip(data.RAWSOURCE);
                }
                catch
                {
                    data.Logger.Log("[WARNING] Tried to decompress gzip but failed", LogColors.DarkOrange);
                }
            }

            // Source
            if (!string.IsNullOrWhiteSpace(requestOptions.CodePagesEncoding))
            {
                data.SOURCE = CodePagesEncodingProvider.Instance
                    .GetEncoding(requestOptions.CodePagesEncoding).GetString(data.RAWSOURCE);
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
}
