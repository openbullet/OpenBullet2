using System;
using System.Runtime.InteropServices;

namespace RuriLib.Http.Curl.Native;

internal static partial class CurlNativeMethods
{
    // Logical library name. CurlNativeLibraryResolver maps this to the actual
    // platform file name and RID-specific publish folder.
    public const string CurlLibrary = "curl-impersonate";

    static CurlNativeMethods()
    {
        CurlNativeLibraryResolver.Initialize();
    }

    [DllImport(CurlLibrary, EntryPoint = "curl_global_init", CallingConvention = CallingConvention.Cdecl)]
    public static extern CurlCode GlobalInit(long flags);

    [DllImport(CurlLibrary, EntryPoint = "curl_easy_init", CallingConvention = CallingConvention.Cdecl)]
    public static extern nint EasyInit();

    [DllImport(CurlLibrary, EntryPoint = "curl_easy_cleanup", CallingConvention = CallingConvention.Cdecl)]
    public static extern void EasyCleanup(nint handle);

    [DllImport(CurlLibrary, EntryPoint = "curl_easy_perform", CallingConvention = CallingConvention.Cdecl)]
    public static extern CurlCode EasyPerform(nint handle);

    // curl_easy_getinfo is variadic in C. This overload covers long outputs.
    [DllImport(CurlLibrary, EntryPoint = "curl_easy_getinfo", CallingConvention = CallingConvention.Cdecl)]
    public static extern CurlCode EasyGetInfo(nint handle, CurlInfo info, out long value);

    // curl_easy_setopt is variadic in C. These overloads cover the argument
    // shapes used by this handler; selecting the right CurlOption value matters.
    [DllImport(CurlLibrary, EntryPoint = "curl_easy_setopt", CallingConvention = CallingConvention.Cdecl)]
    public static extern CurlCode EasySetOpt(nint handle, CurlOption option, long value);

    [DllImport(CurlLibrary, EntryPoint = "curl_easy_setopt", CallingConvention = CallingConvention.Cdecl)]
    public static extern CurlCode EasySetOpt(nint handle, CurlOption option, nint value);

    [DllImport(CurlLibrary, EntryPoint = "curl_easy_impersonate", CallingConvention = CallingConvention.Cdecl)]
    public static extern CurlCode EasyImpersonate(nint handle, nint target, int defaultHeaders);

    [DllImport(CurlLibrary, EntryPoint = "curl_easy_strerror", CallingConvention = CallingConvention.Cdecl)]
    public static extern nint EasyStrError(CurlCode code);

    [DllImport(CurlLibrary, EntryPoint = "curl_slist_append", CallingConvention = CallingConvention.Cdecl)]
    public static extern nint SlistAppend(nint list, nint value);

    [DllImport(CurlLibrary, EntryPoint = "curl_slist_free_all", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SlistFreeAll(nint list);

    public static string GetErrorString(CurlCode code)
    {
        var ptr = EasyStrError(code);
        return ptr == 0 ? code.ToString() : Marshal.PtrToStringUTF8(ptr) ?? code.ToString();
    }

    public static void ThrowIfError(CurlCode code, string operation)
    {
        if (code != CurlCode.Ok)
        {
            throw new InvalidOperationException($"{operation} failed with code {(int)code} ({GetErrorString(code)})");
        }
    }
}
