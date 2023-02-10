using OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

namespace OpenBullet2.Web.Dtos.Config.Blocks;

/// <summary>
/// DTO that represents a keycheck block instance.
/// </summary>
public class KeycheckBlockInstanceDto : BlockInstanceDto
{
    /// <summary>
    /// The ordered list of keychains.
    /// </summary>
    public List<KeychainDto> Keychains { get; set; } = new();
}
