namespace OpenBullet2.Web.Dtos.User;

/// <summary>
/// DTO that contains information about the user that owns a resource.
/// </summary>
public class OwnerDto
{
    /// <summary>
    /// The id of the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The username of the user.
    /// </summary>
    public string Username { get; set; } = string.Empty;
}
