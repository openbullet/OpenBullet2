using System;

namespace RuriLib.Http.Curl;

/// <summary>
/// Exception thrown when curl-impersonate reports a transfer error.
/// </summary>
public sealed class CurlImpersonateException : Exception
{
    /// <summary>
    /// The native CURLcode returned by libcurl.
    /// </summary>
    public int CurlCode { get; }

    internal CurlImpersonateException(Native.CurlCode curlCode, string? message)
        : base(string.IsNullOrWhiteSpace(message)
            ? $"curl-impersonate failed with code {(int)curlCode} ({curlCode})"
            : $"curl-impersonate failed with code {(int)curlCode} ({curlCode}): {message}")
    {
        CurlCode = (int)curlCode;
    }
}
