using System;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs.StartConditions
{
    public abstract class StartCondition
    {
        public virtual bool Verify(Job job) 
        {
            throw new NotImplementedException(); 
        }

        public async Task WaitUntilVerified(Job job)
        {
            while (!Verify(job))
                await Task.Delay(1000);
        }
    }
}
