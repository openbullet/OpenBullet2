using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Guest;

/// <summary>
/// DTO to update a guest user's password.
/// </summary>
public class UpdateGuestPasswordDto
{
    /// <summary>
    /// The id of the guest user to update.
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// The new password the guest user will use to log in.
    /// </summary>
    [Required, MinLength(8), MaxLength(32)]
    public string Password { get; set; } = default!;
}
