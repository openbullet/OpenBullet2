namespace OpenBullet2.Web.Interfaces;

/// <summary>
/// Service that reads the bundled changelog markdown.
/// </summary>
public interface IChangelogService
{
    /// <summary>
    /// Fetches the complete changelog markdown.
    /// </summary>
    Task<string> FetchChangelogAsync(CancellationToken cancellationToken);
}
