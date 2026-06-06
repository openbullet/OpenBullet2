using RuriLib.Http.Curl.Native;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace RuriLib.Http.Curl.Internal;

internal sealed class CurlMultiTransfer : IDisposable
{
    private const int PollTimeoutMilliseconds = 50;

    private readonly nint easyHandle;
    private readonly nint multiHandle;
    private bool easyHandleAdded;
    private bool disposed;

    public CurlMultiTransfer(nint easyHandle)
    {
        if (easyHandle == 0)
        {
            throw new ArgumentException("The easy handle cannot be zero.", nameof(easyHandle));
        }

        this.easyHandle = easyHandle;
        multiHandle = CurlNativeMethods.MultiInit();

        if (multiHandle == 0)
        {
            throw new InvalidOperationException("curl_multi_init failed");
        }

        try
        {
            CurlNativeMethods.ThrowIfMultiError(
                CurlNativeMethods.MultiAddHandle(multiHandle, easyHandle),
                "curl_multi_add_handle");

            easyHandleAdded = true;
        }
        catch
        {
            CurlNativeMethods.MultiCleanup(multiHandle);
            throw;
        }
    }

    public CurlCode Perform(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var registration = cancellationToken.CanBeCanceled
            ? cancellationToken.Register(static state =>
            {
                var handle = (nint)state!;

                if (handle != 0)
                {
                    CurlNativeMethods.MultiWakeup(handle);
                }
            }, multiHandle)
            : default;

        var runningHandles = 0;
        PerformOnce(ref runningHandles);

        while (runningHandles > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CurlNativeMethods.ThrowIfMultiError(
                CurlNativeMethods.MultiPoll(multiHandle, 0, 0, PollTimeoutMilliseconds, out _),
                "curl_multi_poll");

            cancellationToken.ThrowIfCancellationRequested();
            PerformOnce(ref runningHandles);
        }

        return ReadTransferResult();
    }

    private void PerformOnce(ref int runningHandles)
    {
        CurlMultiCode result;

        do
        {
            result = CurlNativeMethods.MultiPerform(multiHandle, out runningHandles);
        }
        while (result == CurlMultiCode.CallMultiPerform);

        CurlNativeMethods.ThrowIfMultiError(result, "curl_multi_perform");
    }

    private CurlCode ReadTransferResult()
    {
        while (true)
        {
            var messagePtr = CurlNativeMethods.MultiInfoRead(multiHandle, out _);

            if (messagePtr == 0)
            {
                break;
            }

            var message = Marshal.PtrToStructure<CurlMultiMessage>(messagePtr);

            if (message.Message == CurlMessage.Done && message.EasyHandle == easyHandle)
            {
                return message.Result;
            }
        }

        throw new InvalidOperationException("curl_multi_info_read did not return a completion message");
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        if (easyHandleAdded)
        {
            CurlNativeMethods.MultiRemoveHandle(multiHandle, easyHandle);
            easyHandleAdded = false;
        }

        CurlNativeMethods.MultiCleanup(multiHandle);
    }
}
