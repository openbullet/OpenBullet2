using RuriLib.Models.Bots;
using System.Collections.Generic;

namespace RuriLib.Functions.Http.Options
{
    public class HttpRequestOptions
    {
        public string Url { get; set; }
        public HttpMethod Method { get; set; }
        public bool AutoRedirect { get; set; }
        public int MaxNumberOfRedirects { get; set; }
        public SecurityProtocol SecurityProtocol { get; set; }
        public Dictionary<string, string> CustomCookies { get; set; }
        public Dictionary<string, string> CustomHeaders { get; set; }
        public int TimeoutMilliseconds { get; set; }
        public string HttpVersion { get; set; }
        public bool UseCustomCipherSuites { get; set; }
        public List<string> CustomCipherSuites { get; set; }
        public bool AlwaysSendContent { get; set; }
    }
}
