using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Service that reads the changelog bundled with the application.
/// </summary>
public class ChangelogService : IChangelogService
{
    /// <inheritdoc />
    public async Task<string> FetchChangelogAsync(CancellationToken cancellationToken)
    {
        await using var stream = typeof(ChangelogService).Assembly
            .GetManifestResourceStream("OpenBullet2.Changelog.md")
            ?? throw new InvalidOperationException("The bundled changelog could not be found");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
