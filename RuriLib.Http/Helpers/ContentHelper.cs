using System;
using System.Linq;

namespace RuriLib.Http.Helpers
{
    static internal class ContentHelper
    {
        //https://github.com/dotnet/corefx/blob/3e72ee5971db5d0bd46606fa672969adde29e307/src/System.Net.Http/src/System/Net/Http/Headers/KnownHeaders.cs
        private static readonly string[] contentHeaders = new []
        {
            "Last-Modified",
            "Expires",
            "Content-Type",
            "Content-Range",
            "Content-MD5",
            "Content-Location",
            "Content-Length",
            "Content-Language",
            "Content-Encoding",
            "Allow"
        };

        public static bool IsContentHeader(string name) => contentHeaders.Any(h => h.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
