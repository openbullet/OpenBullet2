using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs;

/// <summary>
/// Determines proxy quality using judges that return the classic <c>azenv.php</c> format.
/// </summary>
public partial class AzenvProxyJudge : IProxyJudge
{
    private static readonly string[] TransparentHeaders =
    [
        "HTTP_X_FORWARDED_FOR",
        "HTTP_X_REAL_IP",
        "HTTP_CLIENT_IP",
        "HTTP_TRUE_CLIENT_IP",
        "HTTP_CF_CONNECTING_IP"
    ];

    private static readonly string[] AnonymousHeaders =
    [
        "HTTP_VIA",
        "PROXY_REMOTE_ADDR",
        "HTTP_PROXY_CONNECTION",
        "HTTP_X_PROXY_ID",
        "HTTP_X_BLUECOAT_VIA"
    ];

    /// <inheritdoc />
    public async Task<ProxyQuality> DetermineQualityAsync(HttpClient http, IReadOnlyList<string> judgeUrls,
        TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (judgeUrls.Count == 0)
        {
            return ProxyQuality.Unknown;
        }

        foreach (var judgeUrl in judgeUrls)
        {
            try
            {
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(timeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cts.Token, cancellationToken);

                using var response = await http.GetAsync(judgeUrl, linkedCts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
                if (TryClassify(content, out var quality))
                {
                    return quality;
                }
            }
            catch
            {
                // Try the next judge in the list.
            }
        }

        return ProxyQuality.Unknown;
    }

    /// <summary>
    /// Attempts to classify a proxy from an <c>azenv.php</c>-style response body.
    /// </summary>
    /// <param name="content">The raw judge response content.</param>
    /// <param name="quality">The classified proxy quality when successful.</param>
    /// <returns><c>true</c> when the response could be parsed; otherwise <c>false</c>.</returns>
    public bool TryClassify(string content, out ProxyQuality quality)
    {
        quality = ProxyQuality.Unknown;

        if (!TryParseVariables(content, out var variables))
        {
            return false;
        }

        if (HasTransparentMarkers(variables))
        {
            quality = ProxyQuality.Transparent;
            return true;
        }

        if (HasAnonymousMarkers(variables))
        {
            quality = ProxyQuality.Anonymous;
            return true;
        }

        quality = ProxyQuality.Elite;
        return true;
    }

    private static bool TryParseVariables(string content, out Dictionary<string, string> variables)
    {
        variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        foreach (Match match in VariableRegex().Matches(content))
        {
            variables[match.Groups["key"].Value] = match.Groups["value"].Value.Trim();
        }

        return variables.ContainsKey("REMOTE_ADDR");
    }

    private static bool HasTransparentMarkers(IReadOnlyDictionary<string, string> variables)
    {
        foreach (var header in TransparentHeaders)
        {
            if (variables.TryGetValue(header, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return true;
            }
        }

        if (variables.TryGetValue("HTTP_FORWARDED", out var forwardedValue)
            && forwardedValue.Contains("for=", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool HasAnonymousMarkers(IReadOnlyDictionary<string, string> variables)
    {
        foreach (var header in AnonymousHeaders)
        {
            if (variables.TryGetValue(header, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return true;
            }
        }

        return variables.TryGetValue("HTTP_FORWARDED", out var forwardedValue)
            && !string.IsNullOrWhiteSpace(forwardedValue);
    }

    [GeneratedRegex(@"(?m)^\s*(?<key>[A-Z0-9_]+)\s*=\s*(?<value>.*)$")]
    private static partial Regex VariableRegex();
}
