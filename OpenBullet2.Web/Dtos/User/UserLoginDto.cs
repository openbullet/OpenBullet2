using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.User;

public class UserLoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
