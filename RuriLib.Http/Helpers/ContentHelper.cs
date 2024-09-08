using System;
using System.Linq;

namespace RuriLib.Http.Helpers;

internal static class ContentHelper
{
    //https://github.com/dotnet/corefx/blob/3e72ee5971db5d0bd46606fa672969adde29e307/src/System.Net.Http/src/System/Net/Http/Headers/KnownHeaders.cs
    private static readonly string[] _contentHeaders =
    [
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
    ];

    public static bool IsContentHeader(string name) => _contentHeaders.Any(h => h.Equals(name, StringComparison.OrdinalIgnoreCase));
}
