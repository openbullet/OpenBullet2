using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using System.Collections.Generic;

namespace RuriLib.Functions.Http.Options
{
    public class MultipartHttpRequestOptions : HttpRequestOptions
    {
        public string Boundary { get; set; } = string.Empty;
        public List<MyHttpContent> Contents { get; set; } = new();
    }
}
