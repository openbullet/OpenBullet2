namespace RuriLib.Functions.Http.Options
{
    public class RawHttpRequestOptions : HttpRequestOptions
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}
