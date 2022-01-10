using RuriLib.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Http.Models
{
    /// <summary>
    /// An HTTP request that can be sent using a <see cref="RLHttpClient"/>.
    /// </summary>
    public class HttpRequest : IDisposable
    {
        /// <summary>
        /// Whether to write the absolute URI in the first line of the request instead of
        /// the relative path (e.g. https://example.com/abc instead of /abc)
        /// </summary>
        public bool AbsoluteUriInFirstLine { get; set; } = false;

        /// <summary>
        /// The HTTP version to use.
        /// </summary>
        public Version Version { get; set; } = new(1, 1);

        /// <summary>
        /// The HTTP method to use.
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.Get;

        /// <summary>
        /// The URI of the remote resource.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// The cookies to send inside the Cookie header of this request.
        /// </summary>
        public Dictionary<string, string> Cookies { get; set; } = new();

        /// <summary>
        /// The headers of this request.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// The content of this request.
        /// </summary>
        public HttpContent Content { get; set; }

        /// <summary>
        /// Gets the raw bytes that will be sent on the network stream.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel the operation</param>
        public async Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream();
            ms.Write(Encoding.ASCII.GetBytes(BuildFirstLine()));
            ms.Write(Encoding.ASCII.GetBytes(BuildHeaders()));

            if (Content != null)
            {
                ms.Write(await Content.ReadAsByteArrayAsync(cancellationToken));
            }

            return ms.ToArray();
        }

        private static readonly string newLine = "\r\n";

        /// <summary>
        /// Safely adds a header to the dictionary.
        /// </summary>
        public void AddHeader(string name, string value)
        {
            // Make sure Host is written properly otherwise it won't get picked up below
            if (name.Equals("Host", StringComparison.OrdinalIgnoreCase))
            {
                Headers["Host"] = value;
            }
            else
            {
                Headers[name] = value;
            }
        }

        // Builds the first line, for example
        // GET /resource HTTP/1.1
        private string BuildFirstLine()
        {
            if (Version >= new Version(2, 0))
                throw new Exception($"HTTP/{Version.Major}.{Version.Minor} not supported yet");

            return $"{Method.Method} {(AbsoluteUriInFirstLine ? Uri.AbsoluteUri : Uri.PathAndQuery)} HTTP/{Version}{newLine}";
        }

        // Builds the headers, for example
        // Host: example.com
        // Connection: Close
        private string BuildHeaders()
        {
            // NOTE: Do not use AppendLine because it appends \n instead of \r\n
            // on Unix-like systems.
            var sb = new StringBuilder();
            var finalHeaders = new List<KeyValuePair<string, string>>();

            // Add the Host header if not already provided
            if (!HeaderExists("Host", out _))
            {
                finalHeaders.Add("Host", Uri.Host);
            }

            // If there is no Connection header, add it
            if (!HeaderExists("Connection", out var connectionHeaderName))
            {
                finalHeaders.Add("Connection", "Close");
            }

            // Add the non-content headers
            foreach (var header in Headers)
            {
                finalHeaders.Add(header);
            }

            // Add the Cookie header if not set manually and container not null
            if (!HeaderExists("Cookie", out _) && Cookies.Any())
            {
                var cookieBuilder = new StringBuilder();

                foreach (var cookie in Cookies)
                {
                    cookieBuilder
                        .Append($"{cookie.Key}={cookie.Value}; ");
                }

                // Remove the last ; and space if not empty
                if (cookieBuilder.Length > 2)
                {
                    cookieBuilder.Remove(cookieBuilder.Length - 2, 2);
                }

                finalHeaders.Add("Cookie", cookieBuilder);
            }

            // Add the content headers
            if (Content != null)
            {
                foreach (var header in Content.Headers)
                {
                    // If it was already set, skip
                    if (!HeaderExists(header.Key, out _))
                    {
                        finalHeaders.Add(header.Key, string.Join(' ', header.Value));
                    }
                }

                // Add the Content-Length header if not already present
                if (!finalHeaders.Any(h => h.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)))
                {
                    var contentLength = Content.Headers.ContentLength;

                    if (contentLength.HasValue && contentLength.Value > 0)
                    {
                        finalHeaders.Add("Content-Length", contentLength);
                    }
                }
            }

            // Write all non-empty headers to the StringBuilder
            foreach (var header in finalHeaders.Where(h => !string.IsNullOrEmpty(h.Value)))
            {
                sb
                    .Append(header.Key)
                    .Append(": ")
                    .Append(header.Value)
                    .Append(newLine);
            }

            // Write the final blank line after all headers
            sb.Append(newLine);

            return sb.ToString();
        }

        /// <summary>
        /// Checks whether a header that matches a given <paramref name="name"/> exists. If it exists,
        /// its original name will be written to <paramref name="actualName"/>.
        /// </summary>
        public bool HeaderExists(string name, out string actualName)
        {
            var key = Headers.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
            actualName = key;
            return key != null;
        }

        /// <inheritdoc/>
        public void Dispose() => Content?.Dispose();
    }
}
