using System;
using System.Runtime.InteropServices;

namespace RuriLib.Http.Curl.Native;

// Matches CURLOPT_WRITEFUNCTION and CURLOPT_HEADERFUNCTION. libcurl passes a
// buffer pointer plus size * nmemb bytes and expects that exact byte count back.
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate nuint CurlWriteCallback(nint buffer, nuint size, nuint nmemb, nint userData);

// Matches CURLOPT_XFERINFOFUNCTION. Returning non-zero aborts the transfer, so
// the handler uses it to translate cancellation tokens into a libcurl abort.
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate int CurlProgressCallback(nint clientp, long downloadTotal, long downloadNow, long uploadTotal, long uploadNow);

// Matches CURLOPT_DEBUGFUNCTION. The return value is ignored by libcurl.
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate int CurlDebugCallback(nint handle, CurlDebugInfoType type, nint data, nuint size, nint userData);

internal enum CurlDebugInfoType
{
    Text = 0,
    HeaderIn = 1,
    HeaderOut = 2,
    DataIn = 3,
    DataOut = 4,
    SslDataIn = 5,
    SslDataOut = 6
}
