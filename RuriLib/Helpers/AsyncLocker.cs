using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Helpers
{
    public class AsyncLocker : IDisposable
    {
        private readonly Dictionary<string, SemaphoreSlim> semaphores = new();

        public async Task Acquire(string key, CancellationToken cancellationToken)
        {
            if (!semaphores.ContainsKey(key))
            {
                semaphores[key] = new SemaphoreSlim(1, 1);
            }

            await semaphores[key].WaitAsync(cancellationToken);
        }

        public void Release(string key) => semaphores[key].Release();
        
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
