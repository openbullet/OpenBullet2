using RuriLib.Models.Conditions.Comparisons;
using System;

namespace RuriLib.Models.Jobs.Monitor.Triggers;

/// <summary>
/// Base class for triggers that target a <see cref="MultiRunJob"/>.
/// </summary>
public abstract class MultiRunJobTrigger : Trigger
{
    /// <inheritdoc />
    public override bool CheckStatus(Job job)
    {
        if (job is not MultiRunJob multiRunJob)
        {
            throw new InvalidOperationException("The job must be a MultiRunJob");
        }

        return CheckStatus(multiRunJob);
    }

    /// <summary>
    /// Checks whether the trigger matches the given <see cref="MultiRunJob"/>.
    /// </summary>
    /// <param name="job">The job to inspect.</param>
    /// <returns><see langword="true"/> if the trigger matches.</returns>
    public virtual bool CheckStatus(MultiRunJob job)
        => throw new NotImplementedException();
}

#region Data Numeric Triggers
/// <summary>Matches the tested-lines count.</summary>
public class TestedCountTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.DataTested, Comparison, Amount);
}

/// <summary>Matches the hit count.</summary>
public class HitCountTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.DataHits, Comparison, Amount);
}

/// <summary>Matches the custom-status count.</summary>
public class CustomCountTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.DataCustom, Comparison, Amount);
}

/// <summary>Matches the to-check count.</summary>
public class ToCheckCountTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.DataToCheck, Comparison, Amount);
}

/// <summary>Matches the fail count.</summary>
public class FailCountTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.DataFails, Comparison, Amount);
}

/// <summary>Matches the retry count.</summary>
public class RetryCountTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.DataRetried, Comparison, Amount);
}

/// <summary>Matches the ban count.</summary>
public class BanCountTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.DataBanned, Comparison, Amount);
}

/// <summary>Matches the error count.</summary>
public class ErrorCountTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.DataErrors, Comparison, Amount);
}
#endregion

#region Proxy Numeric Triggers
/// <summary>Matches the alive-proxies count.</summary>
public class AliveProxiesCountTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.ProxiesAlive, Comparison, Amount);
}

/// <summary>Matches the banned-proxies count.</summary>
public class BannedProxiesCountTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.ProxiesBanned, Comparison, Amount);
}
#endregion

#region Other Triggers
/// <summary>Matches the CPM value.</summary>
public class CPMTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(job.CPM, Comparison, Amount);
}

/// <summary>Matches the captcha credit.</summary>
public class CaptchaCreditTrigger : MultiRunJobTrigger
{
    /// <summary>Gets or sets the comparison operator.</summary>
    public NumComparison Comparison { get; set; }
    /// <summary>Gets or sets the comparison amount.</summary>
    public float Amount { get; set; }

    /// <inheritdoc />
    public override bool CheckStatus(MultiRunJob job)
        => Functions.Conditions.Conditions.Check(Convert.ToSingle(job.CaptchaCredit), Comparison, Amount);
}
#endregion
