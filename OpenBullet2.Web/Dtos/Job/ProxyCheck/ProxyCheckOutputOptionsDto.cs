using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Web.Attributes;

namespace OpenBullet2.Web.Dtos.Job.ProxyCheck;

/// <summary>
/// Information about where to save proxy check results.
/// </summary>
public class ProxyCheckOutputOptionsDto : PolyDto
{
}

/// <summary>
/// Saves proxy check results to the database.
/// </summary>
[PolyType("databaseProxyCheckOutput")]
[MapsFrom(typeof(DatabaseProxyCheckOutputOptions))]
[MapsTo(typeof(DatabaseProxyCheckOutputOptions))]
public class DatabaseProxyCheckOutputOptionsDto : ProxyCheckOutputOptionsDto
{
}
