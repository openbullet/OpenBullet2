namespace OpenBullet2.Web.Dtos.Config.Blocks;

/// <summary>
/// The type of block instance.
/// </summary>
public enum BlockInstanceType
{
    /// <summary>
    /// Auto block.
    /// </summary>
    Auto,

    /// <summary>
    /// HTTP request block.
    /// </summary>
    HttpRequest,

    /// <summary>
    /// Key check block.
    /// </summary>
    Keycheck,

    /// <summary>
    /// Script block.
    /// </summary>
    Script,

    /// <summary>
    /// Parse block.
    /// </summary>
    Parse,

    /// <summary>
    /// LoliCode block.
    /// </summary>
    LoliCode
}
