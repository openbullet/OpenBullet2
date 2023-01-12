using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Guest;

public class UpdateGuestInfoDto
{
    [Required]
    public int Id { get; set; }

    [Required, MinLength(3), MaxLength(32)]
    public string Username { get; set; } = string.Empty;

    public DateTime AccessExpiration { get; set; } = DateTime.MaxValue;

    public List<string> AllowedAddresses { get; set; } = new();
}
