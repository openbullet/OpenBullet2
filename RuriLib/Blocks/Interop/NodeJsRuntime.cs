using Jering.Javascript.NodeJS;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Interop;

/// <summary>
/// Configures the shared NodeJS runtime used by Script blocks.
/// </summary>
internal static class NodeJsRuntime
{
    internal const int MaxTransientDisposalRetryAttempts = 3;

    private const string DefaultMeterFactoryObjectName = "DefaultMeterFactory";
    private const string SemaphoreSlimObjectName = "System.Threading.SemaphoreSlim";
    private static readonly object SyncRoot = new();
    private static bool configured;

    public static void EnsureConfigured()
    {
        if (configured)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (configured)
            {
                return;
            }

            StaticNodeJSService.Configure<OutOfProcessNodeJSServiceOptions>(options =>
            {
                options.ConnectionTimeoutMS = 15000;
                options.Concurrency = Concurrency.MultiProcess;
                options.ConcurrencyDegree = 0;
                options.NumConnectionRetries = 1;
            });

            configured = true;
        }
    }

    public static async Task<T?> InvokeFromStringAsync<T>(
        string moduleString,
        string? cacheIdentifier,
        string? exportName,
        object[] args,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        return await InvokeWithTransientDisposalRetryAsync(
            () => StaticNodeJSService.InvokeFromStringAsync<T>(
                moduleString,
                cacheIdentifier,
                exportName,
                args,
                cancellationToken),
            cancellationToken).ConfigureAwait(false);
    }

    public static async Task<T?> InvokeFromStringAsync<T>(
        Func<string> moduleFactory,
        string cacheIdentifier,
        string? exportName,
        object[] args,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        return await InvokeWithTransientDisposalRetryAsync(
            () => StaticNodeJSService.InvokeFromStringAsync<T>(
                moduleFactory,
                cacheIdentifier,
                exportName,
                args,
                cancellationToken),
            cancellationToken).ConfigureAwait(false);
    }

    public static async Task<T?> InvokeFromFileAsync<T>(
        string modulePath,
        string? exportName,
        object[] args,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        return await InvokeWithTransientDisposalRetryAsync(
            () => StaticNodeJSService.InvokeFromFileAsync<T>(
                modulePath,
                exportName,
                args,
                cancellationToken),
            cancellationToken).ConfigureAwait(false);
    }

    public static async Task<T?> InvokeWithTransientDisposalRetryAsync<T>(
        Func<Task<T?>> invocation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await invocation().ConfigureAwait(false);
            }
            catch (ObjectDisposedException ex) when (
                IsTransientNodeJsDisposal(ex) &&
                attempt < MaxTransientDisposalRetryAttempts &&
                !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    internal static bool IsTransientNodeJsDisposal(ObjectDisposedException exception)
        => IsDisposedObject(exception, SemaphoreSlimObjectName) ||
           IsDisposedObject(exception, DefaultMeterFactoryObjectName);

    private static bool IsDisposedObject(ObjectDisposedException exception, string objectName)
        => exception.ObjectName == objectName ||
           exception.Message.Contains(objectName, StringComparison.Ordinal);
}
