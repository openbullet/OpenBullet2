namespace RuriLib.Functions.Http.Options;

/// <summary>
/// Options for an HTTP request using Basic Authentication.
/// </summary>
public class BasicAuthHttpRequestOptions : HttpRequestOptions
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
