namespace OpenBullet2.Web.Models.Identity;

/// <summary>
/// Information of a logged in API user.
/// </summary>
internal class ApiUser
{
    public int Id { get; set; } = -1;
    public UserRole Role { get; set; } = UserRole.Anonymous;
    public string? Username { get; set; }
}

internal enum UserRole
{
    Admin,
    Guest,
    Anonymous
}
