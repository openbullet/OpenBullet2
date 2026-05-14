using System.Net;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Service that reads changelog files from the OpenBullet2 repository on GitHub.
/// </summary>
public class ChangelogService(HttpClient httpClient) : IChangelogService
{
    private readonly HttpClient _httpClient = httpClient;

    /// <inheritdoc />
    public async Task<string> FetchChangelogAsync(string version, CancellationToken cancellationToken)
    {
        var url = $"https://raw.githubusercontent.com/openbullet/OpenBullet2/master/Changelog/{version}.md";
        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new ResourceNotFoundException(
                ErrorCode.RemoteResourceNotFound,
                $"Changelog for version {version}", url);
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
