using FluentValidation;

namespace OpenBullet2.Web.Dtos.Wordlist;

/// <summary>
/// DTO to update a wordlist's info.
/// </summary>
public class UpdateWordlistInfoDto
{
    /// <summary>
    /// The id of the wordlist to update.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// The name of the wordlist.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The purpose of the wordlist.
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// The wordlist type.
    /// </summary>
    public string WordlistType { get; set; } = "Default";
}

internal class UpdateWordlistInfoDtoValidator : AbstractValidator<UpdateWordlistInfoDto>
{
    public UpdateWordlistInfoDtoValidator()
    {
        RuleFor(dto => dto.Id).GreaterThan(0);
        RuleFor(dto => dto.Name).NotEmpty();
    }
}
