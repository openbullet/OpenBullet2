using System.Text.RegularExpressions;

namespace OpenBullet2.Web.Updater.Helpers;

public static partial class InputValidation
{
    public static void ValidateRepository(string repository)
    {
        // Make sure the repository is in the right format
        if (!RepositoryRegex().IsMatch(repository))
        {
            Utils.ExitWithError("The repository must be in the format owner/repo (e.g. openbullet/OpenBullet2)");
        }
    }

    [GeneratedRegex(@"^[\w-]+/[\w-]+$")]
    private static partial Regex RepositoryRegex();
}
