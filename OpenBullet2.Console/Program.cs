using CommandLine;
using CommandLine.Text;
using RuriLib.Helpers;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Hits;
using RuriLib.Models.Hits.HitOutputs;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;
using RuriLib.Services;
using RuriLib.Parallelization.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RuriLib.Models.Bots;
using System.Threading.Tasks;
using Spectre.Console;

namespace OpenBullet2.Console
{
    class Program
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         *                                                                                                                           *
         *  THIS IS A POC (Proof Of Concept) IMPLEMENTATION OF RuriLib IN CLI (Command Line Interface).                              *
         *  The functionalities supported here don't even come close to the ones of the main implementation.                         *
         *  Feel free to contribute to the versatility of this project by adding the missing functionalities and submitting a PR.    *
         *                                                                                                                           *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        class Options
        {
            [Option('c', "config", Required = true, HelpText = "Configuration file to be processed.")]
            public string ConfigFile { get; set; }

            [Option('w', "wordlist", Required = true, HelpText = "Wordlist file to be processed.")]
            public string WordlistFile { get; set; }

            [Option("wltype", Required = true, HelpText = "Type of the wordlist loaded (see Environment.ini for all allowed types).")]
            public string WordlistType { get; set; }

            [Option('p', "proxies", Default = null, HelpText = "Proxy file to be processed.")]
            public string ProxyFile { get; set; }

            [Option("ptype", Default = ProxyType.Http, HelpText = "Type of proxies loaded (Http, Socks4, Socks5).")]
            public ProxyType ProxyType { get; set; }

            [Option("pmode", Default = JobProxyMode.Default, HelpText = "The proxy mode (On, Off, Default).")]
            public JobProxyMode ProxyMode { get; set; }

            [Option('s', "skip", Default = 1, HelpText = "Number of lines to skip in the Wordlist.")]
            public int Skip { get; set; }

            [Option('b', "bots", Default = 0, HelpText = "Number of concurrent bots working. If not specified, the config default will be used.")]
            public int BotsNumber { get; set; }

            [Option('v', "verbose", Default = false, HelpText = "Prints fails and task errors.")]
            public bool Verbose { get; set; }

            [Usage(ApplicationAlias = "dotnet OpenBullet2.Console.dll")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    return new List<Example>() {
                        new Example("Simple POC CLI Implementation of RuriLib that executes a MultiRunJob.",
                            new Options {
                                ConfigFile = "config.opk",
                                WordlistFile = "rockyou.txt",
                                WordlistType = "Default",
                                ProxyFile = "proxies.txt",
                                ProxyType = ProxyType.Http,
                                ProxyMode = JobProxyMode.Default,
                                Skip = 1,
                                BotsNumber = 1,
                                Verbose = false
                            }
                        )
                    };
                }
            }
        }

        private static MultiRunJob job;
        private static Options options;
        private static bool completed;

        static async Task Main(string[] args)
        {
            ThreadPool.SetMinThreads(1000, 1000);

            System.Console.Title = "OpenBullet 2 (Console POC)";
            System.Console.WriteLine(@"
This is a POC (Proof of Concept) implementation of RuriLib as a console application.
The functionalities supported here don't even come close to the ones of the main implementation.
Feel free to contribute to the versatility of this project by adding the missing functionalities and submitting a PR.
");

            // Parse the Options
            await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(RunAsync);
        }

        private static async Task RunAsync(Options opts)
        {
            options = opts;

            var rlSettings = new RuriLibSettingsService("UserData");
            var pluginRepo = new PluginRepository("UserData/Plugins");

            // Unpack the config
            using var fs = new FileStream(opts.ConfigFile, FileMode.Open);
            var config = ConfigPacker.Unpack(fs).Result;

            // Setup the job
            job = new MultiRunJob(rlSettings, pluginRepo)
            {
                Config = config,
                CreationTime = DateTime.Now,
                ProxyMode = opts.ProxyMode,
                ProxySources = new List<ProxySource> { new FileProxySource(opts.ProxyFile) { DefaultType = opts.ProxyType } },
                Providers = new Providers(rlSettings),
                Bots = opts.BotsNumber,
                DataPool = new FileDataPool(opts.WordlistFile, opts.WordlistType),
                HitOutputs = new List<IHitOutput> { new FileSystemHitOutput("UserData/Hits") },
                BotLimit = opts.BotsNumber,
                CurrentBotDatas = new BotData[opts.BotsNumber]
            };

            // Ask custom inputs (if any)
            foreach (var input in config.Settings.InputSettings.CustomInputs)
            {
                var answer = AnsiConsole.Ask<string>($"{input.Description} ({input.DefaultAnswer})"); ;
                job.CustomInputsAnswers[input.VariableName] = string.IsNullOrWhiteSpace(answer)
                    ? input.DefaultAnswer
                    : answer;
            }

            // Hook event handlers
            job.OnCompleted += (sender, args) => completed = true;
            job.OnResult += PrintResult;
            job.OnTaskError += PrintTaskError;
            job.OnError += (sender, ex) => AnsiConsole.WriteException(ex);

            var consoleManager = new ConsoleManager(job);

            // Start the job
            await job.Start();

            _ = consoleManager.StartUpdatingTitleAsync();

            _ = consoleManager.StartListeningKeysAsync();

            // Wait until it finished
            while (!completed)
            {
                await Task.Delay(1000);
            }

            AnsiConsole.MarkupLine($"[red3]aborted at {DateTime.Now}[/]");

            // Print colored finish message
            //AnsiConsole.MarkupLine($"Finished. Found: [green4]{job.DataHits} hits[/], [orange3]{job.DataCustom} custom[/], [cyan3]{job.DataToCheck} to check[/].");

            // Prevent console from closing until the user presses return, then close
            System.Console.ReadLine();
            Environment.Exit(0);
        }

        private static void PrintResult(object sender, ResultDetails<MultiRunInput, CheckResult> details)
        {
            var botData = details.Result.BotData;
            var data = botData.Line.Data;

            if (botData.STATUS == "FAIL")
                return;

            switch (botData.STATUS)
            {
                case "SUCCESS":
                    AnsiConsole.MarkupLine($"[green4]{botData.STATUS}:[/] {data}");
                    break;
                case "BAN":
                    AnsiConsole.MarkupLine($"[plum3]{botData.STATUS}:[/] {data}");
                    break;
                case "RETRY":
                    AnsiConsole.MarkupLine($"[yellow3_1]{botData.STATUS}:[/] {data}");
                    break;
                case "ERROR":
                    AnsiConsole.MarkupLine($"[red]{botData.STATUS}:[/] {data}");
                    break;
                default:
                    AnsiConsole.MarkupLine($"[orange3]{botData.STATUS}:[/] {data}");
                    break;
            }
        }

        private static void PrintTaskError(object sender, ErrorDetails<MultiRunInput> details)
        {
            if (!options.Verbose) return;

            var proxy = details.Item.BotData.Proxy;
            var data = details.Item.BotData.Line.Data;
            System.Console.WriteLine($"Task Error: ({proxy})({data})! {details.Exception.Message}");
        }
    }
}
