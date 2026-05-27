using RuriLib.Http.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace RuriLib.Http.Curl.Internal;

internal static class CurlResponseMessageBuilder
{
    public static HttpResponseMessage Build(CurlResponseData data, HttpRequestMessage request,
        CurlImpersonateHandlerOptions options)
    {
        var headers = ParseHeaders(data.Headers);
        var body = options.AutomaticDecompression && options.ReadResponseContent
            ? TryDecompress(data.Body, headers)
            : data.Body;

        var response = new HttpResponseMessage((HttpStatusCode)data.StatusCode)
        {
            RequestMessage = request,
            Content = new ByteArrayContent(options.ReadResponseContent ? body : [])
        };

        foreach (var header in headers)
        {
            if (ContentHelper.IsContentHeader(header.Key))
            {
                response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            else
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return response;
    }

    private static List<KeyValuePair<string, string>> ParseHeaders(IEnumerable<string> rawHeaders)
    {
        var headers = new List<KeyValuePair<string, string>>();

        foreach (var rawHeader in rawHeaders)
        {
            var separator = rawHeader.IndexOf(':');

            if (separator <= 0)
            {
                continue;
            }

            headers.Add(new KeyValuePair<string, string>(
                rawHeader[..separator].Trim(),
                rawHeader[(separator + 1)..].Trim()));
        }

        return headers;
    }

    private static byte[] TryDecompress(byte[] body, List<KeyValuePair<string, string>> headers)
    {
        if (body.Length == 0)
        {
            return body;
        }

        var contentEncodings = headers
            .Where(h => h.Key.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase))
            .Select(h => h.Value)
            .ToArray();

        if (contentEncodings.Length == 0)
        {
            return body;
        }

        try
        {
            using var input = new MemoryStream(body);
            using var decoded = ContentEncodingHelper.GetDecodedStream(input, contentEncodings);
            using var output = new MemoryStream();
            decoded.CopyTo(output);
            return output.ToArray();
        }
        catch
        {
            return body;
        }
    }
}
