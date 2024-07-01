namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A key of the keychain.
/// </summary>
public class KeyDto : PolyDto
{
    /// <summary>
    /// The left comparison term.
    /// </summary>
    public BlockSettingDto? Left { get; set; }

    /// <summary>
    /// The right comparison term.
    /// </summary>
    public BlockSettingDto? Right { get; set; }
}
