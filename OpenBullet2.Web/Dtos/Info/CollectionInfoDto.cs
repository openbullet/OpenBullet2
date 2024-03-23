namespace OpenBullet2.Web.Dtos.Info;

/// <summary>
/// Info about the items in several collections.
/// </summary>
public class CollectionInfoDto
{
    /// <summary>
    /// The number of created jobs.
    /// </summary>
    public int JobsCount { get; set; }

    /// <summary>
    /// The number of imported proxies.
    /// </summary>
    public int ProxiesCount { get; set; }

    /// <summary>
    /// The number of imported wordlists.
    /// </summary>
    public int WordlistsCount { get; set; }

    /// <summary>
    /// The number of lines across all wordlists.
    /// </summary>
    public long WordlistsLines { get; set; }

    /// <summary>
    /// The number of obtained hits.
    /// </summary>
    public int HitsCount { get; set; }

    /// <summary>
    /// The number of configs.
    /// </summary>
    public int ConfigsCount { get; set; }

    /// <summary>
    /// The number of guest users.
    /// </summary>
    public int GuestsCount { get; set; }

    /// <summary>
    /// The number of plugins.
    /// </summary>
    public int PluginsCount { get; set; }
}
