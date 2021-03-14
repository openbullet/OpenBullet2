namespace RuriLib.Functions.Http.Options
{
    public class StandardHttpRequestOptions : HttpRequestOptions
    {
        public string Content { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public bool UrlEncodeContent { get; set; } = false;
    }
}
