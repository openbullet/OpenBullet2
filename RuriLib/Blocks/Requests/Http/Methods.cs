using RuriLib.Attributes;
using RuriLib.Extensions;
using RuriLib.Functions.Files;
using RuriLib.Functions.Http;
using RuriLib.Helpers;
using RuriLib.Logging;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Requests.Http
{
    [BlockCategory("Http", "Blocks for performing Http requests", "#32cd32")]
    public static class Methods
    {
        private static readonly string[] commaHeaders = new[] { "Accept", "Accept-Encoding" };

        // STANDARD REQUESTS
        // This method is accessed via a custom descriptor so it must not be an auto block
        public static async Task HttpRequestStandard(BotData data, string url, RuriLib.Functions.Http.HttpMethod method,
            bool autoRedirect, SecurityProtocol securityProtocol, string content, string contentType, Dictionary<string, string> customCookies,
            Dictionary<string, string> customHeaders, int timeoutMilliseconds, string httpVersion, bool useCustomCipherSuites,
            List<string> customCipherSuites)
        {
            foreach (var cookie in customCookies)
                data.CookieContainer.Add(new Uri(url), new System.Net.Cookie(cookie.Key, cookie.Value));

            var options = new HttpHandlerOptions
            {
                Cookies = data.CookieContainer,
                ConnectTimeout = data.Providers.ProxySettings.ConnectTimeout,
                ReadWriteTimeout = data.Providers.ProxySettings.ReadWriteTimeout,
                AutoRedirect = autoRedirect,
                SecurityProtocol = securityProtocol,
                UseCustomCipherSuites = useCustomCipherSuites,
                CustomCipherSuites = ParseCipherSuites(customCipherSuites)
            };

            using var handler = HttpHandlerFactory.GetHandler(data.Proxy, options);

            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds) };

            using var request = new HttpRequestMessage 
            {
                Method = new System.Net.Http.HttpMethod(method.ToString()),
                RequestUri = new Uri(url),
                Version = Version.Parse(httpVersion),
                Content = new StringContent(content.Unescape())
            };

            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

            foreach (var header in customHeaders)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            data.Logger.LogHeader();
            LogHttpRequestData(data, request);
            
            try
            {
                Activity.Current = null;
                var response = await client.SendAsync(request, data.CancellationToken);
                await LogHttpResponseData(data, response, request);
            }
            finally
            {
                request.Dispose();
                client.Dispose();
            }
        }

        // RAW REQUESTS
        // This method is accessed via a custom descriptor so it must not be an auto block
        public static async Task HttpRequestRaw(BotData data, string url, RuriLib.Functions.Http.HttpMethod method,
            bool autoRedirect, SecurityProtocol securityProtocol, byte[] content, string contentType,
            Dictionary<string, string> customCookies, Dictionary<string, string> customHeaders,
            int timeoutMilliseconds, string httpVersion, bool useCustomCipherSuites, List<string> customCipherSuites)
        {
            foreach (var cookie in customCookies)
                data.CookieContainer.Add(new Uri(url), new System.Net.Cookie(cookie.Key, cookie.Value));

            var options = new HttpHandlerOptions
            {
                Cookies = data.CookieContainer,
                ConnectTimeout = data.Providers.ProxySettings.ConnectTimeout,
                ReadWriteTimeout = data.Providers.ProxySettings.ReadWriteTimeout,
                AutoRedirect = autoRedirect,
                SecurityProtocol = securityProtocol,
                UseCustomCipherSuites = useCustomCipherSuites,
                CustomCipherSuites = ParseCipherSuites(customCipherSuites)
            };

            using var handler = HttpHandlerFactory.GetHandler(data.Proxy, options);

            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds) };

            using var request = new HttpRequestMessage
            {
                Method = new System.Net.Http.HttpMethod(method.ToString()),
                RequestUri = new Uri(url),
                Version = Version.Parse(httpVersion),
                Content = new ByteArrayContent(content)
            };

            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

            foreach (var header in customHeaders)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            data.Logger.LogHeader();
            LogHttpRequestData(data, request);

            try
            {
                Activity.Current = null;
                var response = await client.SendAsync(request, data.CancellationToken);
                await LogHttpResponseData(data, response, request);
            }
            finally
            {
                request.Dispose();
                client.Dispose();
            }
        }

        // BASIC AUTH
        // This method is accessed via a custom descriptor so it must not be an auto block
        public static async Task HttpRequestBasicAuth(BotData data, string url, bool autoRedirect, 
            SecurityProtocol securityProtocol, string username, string password, Dictionary<string, string> customCookies,
            Dictionary<string, string> customHeaders, int timeoutMilliseconds, string httpVersion, bool useCustomCipherSuites,
            List<string> customCipherSuites)
        {
            foreach (var cookie in customCookies)
                data.CookieContainer.Add(new Uri(url), new System.Net.Cookie(cookie.Key, cookie.Value));

            var options = new HttpHandlerOptions
            {
                Cookies = data.CookieContainer,
                ConnectTimeout = data.Providers.ProxySettings.ConnectTimeout,
                ReadWriteTimeout = data.Providers.ProxySettings.ReadWriteTimeout,
                AutoRedirect = autoRedirect,
                SecurityProtocol = securityProtocol,
                UseCustomCipherSuites = useCustomCipherSuites,
                CustomCipherSuites = ParseCipherSuites(customCipherSuites)
            };

            using var handler = HttpHandlerFactory.GetHandler(data.Proxy, options);

            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds) };

            using var request = new HttpRequestMessage
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri(url),
                Version = Version.Parse(httpVersion)
            };

            foreach (var header in customHeaders)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            // Add the basic auth header
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

            data.Logger.LogHeader();
            LogHttpRequestData(data, request);

            try
            {
                Activity.Current = null;
                var response = await client.SendAsync(request, data.CancellationToken);
                await LogHttpResponseData(data, response, request);
            }
            finally
            {
                request.Dispose();
                client.Dispose();
            }
        }

        // MULTIPART
        // This method is accessed via a custom descriptor so it must not be an auto block
        public static async Task HttpRequestMultipart(BotData data, string url, RuriLib.Functions.Http.HttpMethod method, bool autoRedirect,
            SecurityProtocol securityProtocol, string boundary, List<MyHttpContent> content, 
            Dictionary<string, string> customCookies, Dictionary<string, string> customHeaders, int timeoutMilliseconds, string httpVersion,
            bool useCustomCipherSuites, List<string> customCipherSuites)
        {
            foreach (var cookie in customCookies)
                data.CookieContainer.Add(new Uri(url), new System.Net.Cookie(cookie.Key, cookie.Value));

            var options = new HttpHandlerOptions
            {
                Cookies = data.CookieContainer,
                ConnectTimeout = data.Providers.ProxySettings.ConnectTimeout,
                ReadWriteTimeout = data.Providers.ProxySettings.ReadWriteTimeout,
                AutoRedirect = autoRedirect,
                SecurityProtocol = securityProtocol,
                UseCustomCipherSuites = useCustomCipherSuites,
                CustomCipherSuites = ParseCipherSuites(customCipherSuites)
            };
            
            using var handler = HttpHandlerFactory.GetHandler(data.Proxy, options);

            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds) };

            if (string.IsNullOrWhiteSpace(boundary))
                boundary = GenerateMultipartBoundary();
            
            var multipartContent = new MultipartFormDataContent(boundary);

            FileStream fileStream = null;

            foreach (var c in content)
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
                            fileStream = new FileStream(x.FileName, FileMode.Open);
                            var fileContent = CreateFileContent(fileStream, x.Name, Path.GetFileName(x.FileName), x.ContentType);
                            multipartContent.Add(fileContent, x.Name);
                        }
                        break;
                }
            }

            using var request = new HttpRequestMessage
            {
                Method = new System.Net.Http.HttpMethod(method.ToString()),
                RequestUri = new Uri(url),
                Version = Version.Parse(httpVersion),
                Content = multipartContent
            };

            foreach (var header in customHeaders)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            data.Logger.LogHeader();
            LogHttpRequestData(data, request, boundary, content);

            try
            {
                Activity.Current = null;
                var response = await client.SendAsync(request, data.CancellationToken);
                await LogHttpResponseData(data, response, request);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Dispose();

                request.Dispose();
                client.Dispose();
            }
        }

        private static void LogHttpRequestData(BotData data, HttpRequestMessage request,
            string boundary = null, List<MyHttpContent> multipartContents = null)
        {
            using StringWriter writer = new StringWriter();

            // Log the method, uri and http version
            writer.WriteLine($"{request.Method.Method} {request.RequestUri.PathAndQuery} HTTP/{request.Version.Major}.{request.Version.Minor}");

            // Log the headers
            if (!request.Headers.Contains("Host"))
                writer.WriteLine($"Host: {request.RequestUri.Host}");

            foreach (var header in request.Headers)
            {
                var separator = commaHeaders.Contains(header.Key) ? ", " : " ";
                writer.WriteLine($"{header.Key}: {string.Join(separator, header.Value)}");
            }

            // Log the cookie header
            var cookies = RuriLib.Functions.Http.Http.GetAllCookies(data.CookieContainer)
                .Select(c => $"{c.Name}={c.Value}");
            
            if (cookies.Count() > 0)
                writer.WriteLine($"Cookie: {string.Join("; ", cookies)}");

            if (request.Content != null)
            {
                switch (request.Content)
                {
                    case StringContent x:
                        writer.WriteLine($"Content-Type: {x.Headers.ContentType}");
                        writer.WriteLine($"Content-Length: {x.Headers.ContentLength}");
                        writer.WriteLine();
                        writer.WriteLine(x.ReadAsStringAsync().Result);
                        break;

                    case ByteArrayContent x:
                        writer.WriteLine($"Content-Type: {x.Headers.ContentType}");
                        writer.WriteLine($"Content-Length: {x.Headers.ContentLength}");
                        writer.WriteLine();
                        writer.WriteLine(RuriLib.Functions.Conversion.HexConverter.ToHexString(x.ReadAsByteArrayAsync().Result));
                        break;

                    case MultipartFormDataContent x:
                        writer.WriteLine($"Content-Type: multipart/form-data; boundary=\"{boundary}\"");
                        var serializedMultipart = SerializeMultipart(boundary, multipartContents);
                        writer.WriteLine($"Content-Length: (not calculated)");
                        writer.WriteLine();
                        writer.WriteLine(serializedMultipart);
                        break;
                }
            }

            data.Logger.Log(writer.ToString(), LogColors.NonPhotoBlue);
        }

        private static string SerializeMultipart(string boundary, List<MyHttpContent> contents)
        {
            using StringWriter writer = new StringWriter();

            foreach (var content in contents)
            {
                writer.WriteLine(boundary);

                switch (content)
                {
                    case StringHttpContent x:
                        writer.WriteLine($"Content-Disposition: form-data; name={x.Name}");
                        writer.WriteLine($"Content-Type: {GetMediaHeaderString(x.ContentType)}");
                        writer.WriteLine();
                        writer.WriteLine(x.Data);
                        break;

                    case RawHttpContent x:
                        writer.WriteLine($"Content-Disposition: form-data; name={x.Name}");
                        writer.WriteLine($"Content-Type: {GetMediaHeaderString(x.ContentType)}");
                        writer.WriteLine();
                        writer.WriteLine(Encoding.UTF8.GetString(x.Data));
                        break;

                    case FileHttpContent x:
                        writer.WriteLine($"Content-Disposition: form-data; name=\"{x.Name}\"; filename=\"{Path.GetFileName(x.FileName)}\"");
                        writer.WriteLine($"Content-Type: {GetMediaHeaderString(x.ContentType)}");
                        writer.WriteLine();
                        writer.WriteLine("[FILE CONTENTS NOT LOGGED]");
                        break;
                }
            }
            
            writer.WriteLine(boundary);

            return writer.ToString();
        }

        private static async Task LogHttpResponseData(BotData data, HttpResponseMessage response, HttpRequestMessage request)
        {
            // Read the raw source for Content-Length calculation
            data.RAWSOURCE = await response.Content.ReadAsByteArrayAsync(data.CancellationToken);

            // Address
            var uri = response.RequestMessage.RequestUri;
            if (!uri.IsAbsoluteUri)
                uri = new Uri(request.RequestUri, uri);
            data.ADDRESS = response.RequestMessage.RequestUri.AbsoluteUri;
            data.Logger.Log($"Address: {data.ADDRESS}", LogColors.DodgerBlue);

            // Response code
            data.RESPONSECODE = (int)response.StatusCode;
            data.Logger.Log($"Response code: {data.RESPONSECODE}", LogColors.Citrine);

            // Headers
            data.HEADERS = response.Headers.ToDictionary(h => h.Key, h => h.Value.First());
            if (!data.HEADERS.ContainsKey("Content-Length"))
                data.HEADERS["Content-Length"] = data.RAWSOURCE.Length.ToString();
            data.Logger.Log("Received Headers:", LogColors.MediumPurple);
            data.Logger.Log(data.HEADERS.Select(h => $"{h.Key}: {h.Value}"), LogColors.Violet);

            // Cookies
            data.COOKIES = RuriLib.Functions.Http.Http.GetAllCookies(data.CookieContainer).ToDictionary(c => c.Name, c => c.Value);
            data.Logger.Log("Received Cookies:", LogColors.MikadoYellow);
            data.Logger.Log(data.COOKIES.Select(h => $"{h.Key}: {h.Value}"), LogColors.Khaki);

            // Unzip the GZipped content if needed (after Content-Length calculation)
            if (response.RequestMessage.Headers.Contains("Accept-Encoding") && response.RequestMessage.Headers.AcceptEncoding.First().Value.Contains("gzip") &&
                response.Content.Headers.Contains("Content-Encoding") && response.Content.Headers.GetValues("Content-Encoding").First().Contains("gzip"))
                data.RAWSOURCE = GZip.Unzip(data.RAWSOURCE);

            // Source
            data.SOURCE = Encoding.UTF8.GetString(data.RAWSOURCE);
            data.Logger.Log("Received Payload:", LogColors.ForestGreen);
            data.Logger.Log(data.SOURCE, LogColors.GreenYellow, true);
        }

        /// <summary>
        /// Generates a random string to be used for boundary.
        /// </summary>
        private static string GenerateMultipartBoundary()
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < 16; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return $"------WebKitFormBoundary{builder.ToString().ToLower()}";
        }

        private static StreamContent CreateFileContent(Stream stream, string fieldName, string fileName, string contentType)
        {
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = $"\"{fieldName}\"",  
                FileName = $"\"{fileName}\""
            }; // the extra quotes are key here
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return fileContent;
        }

        private static string GetMediaHeaderString(string contentType)
            => new MediaTypeHeaderValue(contentType).ToString();

        private static TlsCipherSuite[] ParseCipherSuites(List<string> cipherSuites)
        {
            List<TlsCipherSuite> parsed = new List<TlsCipherSuite>();

            foreach (var suite in cipherSuites)
            {
                try
                {
                    parsed.Add(Enum.Parse<TlsCipherSuite>(suite));
                }
                catch
                {
                    throw new NotSupportedException($"Cipher suite not supported: {suite}");
                }
            }

            return parsed.ToArray();
        }
    }
}
