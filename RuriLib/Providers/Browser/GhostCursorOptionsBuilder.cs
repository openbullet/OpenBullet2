using GhostCursorSharp;
using RuriLib.Models.Configs.Settings;

namespace RuriLib.Providers.Browser;

internal static class GhostCursorOptionsBuilder
{
    public static GhostCursorOptions BuildCursorOptions(BrowserSettings settings, bool performRandomMoves)
        => new()
        {
            DefaultOptions = BuildDefaultOptions(settings),
            PerformRandomMoves = performRandomMoves,
            Visible = false
        };

    public static DefaultOptions BuildDefaultOptions(BrowserSettings settings)
    {
        var ghostCursor = settings.GhostCursor;

        return new()
        {
            RandomMove = new RandomMoveOptions
            {
                MoveSpeed = ghostCursor.MoveSpeed,
                MoveDelay = ghostCursor.MoveDelay,
                RandomizeMoveDelay = ghostCursor.RandomizeMoveDelay,
                DelayPerStep = ghostCursor.DelayPerStep
            },
            Move = BuildMoveOptions(settings),
            MoveTo = BuildMoveToOptions(settings),
            Click = BuildClickOptions(settings),
            Scroll = BuildScrollIntoViewOptions(settings)
        };
    }

    public static MoveOptions BuildMoveOptions(BrowserSettings settings)
    {
        var ghostCursor = settings.GhostCursor;

        return new()
        {
            MoveSpeed = ghostCursor.MoveSpeed,
            MoveDelay = ghostCursor.MoveDelay,
            RandomizeMoveDelay = ghostCursor.RandomizeMoveDelay,
            DelayPerStep = ghostCursor.DelayPerStep,
            ScrollSpeed = ghostCursor.ScrollSpeed,
            ScrollDelay = ghostCursor.ScrollDelay,
            MaxTries = ghostCursor.MaxTries,
            OvershootThreshold = ghostCursor.OvershootThreshold
        };
    }

    public static MoveToOptions BuildMoveToOptions(BrowserSettings settings)
    {
        var ghostCursor = settings.GhostCursor;

        return new()
        {
            MoveSpeed = ghostCursor.MoveSpeed,
            MoveDelay = ghostCursor.MoveDelay,
            RandomizeMoveDelay = ghostCursor.RandomizeMoveDelay,
            DelayPerStep = ghostCursor.DelayPerStep
        };
    }

    public static ClickOptions BuildClickOptions(
        BrowserSettings settings,
        MouseButton? button = null,
        int? clickCount = null,
        int? waitForClick = null)
    {
        var ghostCursor = settings.GhostCursor;

        return new()
        {
            Button = button,
            ClickCount = clickCount,
            Hesitate = ghostCursor.Hesitate,
            WaitForClick = waitForClick ?? ghostCursor.WaitForClick,
            MoveSpeed = ghostCursor.MoveSpeed,
            MoveDelay = ghostCursor.MoveDelay,
            RandomizeMoveDelay = ghostCursor.RandomizeMoveDelay,
            DelayPerStep = ghostCursor.DelayPerStep,
            ScrollSpeed = ghostCursor.ScrollSpeed,
            ScrollDelay = ghostCursor.ScrollDelay,
            MaxTries = ghostCursor.MaxTries,
            OvershootThreshold = ghostCursor.OvershootThreshold
        };
    }

    public static ScrollOptions BuildScrollOptions(BrowserSettings settings)
    {
        var ghostCursor = settings.GhostCursor;

        return new()
        {
            ScrollSpeed = ghostCursor.ScrollSpeed,
            ScrollDelay = ghostCursor.ScrollDelay
        };
    }

    public static ScrollIntoViewOptions BuildScrollIntoViewOptions(BrowserSettings settings)
    {
        var ghostCursor = settings.GhostCursor;

        return new()
        {
            ScrollSpeed = ghostCursor.ScrollSpeed,
            ScrollDelay = ghostCursor.ScrollDelay
        };
    }
}
