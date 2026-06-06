using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs.StartConditions;

/// <summary>
/// Represents a condition that controls when a job can start.
/// </summary>
public abstract class StartCondition
{
    /// <summary>
    /// Verifies whether the start condition has been satisfied.
    /// </summary>
    /// <param name="job">The job being evaluated.</param>
    /// <returns><see langword="true"/> if the job can start.</returns>
    public virtual bool Verify(Job job)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Waits until the start condition has been satisfied.
    /// </summary>
    /// <param name="job">The job being evaluated.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the condition becomes valid.</returns>
    public async Task WaitUntilVerified(Job job, CancellationToken cancellationToken = default)
    {
        while (!Verify(job))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}
