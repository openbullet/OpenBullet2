namespace OpenBullet2.Web.Dtos.User;

/// <summary>
/// DTO that contains post-login information of a user.
/// </summary>
public class LoggedInUserDto
{
    /// <summary>
    /// The authentication token to be used for authenticated requests.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
