using FluentValidation;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Information needed to change the number of bots in a job.
/// </summary>
public class ChangeBotsDto
{
    /// <summary>
    /// The id of the job.
    /// </summary>
    public required int JobId { get; set; }

    /// <summary>
    /// The desired number of bots.
    /// </summary>
    public required int Bots { get; set; }
}

internal class ChangeBotsDtoValidator : AbstractValidator<ChangeBotsDto>
{
    public ChangeBotsDtoValidator()
    {
        RuleFor(dto => dto.JobId).GreaterThan(0);
        RuleFor(dto => dto.Bots).GreaterThan(0);
    }
}
