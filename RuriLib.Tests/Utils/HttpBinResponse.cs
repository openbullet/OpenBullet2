using System.Collections.Generic;

namespace RuriLib.Tests.Utils
{
    /// <summary>
    /// The deserialized data of a httpbin.org/anything response.
    /// </summary>
    public class HttpBinResponse
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, string> Form { get; set; }
        public Dictionary<string, string> Files { get; set; }
    }
}
