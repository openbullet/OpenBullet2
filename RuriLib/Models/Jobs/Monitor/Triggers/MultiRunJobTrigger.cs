using RuriLib.Models.Conditions.Comparisons;
using System;

namespace RuriLib.Models.Jobs.Monitor.Triggers
{
    public abstract class MultiRunJobTrigger : Trigger
    {
        public override bool CheckStatus(Job job)
            => CheckStatus(job as MultiRunJob);

        public virtual bool CheckStatus(MultiRunJob job)
            => throw new NotImplementedException();
    }

    public class JobFinishedTrigger : MultiRunJobTrigger
    {
        public override bool CheckStatus(MultiRunJob job)
            => job.Status == JobStatus.Idle && job.Progress == 1f;
    }

    #region Data Numeric Triggers
    public class TestedCountTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.DataTested, Comparison, Amount);
    }

    public class HitCountTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.DataHits, Comparison, Amount);
    }

    public class CustomCountTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.DataCustom, Comparison, Amount);
    }

    public class ToCheckCountTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.DataToCheck, Comparison, Amount);
    }

    public class FailCountTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.DataFails, Comparison, Amount);
    }

    public class RetryCountTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.DataRetried, Comparison, Amount);
    }

    public class BanCountTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.DataBanned, Comparison, Amount);
    }

    public class ErrorCountTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.DataErrors, Comparison, Amount);
    }
    #endregion

    #region Proxy Numeric Triggers
    public class AliveProxiesCountTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.ProxiesAlive, Comparison, Amount);
    }

    public class BannedProxiesCountTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.ProxiesBanned, Comparison, Amount);
    }
    #endregion

    #region Other Triggers
    public class CPMTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.CPM, Comparison, Amount);
    }

    public class CaptchaCreditTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public float Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(Convert.ToSingle(job.CaptchaCredit), Comparison, Amount);
    }

    public class ProgressTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public float Amount { get; set; }

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.Progress * 100, Comparison, Amount);
    }

    public class TimeElapsedTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Seconds { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Hours { get; set; } = 0;
        public int Days { get; set; } = 0;

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.Elapsed, Comparison, new TimeSpan(Days, Hours, Minutes, Seconds));
    }

    public class TimeRemainingTrigger : MultiRunJobTrigger
    {
        public NumComparison Comparison { get; set; }
        public int Seconds { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Hours { get; set; } = 0;
        public int Days { get; set; } = 0;

        public override bool CheckStatus(MultiRunJob job)
            => Functions.Conditions.Conditions.Check(job.Remaining, Comparison, new TimeSpan(Days, Hours, Minutes, Seconds));
    }
    #endregion
}
