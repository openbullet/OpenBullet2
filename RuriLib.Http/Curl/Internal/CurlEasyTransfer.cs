using RuriLib.Http.Curl.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Http.Curl.Internal;

internal sealed class CurlEasyTransfer : IDisposable
{
    private readonly nint handle;
    private readonly CurlRequestContext context;
    private readonly GCHandle contextHandle;
    // Keep callback delegates rooted for the whole easy handle lifetime. libcurl
    // stores only raw function pointers, so these must not be collected mid-transfer.
    private readonly CurlWriteCallback writeCallback;
    private readonly CurlWriteCallback headerCallback;
    private readonly CurlProgressCallback progressCallback;
    private readonly byte[] errorBuffer = new byte[256];
    private readonly GCHandle errorBufferHandle;
    // String options and curl_slist entries are borrowed by libcurl until cleanup.
    // Hold and free the unmanaged memory together with the easy handle.
    private readonly List<nint> nativeStrings = [];
    private GCHandle requestBodyHandle;
    private nint headerList;
    private bool disposed;

    public CurlEasyTransfer(CurlRequestContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));

        CurlGlobal.Initialize();
        handle = CurlNativeMethods.EasyInit();

        if (handle == 0)
        {
            throw new InvalidOperationException("curl_easy_init failed");
        }

        contextHandle = GCHandle.Alloc(context);
        errorBufferHandle = GCHandle.Alloc(errorBuffer, GCHandleType.Pinned);
        writeCallback = OnWrite;
        headerCallback = OnHeader;
        progressCallback = OnProgress;
    }

    public async Task ConfigureAsync(HttpRequestMessage request, CurlImpersonateHandlerOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(request.RequestUri);

        SetPointerOption(CurlOption.ErrorBuffer, errorBufferHandle.AddrOfPinnedObject());
        SetLongOption(CurlOption.NoSignal, 1);
        SetLongOption(CurlOption.NoProgress, 0);
        SetPointerOption(CurlOption.WriteFunction, Marshal.GetFunctionPointerForDelegate(writeCallback));
        SetPointerOption(CurlOption.WriteData, GCHandle.ToIntPtr(contextHandle));
        SetPointerOption(CurlOption.HeaderFunction, Marshal.GetFunctionPointerForDelegate(headerCallback));
        SetPointerOption(CurlOption.HeaderData, GCHandle.ToIntPtr(contextHandle));
        SetPointerOption(CurlOption.XferInfoFunction, Marshal.GetFunctionPointerForDelegate(progressCallback));
        SetPointerOption(CurlOption.XferInfoData, GCHandle.ToIntPtr(contextHandle));

        SetHttpVersionIfNeeded(request);
        Impersonate(options);

        SetStringOption(CurlOption.Url, request.RequestUri.AbsoluteUri);
        SetTimeouts(options);
        SetProxy(options);
        SetCertificateValidation(options);
        ConfigureMethod(request);
        ConfigureHeaders(request, options);

        if (request.Content is not null)
        {
            var body = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            ConfigureRequestBody(body);
        }
    }

    public CurlResponseData Perform(CancellationToken cancellationToken)
    {
        var result = CurlNativeMethods.EasyPerform(handle);

        if (result == CurlCode.Ok)
        {
            return context.BuildResponse();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        throw new CurlImpersonateException(result, GetErrorMessage());
    }

    private void SetHttpVersionIfNeeded(HttpRequestMessage request)
    {
        if (request.Version <= new Version(1, 1))
        {
            return;
        }

        var curlVersion = request.Version.Major switch
        {
            2 => CurlHttpVersion.Version2Tls,
            3 => CurlHttpVersion.Version30,
            _ => CurlHttpVersion.None
        };

        if (curlVersion != CurlHttpVersion.None)
        {
            SetLongOption(CurlOption.HttpVersion, (long)curlVersion);
        }
    }

    private void Impersonate(CurlImpersonateHandlerOptions options)
    {
        using var target = new NativeUtf8String(options.BrowserProfile.ToCurlTarget());
        // curl_easy_impersonate applies the TLS/HTTP fingerprint. With default
        // headers enabled, it also seeds browser headers in curl's intended order.
        var result = CurlNativeMethods.EasyImpersonate(handle, target.Pointer, options.UseBrowserHeaders ? 1 : 0);
        CurlNativeMethods.ThrowIfError(result, "curl_easy_impersonate");
    }

    private void SetTimeouts(CurlImpersonateHandlerOptions options)
    {
        if (options.ConnectTimeout > TimeSpan.Zero && options.ConnectTimeout != Timeout.InfiniteTimeSpan)
        {
            SetLongOption(CurlOption.ConnectTimeoutMs, (long)options.ConnectTimeout.TotalMilliseconds);
        }

        if (options.Timeout > TimeSpan.Zero && options.Timeout != Timeout.InfiniteTimeSpan)
        {
            SetLongOption(CurlOption.TimeoutMs, (long)options.Timeout.TotalMilliseconds);
        }
    }

    private void SetProxy(CurlImpersonateHandlerOptions options)
    {
        if (options.ProxyUri is null)
        {
            return;
        }

        SetStringOption(CurlOption.Proxy, options.ProxyUri.ToString());

        if (options.ProxyCredentials is not null)
        {
            SetStringOption(CurlOption.ProxyUserPwd,
                $"{options.ProxyCredentials.UserName}:{options.ProxyCredentials.Password}");
        }
    }

    private void SetCertificateValidation(CurlImpersonateHandlerOptions options)
    {
        if (!options.IgnoreCertificateValidation)
        {
            if (OperatingSystem.IsWindows())
            {
                SetLongOption(CurlOption.SslOptions, CurlSslOptions.NativeCa);
            }

            return;
        }

        SetLongOption(CurlOption.SslVerifyPeer, 0);
        SetLongOption(CurlOption.SslVerifyHost, 0);
    }

    private void ConfigureMethod(HttpRequestMessage request)
    {
        if (request.Method == HttpMethod.Head)
        {
            SetLongOption(CurlOption.NoBody, 1);
        }
        else if (request.Method == HttpMethod.Post)
        {
            SetLongOption(CurlOption.Post, 1);
        }
        else if (request.Method != HttpMethod.Get)
        {
            SetStringOption(CurlOption.CustomRequest, request.Method.Method);
        }
    }

    private void ConfigureRequestBody(byte[] body)
    {
        if (requestBodyHandle.IsAllocated)
        {
            requestBodyHandle.Free();
        }

        if (body.Length > 0)
        {
            requestBodyHandle = GCHandle.Alloc(body, GCHandleType.Pinned);
            SetPointerOption(CurlOption.PostFields, requestBodyHandle.AddrOfPinnedObject());
        }
        else
        {
            SetPointerOption(CurlOption.PostFields, 0);
        }

        SetLongOption(CurlOption.PostFieldSizeLarge, body.Length);
    }

    private void ConfigureHeaders(HttpRequestMessage request, CurlImpersonateHandlerOptions options)
    {
        foreach (var header in request.Headers)
        {
            if (CurlHeaderFilters.ShouldSkipRequestHeader(header.Key, options.UseBrowserHeaders))
            {
                continue;
            }

            foreach (var value in header.Value)
            {
                AppendHeader($"{header.Key}: {value}");
            }
        }

        if (options.UseCookies
            && request.RequestUri is not null
            && !request.Headers.Contains("Cookie"))
        {
            var cookieHeader = BuildCookieHeader(options, request.RequestUri);
            if (!string.IsNullOrWhiteSpace(cookieHeader))
            {
                AppendHeader($"Cookie: {cookieHeader}");
            }
        }

        if (request.Content is not null)
        {
            foreach (var header in request.Content.Headers)
            {
                if (CurlHeaderFilters.ShouldSkipRequestHeader(header.Key, options.UseBrowserHeaders))
                {
                    continue;
                }

                foreach (var value in header.Value)
                {
                    AppendHeader($"{header.Key}: {value}");
                }
            }
        }

        if (headerList != 0)
        {
            // libcurl reads this linked list during perform; ownership remains
            // with us and is released through curl_slist_free_all in Dispose.
            SetPointerOption(CurlOption.HttpHeader, headerList);
        }
    }

    private static string BuildCookieHeader(CurlImpersonateHandlerOptions options, Uri uri)
    {
        var cookies = options.CookieContainer.GetCookies(uri);

        if (cookies.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (var cookie in cookies)
        {
            if (builder.Length > 0)
            {
                builder.Append("; ");
            }

            builder.Append(cookie);
        }

        return builder.ToString();
    }

    private void AppendHeader(string header)
    {
        var ptr = AllocUtf8(header);
        headerList = CurlNativeMethods.SlistAppend(headerList, ptr);

        if (headerList == 0)
        {
            throw new InvalidOperationException("curl_slist_append failed");
        }
    }

    private void SetStringOption(CurlOption option, string value)
    {
        SetPointerOption(option, AllocUtf8(value));
    }

    private void SetLongOption(CurlOption option, long value)
    {
        CurlNativeMethods.ThrowIfError(
            CurlNativeMethods.EasySetOpt(handle, option, value),
            $"curl_easy_setopt({option})");
    }

    private void SetPointerOption(CurlOption option, nint value)
    {
        CurlNativeMethods.ThrowIfError(
            CurlNativeMethods.EasySetOpt(handle, option, value),
            $"curl_easy_setopt({option})");
    }

    private nint AllocUtf8(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var ptr = Marshal.AllocHGlobal(bytes.Length + 1);

        try
        {
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            nativeStrings.Add(ptr);
            return ptr;
        }
        catch
        {
            Marshal.FreeHGlobal(ptr);
            throw;
        }
    }

    private string? GetErrorMessage()
    {
        var index = Array.IndexOf(errorBuffer, (byte)0);
        return index <= 0 ? null : Encoding.UTF8.GetString(errorBuffer, 0, index);
    }

    private static nuint OnWrite(nint buffer, nuint size, nuint nmemb, nint userData)
    {
        var context = GetContext(userData);

        if (context.CancellationToken.IsCancellationRequested)
        {
            return 0;
        }

        var byteCount = checked((int)(size * nmemb));

        if (context.ReadResponseContent && byteCount > 0)
        {
            var bytes = new byte[byteCount];
            Marshal.Copy(buffer, bytes, 0, byteCount);
            context.Body.Write(bytes, 0, bytes.Length);
        }

        return size * nmemb;
    }

    private static nuint OnHeader(nint buffer, nuint size, nuint nmemb, nint userData)
    {
        var context = GetContext(userData);

        if (context.CancellationToken.IsCancellationRequested)
        {
            return 0;
        }

        var byteCount = checked((int)(size * nmemb));
        var bytes = new byte[byteCount];
        Marshal.Copy(buffer, bytes, 0, byteCount);
        var headerLine = Encoding.UTF8.GetString(bytes).TrimEnd('\r', '\n');
        context.AddHeaderLine(headerLine);

        return size * nmemb;
    }

    private static int OnProgress(nint clientp, long downloadTotal, long downloadNow, long uploadTotal, long uploadNow)
        => GetContext(clientp).CancellationToken.IsCancellationRequested ? 1 : 0;

    private static CurlRequestContext GetContext(nint userData)
    {
        var handle = GCHandle.FromIntPtr(userData);

        if (handle.Target is not CurlRequestContext context)
        {
            throw new InvalidOperationException("Invalid curl callback context");
        }

        return context;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        if (headerList != 0)
        {
            CurlNativeMethods.SlistFreeAll(headerList);
            headerList = 0;
        }

        foreach (var ptr in nativeStrings)
        {
            Marshal.FreeHGlobal(ptr);
        }

        nativeStrings.Clear();

        if (requestBodyHandle.IsAllocated)
        {
            requestBodyHandle.Free();
        }

        if (handle != 0)
        {
            CurlNativeMethods.EasyCleanup(handle);
        }

        if (errorBufferHandle.IsAllocated)
        {
            errorBufferHandle.Free();
        }

        if (contextHandle.IsAllocated)
        {
            contextHandle.Free();
        }
    }

    private sealed class NativeUtf8String : IDisposable
    {
        public nint Pointer { get; }

        public NativeUtf8String(string value)
        {
            Pointer = Marshal.StringToCoTaskMemUTF8(value);
        }

        public void Dispose()
        {
            if (Pointer != 0)
            {
                Marshal.FreeCoTaskMem(Pointer);
            }
        }
    }
}

internal sealed class CurlRequestContext(CancellationToken cancellationToken, bool readResponseContent) : IDisposable
{
    private readonly List<string> headers = [];

    public CancellationToken CancellationToken { get; } = cancellationToken;

    public bool ReadResponseContent { get; } = readResponseContent;

    public MemoryStream Body { get; } = new();

    private int StatusCode { get; set; }

    public void AddHeaderLine(string headerLine)
    {
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return;
        }

        if (headerLine.StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase))
        {
            StatusCode = ParseStatusCode(headerLine);
            headers.Clear();
            return;
        }

        headers.Add(headerLine);
    }

    public CurlResponseData BuildResponse()
        => new(StatusCode, Body.ToArray(), [.. headers]);

    public void Dispose()
        => Body.Dispose();

    private static int ParseStatusCode(string headerLine)
    {
        var firstSpace = headerLine.IndexOf(' ');

        if (firstSpace < 0 || firstSpace + 4 > headerLine.Length)
        {
            return 0;
        }

        return int.TryParse(headerLine.AsSpan(firstSpace + 1, 3), out var statusCode)
            ? statusCode
            : 0;
    }
}
