using CommandLine;
using System;
using System.Threading.Tasks;
using OpenBullet2.Updater.Core;
using OpenBullet2.Updater.Core.Helpers;
using OpenBullet2.Web.Updater.Helpers;

namespace OpenBullet2.Web.Updater;

public static class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var settings = new UpdaterSettings(
                ReleaseAssetNames.GetCurrentWebAssetName(),
                RequirementsChecker.EnsureOb2WebNotRunningAsync);

            await new Parser(with => { with.CaseInsensitiveEnumValues = true; }).ParseArguments<CliOptions>(args)
                .WithParsedAsync(async opts => await UpdaterRunner.UpdateAsync(opts, settings));
        }
        catch (Exception ex)
        {
            Utils.ExitWithError(ex);
        }
    }
}
