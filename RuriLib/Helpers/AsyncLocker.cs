using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Helpers
{
    public class AsyncLocker : IDisposable
    {
        private readonly Dictionary<string, SemaphoreSlim> semaphores = new();

        public Task Acquire(string key, CancellationToken cancellationToken = default)
        {
            if (!semaphores.ContainsKey(key))
            {
                semaphores[key] = new SemaphoreSlim(1, 1);
            }

            return semaphores[key].WaitAsync(cancellationToken);
        }

        public Task Acquire(Type classType, string methodName, CancellationToken cancellationToken = default)
            => Acquire(CombineTypes(classType, methodName), cancellationToken);

        public void Release(string key) => semaphores[key].Release();

        public void Release(Type classType, string methodName) => Release(CombineTypes(classType, methodName));

        private string CombineTypes(Type classType, string methodName) => $"{classType.FullName}.{methodName}";

        public void Dispose()
        {
            foreach (var semaphore in semaphores.Values)
            {
                try
                {
                    semaphore.Dispose();
                }
                catch
                {

                }
            }
        }
    }
}
