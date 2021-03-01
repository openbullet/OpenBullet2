using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace RuriLib.Http.Models
{
    public class HttpResponse : IDisposable
    {
        /// <summary>
        /// The request that retrieved this response.
        /// </summary>
        public HttpRequest Request { get; set; }

        public Version Version { get; set; } = new(1, 1);
        public HttpStatusCode StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);
        public HttpContent Content { get; set; }

        public void Dispose() => Content?.Dispose();
    }
}
