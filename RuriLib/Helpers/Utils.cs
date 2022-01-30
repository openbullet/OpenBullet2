using System;

namespace RuriLib.Helpers
{
    public static class Utils
    {
        public static bool IsDocker() => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    }
}
