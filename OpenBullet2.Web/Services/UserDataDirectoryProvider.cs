using Microsoft.Extensions.Options;
using OpenBullet2.Web.Options;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Provides normalized paths rooted in the configured user data directory.
/// </summary>
/// <param name="options">The bound web settings options.</param>
public sealed class UserDataDirectoryProvider(IOptions<WebSettingsOptions> options)
{
    /// <summary>
    /// Gets the configured user data root path, falling back to the default when blank.
    /// </summary>
    public string RootPath => string.IsNullOrWhiteSpace(options.Value.UserDataFolder)
        ? WebSettingsOptions.DefaultUserDataFolder
        : options.Value.UserDataFolder;

    /// <summary>
    /// Builds a path under the configured user data root.
    /// </summary>
    /// <param name="segments">The relative path segments to append.</param>
    /// <returns>The combined path.</returns>
    public string GetPath(params string[] segments)
    {
        var path = RootPath;

        foreach (var segment in segments)
        {
            path = Path.Combine(path, segment);
        }

        return path;
    }
}
