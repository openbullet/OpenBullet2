using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Guest;

public class UpdateGuestPasswordDto
{
    [Required]
    public int Id { get; set; }

    [Required, MinLength(8), MaxLength(32)]
    public string Password { get; set; } = string.Empty;
}
