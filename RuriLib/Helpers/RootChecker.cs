using System.Runtime.InteropServices;
using System.Security.Principal;

namespace RuriLib.Helpers;

/// <summary>
/// Checks whether the current process is running with elevated privileges.
/// </summary>
public static class RootChecker
{
    [DllImport("libc")]
    private static extern uint geteuid();

    /// <summary>
    /// Determines whether the current process is running as root or administrator.
    /// </summary>
    /// <returns><c>true</c> if elevated; otherwise <c>false</c>.</returns>
    public static bool IsRoot()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        return geteuid() == 0;
    }

    /// <summary>
    /// Determines whether the current process is running as root on Unix.
    /// </summary>
    /// <returns><c>true</c> if Unix root; otherwise <c>false</c>.</returns>
    public static bool IsUnixRoot()
        => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && geteuid() == 0;
}
