using System.Collections.Generic;

namespace RuriLib.Functions.Http.Options
{
    public class HttpRequestOptions
    {
        public string Url { get; set; } = string.Empty;
        public HttpMethod Method { get; set; } = HttpMethod.GET;
        public bool AutoRedirect { get; set; } = true;
        public int MaxNumberOfRedirects { get; set; } = 8;
        public bool AbsoluteUriInFirstLine { get; set; } = false;
        public HttpLibrary HttpLibrary { get; set; } = HttpLibrary.RuriLibHttp;
        public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SystemDefault;
        public Dictionary<string, string> CustomCookies { get; set; } = new();
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
        public int TimeoutMilliseconds { get; set; } = 10000;
        public string HttpVersion { get; set; } = "1.1";
        public bool UseCustomCipherSuites { get; set; } = false;
        public List<string> CustomCipherSuites { get; set; } = new();
        public string CodePagesEncoding { get; set; } = string.Empty;
        public bool AlwaysSendContent { get; set; } = false;
        public bool DecodeHtml { get; set; } = false;
        public bool ReadResponseContent { get; set; } = true;
    }

    public enum HttpLibrary
    {
        SystemNet,
        RuriLibHttp
    }
}
