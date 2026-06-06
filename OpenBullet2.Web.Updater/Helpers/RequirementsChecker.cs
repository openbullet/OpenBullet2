using System.Threading.Tasks;

namespace OpenBullet2.Web.Updater.Helpers;

public static class RequirementsChecker
{
    public static async Task EnsureOb2WebNotRunningAsync()
        => await OpenBullet2.Updater.Core.Helpers.UpdaterRequirements.EnsureProcessNotRunningAsync("OpenBullet2.Web");
}
