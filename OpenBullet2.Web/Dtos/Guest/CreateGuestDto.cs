using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Guest;

/// <summary>
/// DTO to create a new guest user.
/// </summary>
public class CreateGuestDto
{
    /// <summary>
    /// The username the guest user will use to log in.
    /// </summary>
    [Required, MinLength(3), MaxLength(32)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The password the guest user will use to log in.
    /// </summary>
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// The expiration date of the guest user's account, after which
    /// they will not be able to log in anymore.
    /// </summary>
    public DateTime AccessExpiration { get; set; } = DateTime.MaxValue;

    /// <summary>
    /// The list of allowed IP addressed of the guest user.
    /// If empty, any IP is allowed. Entries can be
    /// IPv4 addresses like 192.168.1.1,
    /// ranges of IPv4 addresses like 10.0.0.0/24,
    /// domain names like example.dyndns.org,
    /// IPv6 addresses like ::1
    /// </summary>
    public List<string> AllowedAddresses { get; set; } = new();
}
