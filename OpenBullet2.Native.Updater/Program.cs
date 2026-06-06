
using System;
using System.Threading.Tasks;
using CommandLine;
using OpenBullet2.Updater.Core;
using OpenBullet2.Updater.Core.Helpers;
using OpenBullet2.Native.Updater.Helpers;

namespace OpenBullet2.Native.Updater;

public static class Program
{
    private static readonly UpdaterSettings Settings = new(
        ReleaseAssetNames.GetCurrentNativeAssetName(),
        RequirementsChecker.EnsureOb2NativeNotRunningAsync);

    private static async Task Main(string[] args)
    {
        try
        {
            await new Parser(with => { with.CaseInsensitiveEnumValues = true; }).ParseArguments<CliOptions>(args)
                .WithParsedAsync(async opts => await UpdaterRunner.UpdateAsync(opts, Settings));
        }
        catch (Exception ex)
        {
            Utils.ExitWithError(ex);
        }
    }
}
