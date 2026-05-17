using CommandLine;
using RuriLib.Helpers;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Data;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Debugger;
using RuriLib.Models.Hits;
using RuriLib.Models.Hits.HitOutputs;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies.ProxySources;
using RuriLib.Models.Variables;
using RuriLib.Parallelization.Models;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Services;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace OpenBullet2.Console;

internal class Program
{
    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     *                                                                                                                           *
     *  THIS IS A POC (Proof Of Concept) IMPLEMENTATION OF RuriLib IN CLI (Command Line Interface).                              *
     *  The functionalities supported here don't even come close to the ones of the main implementation.                         *
     *  Feel free to contribute to the versatility of this project by adding the missing functionalities and submitting a PR.    *
     *                                                                                                                           *
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

    private static MultiRunJob? job;
    private static ConsoleOptions? options;
    private static bool completed;

    private static int Main(string[] args)
    {
        ThreadPool.SetMinThreads(1000, 1000);

        System.Console.Title = "OpenBullet 2 (Console POC)";
        AnsiConsole.Write(new Text(@"
This is a POC (Proof of Concept) implementation of RuriLib as a console application.
The functionalities supported here don't even come close to the ones of the main implementation.
Feel free to contribute to the versatility of this project by adding the missing functionalities and submitting a PR.
", Style.Plain));

        var exitCode = 0;

        CommandLine.Parser.Default.ParseArguments<ConsoleOptions>(args)
            .WithParsed(opts => exitCode = Run(opts))
            .WithNotParsed(_ => exitCode = 1);

        return exitCode;
    }

    private static int Run(ConsoleOptions opts)
    {
        options = opts;
        completed = false;
        var rlSettings = new RuriLibSettingsService("UserData");
        rlSettings.RuriLibSettings.GeneralSettings.VerboseMode = opts.Verbose;

        var validationErrors = ConsoleRunPlanner.Validate(opts);
        if (validationErrors.Count > 0)
        {
            foreach (var error in validationErrors)
            {
                WriteLine($"Argument error: {error}", "#ff6347");
            }

            WriteLine("Use --help to see the supported options.", "#ff6347");
            return 1;
        }

        var pluginRepo = new PluginRepository("UserData/Plugins");

        using var fs = new FileStream(opts.ConfigFile, FileMode.Open);
        var config = ConfigPacker.UnpackAsync(fs).Result;

        var customInputsAnswers = AskCustomInputs(config);

        if (ConsoleRunPlanner.GetRunMode(opts) == ConsoleRunMode.SingleRunDebug)
        {
            RunSingleDebugSession(config, rlSettings, pluginRepo, opts);
            WaitForExit();
            return 0;
        }

        RunMultiRunJob(config, rlSettings, pluginRepo, opts, customInputsAnswers);
        WaitForExit();
        return 0;
    }

    private static void RunSingleDebugSession(RuriLib.Models.Configs.Config config,
        RuriLibSettingsService rlSettings, PluginRepository pluginRepo, ConsoleOptions opts)
    {
        var debuggerOptions = ConsoleRunPlanner.BuildDebuggerOptions(opts);

        using var debugger = new ConfigDebugger(config, debuggerOptions)
        {
            RuriLibSettings = rlSettings,
            PluginRepo = pluginRepo,
            RNGProvider = new DefaultRNGProvider()
        };

        debugger.NewLogEntry += PrintDebuggerLog;
        debugger.StatusChanged += (_, status) =>
        {
            if (status != ConfigDebuggerStatus.WaitingForStep)
            {
                return;
            }

            WriteStepPromptPanel(config);
            System.Console.ReadLine();
            debugger.TryTakeStep();
        };

        debugger.Run().GetAwaiter().GetResult();
        RenderDebuggerVariableRecap(debugger.Options.Variables);

        WriteLine("Single-run debug session completed.", "#7fffd4");
    }

    private static void RunMultiRunJob(RuriLib.Models.Configs.Config config,
        RuriLibSettingsService rlSettings, PluginRepository pluginRepo, ConsoleOptions opts,
        Dictionary<string, string> customInputsAnswers)
    {
        var effectiveBots = ConsoleRunPlanner.ResolveBots(
            opts.BotsNumber,
            config.Settings.GeneralSettings.SuggestedBots);
        var dataPool = BuildDataPool(opts);

        job = new MultiRunJob(rlSettings, pluginRepo, opts.Verbose ? new ConsoleJobLogger() : null)
        {
            Config = config,
            CreationTime = DateTime.Now,
            ProxyMode = opts.ProxyMode,
            ProxySources = string.IsNullOrEmpty(opts.ProxyFile)
                ? []
                : [new FileProxySource(opts.ProxyFile) { DefaultType = opts.ProxyType }],
            Providers = new Providers(rlSettings),
            Bots = effectiveBots,
            DataPool = dataPool,
            HitOutputs = [new FileSystemHitOutput("UserData/Hits")],
            BotLimit = effectiveBots,
            Skip = opts.Skip,
            CustomInputsAnswers = customInputsAnswers,
            CurrentBotDatas = new BotData[effectiveBots]
        };

        job.OnCompleted += (_, _) => completed = true;
        job.OnResult += PrintResult;
        job.OnTaskError += PrintTaskError;
        job.OnError += (_, ex) => WriteLine($"Error: {ex.Message}", "#ff6347");

        job.Start().Wait();

        while (!completed)
        {
            Thread.Sleep(100);
            UpdateTitle();
        }

        System.Console.Write($"Finished. Found: ");
        Write($"{job.DataHits} hits, ", "#adff2f");
        Write($"{job.DataCustom} custom, ", "#ff8c00");
        WriteLine($"{job.DataToCheck} to check.", "#7fffd4");
    }

    private static DataPool BuildDataPool(ConsoleOptions opts)
    {
        if (string.IsNullOrEmpty(opts.WordlistFile) && !string.IsNullOrEmpty(opts.WordlistRange))
        {
            var splitRange = opts.WordlistRange.Split(',');
            return new RangeDataPool(
                start: Convert.ToInt64(splitRange[0]),
                amount: Convert.ToInt32(splitRange[1]),
                step: splitRange.Length > 2 ? Convert.ToInt32(splitRange[2]) : default,
                pad: splitRange.Length > 3 && Convert.ToBoolean(splitRange[3]),
                opts.WordlistType);
        }

        if (string.IsNullOrEmpty(opts.WordlistFile))
        {
            throw new ArgumentException("A wordlist file must be provided when no wordlist range is specified.");
        }

        return new FileDataPool(opts.WordlistFile, opts.WordlistType);
    }

    private static Dictionary<string, string> AskCustomInputs(RuriLib.Models.Configs.Config config)
    {
        var answers = new Dictionary<string, string>();

        foreach (var input in config.Settings.InputSettings.CustomInputs)
        {
            var prompt = new TextPrompt<string>($"{input.Description} ({input.DefaultAnswer}):")
                .AllowEmpty();

            var answer = AnsiConsole.Prompt(prompt);

            if (!string.IsNullOrWhiteSpace(answer))
            {
                input.DefaultAnswer = answer;
            }

            answers[input.VariableName] = input.DefaultAnswer;
        }

        return answers;
    }

    private static void PrintDebuggerLog(object? sender, BotLoggerEntry entry)
    {
        if (string.IsNullOrEmpty(entry.Message))
        {
            AnsiConsole.WriteLine();
            return;
        }

        WriteLine(entry.Message, entry.Color);
    }

    private static void Write(string text, string? htmlColor = null)
    {
        AnsiConsole.Write(new Text(text, new Style(foreground: ToSpectreColor(htmlColor))));
    }

    private static void WriteLine(string text, string? htmlColor = null)
    {
        Write(text, htmlColor);
        AnsiConsole.WriteLine();
    }

    private static void WriteStepPromptPanel(RuriLib.Models.Configs.Config config)
    {
        var configName = string.IsNullOrWhiteSpace(config.Metadata.Name)
            ? Path.GetFileNameWithoutExtension(options?.ConfigFile ?? config.Id)
            : config.Metadata.Name;

        var panelContent = new Markup(
            $"[deepskyblue1]Step-by-step debugger paused[/]{Environment.NewLine}" +
            $"[grey]Config:[/] [white]{Markup.Escape(configName)}[/]{Environment.NewLine}" +
            $"[yellow]Press ENTER to execute the next step[/]{Environment.NewLine}" +
            $"[grey]Use Ctrl+C to stop the session[/]");

        var panel = new Panel(panelContent)
        {
            Header = new PanelHeader("Waiting For Input"),
            Border = BoxBorder.Rounded,
            Expand = false
        };

        panel.BorderStyle = new Style(foreground: Spectre.Console.Color.Aqua);
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static void RenderDebuggerVariableRecap(IEnumerable<Variable> variables)
    {
        var orderedVariables = variables
            .OrderByDescending(v => v.MarkedForCapture)
            .ThenBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (orderedVariables.Count == 0)
        {
            return;
        }

        var captures = orderedVariables.Where(v => v.MarkedForCapture).ToList();
        var regularVariables = orderedVariables.Where(v => !v.MarkedForCapture).ToList();

        if (captures.Count > 0)
        {
            RenderVariablesTable("Captures", captures);
        }

        if (regularVariables.Count > 0)
        {
            RenderVariablesTable("Variables", regularVariables);
        }
    }

    private static void RenderVariablesTable(string title, List<Variable> variables)
    {
        var table = new Table
        {
            Border = TableBorder.Rounded,
            Expand = true
        };

        table.Title = new TableTitle($"[white]{Markup.Escape(title)}[/]");
        table.BorderColor(Spectre.Console.Color.White);
        table.AddColumn("[white]Name[/]");
        table.AddColumn("[white]Type[/]");
        table.AddColumn("[white]Value[/]");

        foreach (var variable in variables)
        {
            var nameColor = variable.MarkedForCapture ? "#ff6347" : "#ffd700";

            table.AddRow(
                $"[{nameColor}]{Markup.Escape(variable.Name)}[/]",
                $"[grey]{Markup.Escape(variable.Type.ToString())}[/]",
                $"[white]{Markup.Escape(variable.AsString())}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static Spectre.Console.Color ToSpectreColor(string? htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
        {
            return Spectre.Console.Color.Default;
        }

        var color = htmlColor.Trim().TrimStart('#');

        if (color.Length == 3)
        {
            color = string.Concat(color[0], color[0], color[1], color[1], color[2], color[2]);
        }

        if (color.Length != 6
            || !byte.TryParse(color.AsSpan(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var r)
            || !byte.TryParse(color.AsSpan(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g)
            || !byte.TryParse(color.AsSpan(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
        {
            return Spectre.Console.Color.Default;
        }

        return new Spectre.Console.Color(r, g, b);
    }

    private static void WaitForExit()
    {
        System.Console.WriteLine("Press ENTER to exit.");
        System.Console.ReadLine();
    }

    private static void PrintResult(object? sender, ResultDetails<MultiRunInput, CheckResult> details)
    {
        var botData = details.Result.BotData;
        var data = botData.Line.Data;

        if (botData.STATUS == "FAIL" && options?.Verbose != true)
        {
            return;
        }

        var color = botData.STATUS switch
        {
            "SUCCESS" => "#9acd32",
            "FAIL" => "#ff6347",
            "BAN" => "#dda0dd",
            "RETRY" => "#ffff00",
            "ERROR" => "#ff0000",
            "NONE" => "#87ceeb",
            _ => "#ffa500"
        };

        WriteLine($"{botData.STATUS}: {data}", color);
    }

    private static void PrintTaskError(object? sender, ErrorDetails<MultiRunInput> details)
    {
        if (options?.Verbose != true)
        {
            return;
        }

        var proxy = details.Item.BotData.Proxy;
        var data = details.Item.BotData.Line.Data;
        WriteLine($"Task Error: ({proxy})({data})! {details.Exception.Message}", "#ff6347");
    }

    private static void UpdateTitle()
    {
        try
        {
            if (job?.Config is null || options is null || job.DataPool is null)
            {
                return;
            }

            System.Console.Title = $"OpenBullet 2 (Console POC) - {job.Status} | " +
                                   $"Config: {job.Config.Metadata.Name} | " +
                                   $"Wordlist: {Path.GetFileName(options.WordlistFile ?? string.Empty)} | " +
                                   $"Bots: {job.Bots} | " +
                                   $"CPM: {job.CPM} | " +
                                   $"Progress: {job.DataTested} / {job.DataPool.Size} ({job.Progress * 100:0.00}%) | " +
                                   $"Hits: {job.DataHits} Custom: {job.DataCustom} ToCheck: {job.DataToCheck} Fails: {job.DataFails} Retries: {job.DataRetried + job.DataBanned} | " +
                                   $"Proxies: {job.ProxiesAlive} / {job.ProxiesTotal}";
        }
        catch (InvalidOperationException)
        {
            // The title reads some collections that might be updated concurrently by the job.
        }
    }
}
