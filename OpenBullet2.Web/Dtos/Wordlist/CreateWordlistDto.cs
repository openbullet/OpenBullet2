using FluentValidation;

namespace OpenBullet2.Web.Dtos.Wordlist;

/// <summary>
/// DTO used to create a wordlist that references
/// an existing file on disk.
/// </summary>
public class CreateWordlistDto
{
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

    /// <summary>
    /// The path to the actual file on disk.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}

internal class CreateWordlistDtoValidator : AbstractValidator<CreateWordlistDto>
{
    public CreateWordlistDtoValidator()
    {
        RuleFor(dto => dto.Name).NotEmpty();
        RuleFor(dto => dto.FilePath).NotEmpty();
    }
}
