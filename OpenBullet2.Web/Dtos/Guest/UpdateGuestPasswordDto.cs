using FluentValidation;

namespace OpenBullet2.Web.Dtos.Guest;

/// <summary>
/// DTO to update a guest user's password.
/// </summary>
public class UpdateGuestPasswordDto
{
    /// <summary>
    /// The id of the guest user to update.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// The new password the guest user will use to log in.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}

internal class UpdateGuestPasswordDtoValidator : AbstractValidator<UpdateGuestPasswordDto>
{
    public UpdateGuestPasswordDtoValidator()
    {
        RuleFor(dto => dto.Id).GreaterThan(0);
        RuleFor(dto => dto.Password).NotEmpty().MinimumLength(8);
    }
}
