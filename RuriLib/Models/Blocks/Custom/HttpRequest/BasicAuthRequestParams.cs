using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest;

/// <summary>
/// Parameters for a basic-auth HTTP request.
/// </summary>
public class BasicAuthRequestParams : RequestParams
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public BlockSetting Username { get; set; } = BlockSettingFactory.CreateStringSetting("username");
    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public BlockSetting Password { get; set; } = BlockSettingFactory.CreateStringSetting("password");
}
