using System;

namespace RuriLib.Functions.Http.Options
{
    public class RawHttpRequestOptions : HttpRequestOptions
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
    }
}
