using System.IO;
using Microsoft.Extensions.Options;
using OpenBullet2.Core.Options;

namespace OpenBullet2.Core.Services;

public sealed class UserDataDirectoryProvider(IOptions<UserDataSettingsOptions> options)
{
    public string RootPath => string.IsNullOrWhiteSpace(options.Value.UserDataFolder)
        ? UserDataSettingsOptions.DefaultUserDataFolder
        : options.Value.UserDataFolder;

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
