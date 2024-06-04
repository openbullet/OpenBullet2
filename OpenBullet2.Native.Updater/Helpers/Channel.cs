using Spectre.Console;

namespace OpenBullet2.Native.Updater.Helpers;

public static class Channel
{
    public static BuildChannel AskForChannel()
    {
        var response = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Please select the channel")
                .PageSize(3)
                .AddChoices(["Staging (early builds)", "Release (stable builds)"]));
                
        return response switch
        {
            "Staging (early builds)" => BuildChannel.Staging,
            "Release (stable builds)" => BuildChannel.Release,
            _ => BuildChannel.Release
        };
    }
}
