using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Interop;

/// <summary>
/// Limits concurrent access to the shared static NodeJS service.
/// </summary>
internal static class NodeJsInvocationGate
{
    internal const int MaxConcurrentInvocations = 1;

    private static readonly SemaphoreSlim Semaphore = new(
        MaxConcurrentInvocations,
        MaxConcurrentInvocations);

    public static async Task<T> RunAsync<T>(
        Func<Task<T>> invocation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await invocation().ConfigureAwait(false);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
