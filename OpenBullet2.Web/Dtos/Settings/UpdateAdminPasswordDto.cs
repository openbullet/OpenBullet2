using FluentValidation;

namespace OpenBullet2.Web.Dtos.Settings;

/// <summary>
/// DTO to update an admin user's password.
/// </summary>
public class UpdateAdminPasswordDto
{
    /// <summary>
    /// The new password the admin user will use to log in.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

internal class UpdateAdminPasswordDtoValidator : AbstractValidator<UpdateAdminPasswordDto>
{
    public UpdateAdminPasswordDtoValidator()
    {
        RuleFor(dto => dto.Password).MinimumLength(8);
    }
}
