namespace OpenBullet2.Web.Interfaces;

/// <summary>
/// Service that reads changelog markdown for a specific version.
/// </summary>
public interface IChangelogService
{
    /// <summary>
    /// Fetches the changelog markdown for the provided version.
    /// </summary>
    Task<string> FetchChangelogAsync(string version, CancellationToken cancellationToken);
}
