using FluentValidation;

namespace OpenBullet2.Web.Dtos.Guest;

/// <summary>
/// DTO to create a new guest user.
/// </summary>
public class CreateGuestDto
{
    /// <summary>
    /// The username the guest user will use to log in.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// The password the guest user will use to log in.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// The expiration date of the guest user's account, after which
    /// they will not be able to log in anymore.
    /// </summary>
    public DateTime AccessExpiration { get; init; } = DateTime.MaxValue;

    /// <summary>
    /// The list of allowed IP addressed of the guest user.
    /// If empty, any IP is allowed. Entries can be
    /// IPv4 addresses like 192.168.1.1,
    /// ranges of IPv4 addresses like 10.0.0.0/24,
    /// domain names like example.dyndns.org,
    /// IPv6 addresses like ::1
    /// </summary>
    public IEnumerable<string> AllowedAddresses { get; init; } = Array.Empty<string>();
}

internal class CreateGuestDtoValidator : AbstractValidator<CreateGuestDto>
{
    public CreateGuestDtoValidator()
    {
        RuleFor(dto => dto.Username).NotEmpty().MinimumLength(3).MaximumLength(32);
        RuleFor(dto => dto.Password).NotEmpty().MinimumLength(8);
    }
}
