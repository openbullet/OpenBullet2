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
    internal const int MaxSemaphoreSlimRetryAttempts = 3;

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
                options.Concurrency = Concurrency.MultiProcess;
                options.ConcurrencyDegree = 0;
            });

            configured = true;
        }
    }

    public static async Task<T?> InvokeWithSemaphoreSlimRetryAsync<T>(
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
                IsSemaphoreSlimDisposal(ex) &&
                attempt < MaxSemaphoreSlimRetryAttempts &&
                !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    internal static bool IsSemaphoreSlimDisposal(ObjectDisposedException exception)
        => exception.ObjectName == SemaphoreSlimObjectName ||
           exception.Message.Contains(SemaphoreSlimObjectName, StringComparison.Ordinal);
}
