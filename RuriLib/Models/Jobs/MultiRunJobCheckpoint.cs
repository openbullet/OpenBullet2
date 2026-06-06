namespace RuriLib.Models.Jobs;

/// <summary>
/// Helper methods for calculating the checkpoint of a multi-run job.
/// </summary>
public static class MultiRunJobCheckpoint
{
    /// <summary>
    /// Computes the next skip value after processing some lines.
    /// If the job reached the end of a finite data pool, the skip resets to 0
    /// so the next start begins from the start again.
    /// </summary>
    public static int GetNextSkip(int currentSkip, int processed, long total)
    {
        var nextSkip = currentSkip + processed;

        if (total > 0 && nextSkip >= total)
        {
            return 0;
        }

        return nextSkip;
    }
}
