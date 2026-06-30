using System;
using System.Collections.Generic;

namespace RuriLib.Http.Curl.Internal;

internal static class CurlHeaderFilters
{
    private static readonly HashSet<string> BrowserManagedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Accept",
        "Accept-Encoding",
        "Accept-Language",
        "Connection",
        "Host",
        "Origin",
        "Priority",
        "Sec-Ch-Ua",
        "Sec-Ch-Ua-Arch",
        "Sec-Ch-Ua-Bitness",
        "Sec-Ch-Ua-Full-Version",
        "Sec-Ch-Ua-Full-Version-List",
        "Sec-Ch-Ua-Mobile",
        "Sec-Ch-Ua-Model",
        "Sec-Ch-Ua-Platform",
        "Sec-Ch-Ua-Platform-Version",
        "Sec-Fetch-Dest",
        "Sec-Fetch-Mode",
        "Sec-Fetch-Site",
        "Sec-Fetch-User",
        "Upgrade-Insecure-Requests",
        "User-Agent"
    };

    private static readonly HashSet<string> AlwaysManagedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Content-Length",
        "Transfer-Encoding"
    };

    public static bool ShouldSkipRequestHeader(string name, bool useBrowserHeaders)
        => AlwaysManagedHeaders.Contains(name)
        || (useBrowserHeaders && BrowserManagedHeaders.Contains(name));
}
