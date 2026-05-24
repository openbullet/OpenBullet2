using System.Threading.Tasks;

namespace OpenBullet2.Native.Updater.Helpers;

public static class RequirementsChecker
{
    public static async Task EnsureOb2NativeNotRunningAsync()
        => await OpenBullet2.Updater.Core.Helpers.UpdaterRequirements.EnsureProcessNotRunningAsync("OpenBullet2.Native");
}
