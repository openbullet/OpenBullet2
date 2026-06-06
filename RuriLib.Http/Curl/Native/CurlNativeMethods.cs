using System;
using System.Runtime.CompilerServices;
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

    [LibraryImport(CurlLibrary, EntryPoint = "curl_global_init")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlCode GlobalInit(long flags);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_easy_init")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial nint EasyInit();

    [LibraryImport(CurlLibrary, EntryPoint = "curl_easy_cleanup")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void EasyCleanup(nint handle);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_easy_perform")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlCode EasyPerform(nint handle);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_multi_init")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial nint MultiInit();

    [LibraryImport(CurlLibrary, EntryPoint = "curl_multi_cleanup")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlMultiCode MultiCleanup(nint multiHandle);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_multi_add_handle")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlMultiCode MultiAddHandle(nint multiHandle, nint easyHandle);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_multi_remove_handle")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlMultiCode MultiRemoveHandle(nint multiHandle, nint easyHandle);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_multi_perform")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlMultiCode MultiPerform(nint multiHandle, out int runningHandles);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_multi_poll")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlMultiCode MultiPoll(nint multiHandle, nint extraFds, uint extraNfds,
        int timeoutMilliseconds, out int numFds);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_multi_wakeup")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlMultiCode MultiWakeup(nint multiHandle);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_multi_info_read")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial nint MultiInfoRead(nint multiHandle, out int messagesInQueue);

    // curl_easy_getinfo is variadic in C. This overload covers long outputs.
    [LibraryImport(CurlLibrary, EntryPoint = "curl_easy_getinfo")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlCode EasyGetInfo(nint handle, CurlInfo info, out long value);

    // curl_easy_setopt is variadic in C. These overloads cover the argument
    // shapes used by this handler; selecting the right CurlOption value matters.
    [LibraryImport(CurlLibrary, EntryPoint = "curl_easy_setopt")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlCode EasySetOpt(nint handle, CurlOption option, long value);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_easy_setopt")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlCode EasySetOpt(nint handle, CurlOption option, nint value);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_easy_impersonate")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial CurlCode EasyImpersonate(nint handle, nint target, int defaultHeaders);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_easy_strerror")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial nint EasyStrError(CurlCode code);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_multi_strerror")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial nint MultiStrError(CurlMultiCode code);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_slist_append")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial nint SlistAppend(nint list, nint value);

    [LibraryImport(CurlLibrary, EntryPoint = "curl_slist_free_all")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void SlistFreeAll(nint list);

    public static string GetErrorString(CurlCode code)
    {
        var ptr = EasyStrError(code);
        return ptr == 0 ? code.ToString() : Marshal.PtrToStringUTF8(ptr) ?? code.ToString();
    }

    public static string GetMultiErrorString(CurlMultiCode code)
    {
        var ptr = MultiStrError(code);
        return ptr == 0 ? code.ToString() : Marshal.PtrToStringUTF8(ptr) ?? code.ToString();
    }

    public static void ThrowIfError(CurlCode code, string operation)
    {
        if (code != CurlCode.Ok)
        {
            throw new InvalidOperationException($"{operation} failed with code {(int)code} ({GetErrorString(code)})");
        }
    }

    public static void ThrowIfMultiError(CurlMultiCode code, string operation)
    {
        if (code != CurlMultiCode.Ok)
        {
            throw new InvalidOperationException(
                $"{operation} failed with code {(int)code} ({GetMultiErrorString(code)})");
        }
    }
}
