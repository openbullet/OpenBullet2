using FluentValidation;

namespace OpenBullet2.Web.Dtos.Hit;

/// <summary>
/// DTO that contains information about a new hit.
/// </summary>
public class CreateHitDto
{
    /// <summary>
    /// The data that was provided to the bot to get the hit.
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// The variables captured by the bot.
    /// </summary>
    public string CapturedData { get; set; } = string.Empty;

    /// <summary>
    /// The string representation of the proxy that was used to get the hit, if any.
    /// </summary>
    public string? Proxy { get; set; } = null;

    /// <summary>
    /// The exact date and time when the hit was found. If null, the
    /// current time will be used.
    /// </summary>
    public DateTime? Date { get; set; } = null;

    /// <summary>
    /// The type of hit, for example SUCCESS, NONE, CUSTOM etc.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the config that was used to get the hit.
    /// </summary>
    public string? ConfigId { get; set; } = null;

    /// <summary>
    /// The name of the config that was used to get the hit.
    /// Needed to identify the name even if the config was deleted.
    /// </summary>
    public string? ConfigName { get; set; } = null;

    /// <summary>
    /// The category of the config that was used to get the hit.
    /// Needed to identify the category even if the config was deleted.
    /// </summary>
    public string? ConfigCategory { get; set; } = null;

    /// <summary>
    /// The ID of the wordlist that was used to get the hit, -1 if no wordlist was used, &lt; -1 for other data pools.
    /// </summary>
    public int WordlistId { get; set; } = -1;

    /// <summary>
    /// The name of the wordlist that was used to get the hit, blank if no wordlist was used.
    /// Needed to identify the name even if the wordlist was deleted. If <see cref="WordlistId" /> is less than -1,
    /// this field contains information about the data pool that was used.
    /// </summary>
    public string? WordlistName { get; set; } = null;
}

internal class CreateHitDtoValidator : AbstractValidator<CreateHitDto>
{
    public CreateHitDtoValidator()
    {
        RuleFor(dto => dto.Data).NotEmpty();
        RuleFor(dto => dto.Type).NotEmpty();
    }
}
