using System.Collections.Generic;

namespace RuriLib.Tests.Utils;

/// <summary>
/// The deserialized data of a httpbin.org/anything response.
/// </summary>
public class HttpBinResponse
{
    public string Url { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = [];
    public Dictionary<string, string> Form { get; set; } = [];
    public Dictionary<string, string> Files { get; set; } = [];
}
