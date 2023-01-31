using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Settings;

/// <summary>
/// DTO to update an admin user's password.
/// </summary>
public class UpdateAdminPasswordDto
{
    /// <summary>
    /// The new password the admin user will use to log in.
    /// </summary>
    [Required, MinLength(8), MaxLength(32)]
    public string Password { get; set; } = string.Empty;
}
