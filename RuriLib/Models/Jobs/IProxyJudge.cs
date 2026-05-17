using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs;

/// <summary>
/// Determines the quality of a proxy by querying one or more proxy judges.
/// </summary>
public interface IProxyJudge
{
    /// <summary>
    /// Determines the quality of a proxy from a list of judge URLs.
    /// </summary>
    /// <param name="http">The proxied HTTP client to use for judge requests.</param>
    /// <param name="judgeUrls">The judge URLs to try in order.</param>
    /// <param name="timeout">The timeout for each judge request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The detected proxy quality, or <see cref="ProxyQuality.Unknown"/> if no judge succeeds.</returns>
    Task<ProxyQuality> DetermineQualityAsync(HttpClient http, IReadOnlyList<string> judgeUrls,
        TimeSpan timeout, CancellationToken cancellationToken);
}
