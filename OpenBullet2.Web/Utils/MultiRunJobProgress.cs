using RuriLib.Models.Jobs;

namespace OpenBullet2.Web.Utils;

internal static class MultiRunJobProgress
{
    public static long GetEffectiveDataTested(MultiRunJob job)
    {
        var total = job.DataPool?.Size ?? 0;
        var tested = job.Status switch
        {
            JobStatus.Idle when job.LastRunOutcome == JobLastRunOutcome.Completed => total,
            JobStatus.Idle => job.Skip,
            _ => (long)job.Skip + job.DataTested
        };

        return total > 0
            ? Math.Min(tested, total)
            : tested;
    }

    public static double GetEffectiveProgress(MultiRunJob job)
    {
        var total = job.DataPool?.Size ?? 0;

        if (total <= 0)
        {
            return job.Progress < 0 ? 0 : job.Progress;
        }

        return (double)GetEffectiveDataTested(job) / total;
    }
}
