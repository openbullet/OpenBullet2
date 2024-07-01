using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Web.Attributes;
using RuriLib.Models.Proxies;

namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Information about a source of proxies.
/// </summary>
public class ProxySourceOptionsDto : PolyDto
{
}

/// <summary>
/// Reads proxies from a proxy group in the database.
/// </summary>
[PolyType("groupProxySource")]
[MapsFrom(typeof(GroupProxySourceOptions))]
[MapsTo(typeof(GroupProxySourceOptions))]
public class GroupProxySourceOptionsDto : ProxySourceOptionsDto
{
    /// <summary>
    /// The ID of the proxy group.
    /// </summary>
    public int GroupId { get; set; } = -1;
}

/// <summary>
/// Reads proxies from a file.
/// </summary>
[PolyType("fileProxySource")]
[MapsFrom(typeof(FileProxySourceOptions))]
[MapsTo(typeof(FileProxySourceOptions))]
public class FileProxySourceOptionsDto : ProxySourceOptionsDto
{
    /// <summary>
    /// The path to the file where proxies are stored in a UTF-8 text format, one per line,
    /// in a format that is supported by OB2.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The default proxy type when not specified by the format of the proxy.
    /// </summary>
    public ProxyType DefaultType { get; set; } = ProxyType.Http;
}

/// <summary>
/// Reads proxies from a remote endpoint.
/// </summary>
[PolyType("remoteProxySource")]
[MapsFrom(typeof(RemoteProxySourceOptions))]
[MapsTo(typeof(RemoteProxySourceOptions))]
public class RemoteProxySourceOptionsDto : ProxySourceOptionsDto
{
    /// <summary>
    /// The URL to query in order to retrieve the proxies.
    /// The API should return a text-based response with one proxy per line, in a format supported by OB2.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The default proxy type when not specified by the format of the proxy.
    /// </summary>
    public ProxyType DefaultType { get; set; } = ProxyType.Http;
}
