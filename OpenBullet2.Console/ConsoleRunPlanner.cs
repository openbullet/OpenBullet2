using RuriLib.Models.Debugger;
using System;
using System.Collections.Generic;

namespace OpenBullet2.Console;

internal static class ConsoleRunPlanner
{
    public static ConsoleRunMode GetRunMode(ConsoleOptions options)
        => string.IsNullOrWhiteSpace(options.SingleRunData)
            ? ConsoleRunMode.MultiRunJob
            : ConsoleRunMode.SingleRunDebug;

    public static IReadOnlyList<string> Validate(ConsoleOptions options)
    {
        var errors = new List<string>();

        var hasSingleRunData = !string.IsNullOrWhiteSpace(options.SingleRunData);
        var hasSingleRunOnlyOptions = !string.IsNullOrWhiteSpace(options.SingleRunProxy) || options.StepByStep;

        if (hasSingleRunOnlyOptions && !hasSingleRunData)
        {
            errors.Add("Single-run options require --data.");
        }

        if (GetRunMode(options) == ConsoleRunMode.SingleRunDebug)
        {
            if (!string.IsNullOrWhiteSpace(options.WordlistFile))
            {
                errors.Add("Single-run mode does not use --wordlist.");
            }

            if (!string.IsNullOrWhiteSpace(options.WordlistRange))
            {
                errors.Add("Single-run mode does not use --wordlist-range.");
            }

            if (!string.IsNullOrWhiteSpace(options.ProxyFile))
            {
                errors.Add("Single-run mode does not use --proxies. Use --proxy instead.");
            }

            if (options.Skip != 1)
            {
                errors.Add("Single-run mode does not use --skip.");
            }

            if (options.BotsNumber > 0)
            {
                errors.Add("Single-run mode does not use --bots.");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(options.WordlistFile)
                && string.IsNullOrWhiteSpace(options.WordlistRange))
            {
                errors.Add("A wordlist file must be provided when no wordlist range is specified.");
            }

            if (!string.IsNullOrWhiteSpace(options.SingleRunProxy))
            {
                errors.Add("Multi-run mode does not use --proxy. Use --proxies for a proxy file.");
            }

            if (options.StepByStep)
            {
                errors.Add("The --step option is only available in single-run mode.");
            }
        }

        return errors;
    }

    public static DebuggerOptions BuildDebuggerOptions(ConsoleOptions options)
        => new()
        {
            TestData = options.SingleRunData ?? string.Empty,
            WordlistType = options.WordlistType,
            UseProxy = !string.IsNullOrWhiteSpace(options.SingleRunProxy),
            TestProxy = options.SingleRunProxy ?? string.Empty,
            ProxyType = options.ProxyType,
            PersistLog = false,
            StepByStep = options.StepByStep
        };

    public static int ResolveBots(int requestedBots, int suggestedBots)
        => requestedBots > 0
            ? requestedBots
            : Math.Max(1, suggestedBots);
}
