using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs.StartConditions
{
    public abstract class StartCondition
    {
        public virtual bool Verify(Job job) 
        {
            throw new NotImplementedException(); 
        }

        public async Task WaitUntilVerified(Job job, CancellationToken cancellationToken = default)
        {
            while (!Verify(job))
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
