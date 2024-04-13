using FluentValidation;

namespace OpenBullet2.Web.Dtos.User;

/// <summary>
/// The login information of an admin or guest user.
/// </summary>
public class UserLoginDto
{
    /// <summary>
    /// The username of the user.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The password of the user.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

internal class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(dto => dto.Username).NotEmpty();
        RuleFor(dto => dto.Password).NotEmpty();
    }
}
