namespace OpenBullet2.Web.Dtos.Guest;

public class GuestDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime AccessExpiration { get; set; } = DateTime.MaxValue;
    public List<string> AllowedAddresses { get; set; } = new();
}
