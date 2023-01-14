using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.User;

/// <summary>
/// The login information of an admin or guest user.
/// </summary>
public class UserLoginDto
{
    /// <summary>
    /// The username of the user.
    /// </summary>
    [Required]
    public string Username { get; set; } = default!;

    /// <summary>
    /// The password of the user.
    /// </summary>
    [Required]
    public string Password { get; set; } = default!;
}
