using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace RuriLib.Http.Models
{
    /// <summary>
    /// An HTTP response obtained with a <see cref="RLHttpClient"/>.
    /// </summary>
    public class HttpResponse : IDisposable
    {
        /// <summary>
        /// The request that retrieved this response.
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// The HTTP version.
        /// </summary>
        public Version Version { get; set; } = new(1, 1);

        /// <summary>
        /// The status code of the response.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// The headers of the response.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The content of the response.
        /// </summary>
        public HttpContent Content { get; set; }

        /// <inheritdoc/>
        public void Dispose() => Content?.Dispose();
    }
}
