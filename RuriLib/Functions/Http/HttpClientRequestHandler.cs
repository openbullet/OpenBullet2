using RuriLib.Functions.Http.Options;
using RuriLib.Models.Bots;
using System;
using System.Threading.Tasks;

namespace RuriLib.Functions.Http
{
    internal class HttpClientRequestHandler : IHttpRequestHandler
    {
        public async Task HttpRequestStandard(BotData data, StandardHttpRequestOptions options)
        {
            var clientOptions = GetClientOptions(data, options);
            using var client = HttpFactory.GetRLHttpClient(data.UseProxy ? data.Proxy : null, clientOptions);

            foreach (var cookie in options.CustomCookies)
                data.COOKIES[cookie.Key] = cookie.Value;

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
                        .Select(s => Uri.EscapeDataString(s)))
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
                LogHttpRequestData(data, request);
                throw;
            }
            finally
            {
                request.Dispose();
                client.Dispose();
            }
        }

        public Task HttpRequestRaw(BotData data, RawHttpRequestOptions options)
            => throw new NotImplementedException();

        public Task HttpRequestBasicAuth(BotData data, BasicAuthHttpRequestOptions options)
            => throw new NotImplementedException();
        public Task HttpRequestMultipart(BotData data, MultipartHttpRequestOptions options)
            => throw new NotImplementedException();
    }
}
