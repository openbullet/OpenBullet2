namespace OpenBullet2.Core.Helpers
{
    public static class RootUtils
    {
        public static string RootWarning =>
@"
====================================================
THIS PROGRAM SHOULD NOT RUN AS ROOT / ADMINISTRATOR.
====================================================

This is due to the fact that configs can contain C# code that is not picked up by your antivirus.
This can lead to information leaks, malware, system takeover and more.
Please consider creating a user with limited privileges and running it from there.
";
    }
}
