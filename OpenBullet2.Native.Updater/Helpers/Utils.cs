using System;
using Spectre.Console;

namespace OpenBullet2.Native.Updater.Helpers;

public static class Utils
{
    public static void ExitWithError(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[red]Failed! {message}[/]");
        AnsiConsole.MarkupLine("[red]Press any key to exit...[/]");
        Console.ReadKey();
        Environment.Exit(1);
    }
        
    public static void ExitWithError(Exception ex)
        => ExitWithError(ex.Message);
}
