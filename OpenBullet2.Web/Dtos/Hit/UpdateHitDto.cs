using FluentValidation;

namespace OpenBullet2.Web.Dtos.Hit;

/// <summary>
/// DTO that contains information about some fields of a hit
/// that can be updated.
/// </summary>
public class UpdateHitDto
{
    /// <summary>
    /// The id of the hit to update.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// The data that was provided to the bot to get the hit.
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// The variables captured by the bot.
    /// </summary>
    public string CapturedData { get; set; } = string.Empty;

    /// <summary>
    /// The type of hit, for example SUCCESS, NONE, CUSTOM etc.
    /// </summary>
    public string Type { get; set; } = string.Empty;
}

internal class UpdateHitDtoValidator : AbstractValidator<UpdateHitDto>
{
    public UpdateHitDtoValidator()
    {
        RuleFor(dto => dto.Id).GreaterThan(0);
        RuleFor(dto => dto.Data).NotEmpty();
        RuleFor(dto => dto.CapturedData).NotEmpty();
        RuleFor(dto => dto.Type).NotEmpty();
    }
}
