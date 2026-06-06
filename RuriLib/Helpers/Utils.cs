using System;

namespace RuriLib.Helpers;

/// <summary>
/// Provides miscellaneous environment helpers.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Determines whether the process is running inside a Docker container.
    /// </summary>
    /// <returns><c>true</c> if running in Docker; otherwise <c>false</c>.</returns>
    public static bool IsDocker() => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
}
