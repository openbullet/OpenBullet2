using AsyncKeyedLock;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Helpers
{
    public static class AsyncLocker
    {
        private static readonly AsyncKeyedLocker<string> semaphores = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<IDisposable> LockAsync(string key, CancellationToken cancellationToken) => semaphores.LockAsync(key, cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<IDisposable> LockAsync(string key) => semaphores.LockAsync(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<IDisposable> LockAsync(Type classType, string methodName, CancellationToken cancellationToken) => semaphores.LockAsync(CombineTypes(classType, methodName), cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<IDisposable> LockAsync(Type classType, string methodName) => semaphores.LockAsync(CombineTypes(classType, methodName));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string CombineTypes(Type classType, string methodName) => $"{classType.FullName}.{methodName}";
    }
}
