using System.Runtime.InteropServices;
using System.Security.Principal;

namespace OpenBullet2.Core.Helpers
{
    public static class RootChecker
    {
        [DllImport("libc")]
        private static extern uint geteuid();

        public static bool IsRoot()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                bool isAdmin;
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }

                return isAdmin;
            }
            else
            {
                return geteuid() == 0;
            }
        }

        public static bool IsUnixRoot()
            => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && geteuid() == 0;

        public static string RootWarning =>
@"
====================================================
THIS PROGRAM SHOULD NOT RUN AS ROOT / ADMINISTRATOR.
====================================================

This is due to the fact that configs can contain C# code that is not picked up by your antivirus.
This can lead to information leaks, malware, system takeover and more.
Please consider creating a user with limited priviledges and running it from there.
";
    }
}
