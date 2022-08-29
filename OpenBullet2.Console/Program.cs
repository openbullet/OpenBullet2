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
using System.Drawing;
using System.IO;
using System.Threading;
using RuriLib.Models.Bots;
using RuriLib.Models.Data;

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

            [Option('w', "wordlist", Required = false, HelpText = "Wordlist file to be processed.")]
            public string WordlistFile { get; set; }

            [Option("wordlist-range", Required = false, HelpText = "Wordlist Range to be processed, syntax: start,amount,step,pad(True or False) . Only start and amount are necessary.")]
            public string WordlistRange { get; set; }

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
        private static bool completed = false;

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1000, 1000);

            System.Console.Title = "OpenBullet 2 (Console POC)";
            System.Console.WriteLine(@"
This is a POC (Proof of Concept) implementation of RuriLib as a console application.
The functionalities supported here don't even come close to the ones of the main implementation.
Feel free to contribute to the versatility of this project by adding the missing functionalities and submitting a PR.
");

            // Parse the Options
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(opts => Run(opts))
              .WithNotParsed((errs) => { });
        }

        private static void Run(Options opts)
        {
            options = opts;

            var rlSettings = new RuriLibSettingsService("UserData");
            var pluginRepo = new PluginRepository("UserData/Plugins");

            // Unpack the config
            using var fs = new FileStream(opts.ConfigFile, FileMode.Open);
            var config = ConfigPacker.Unpack(fs).Result;

            DataPool dataPool;
            if (string.IsNullOrEmpty(opts.WordlistFile) && !string.IsNullOrEmpty(opts.WordlistRange))
            {
                string[] splitRange = opts.WordlistRange.Split(",");
                dataPool = new RangeDataPool(
                    start: Convert.ToInt64(splitRange[0]),
                    amount: Convert.ToInt32(splitRange[1]),
                    step: splitRange.Length > 2 ? Convert.ToInt32(splitRange[2]) : default,
                    pad: splitRange.Length > 3 ? Convert.ToBoolean(splitRange[3]) : default,
                    opts.WordlistType
                    );
            }
            else
            {
                dataPool = new FileDataPool(opts.WordlistFile, opts.WordlistType);
            }

            // Setup the job
            job = new MultiRunJob(rlSettings, pluginRepo)
            {
                Config = config,
                CreationTime = DateTime.Now,
                ProxyMode = opts.ProxyMode,
                ProxySources = new List<ProxySource> { new FileProxySource(opts.ProxyFile) { DefaultType = opts.ProxyType } },
                Providers = new Providers(rlSettings),
                Bots = opts.BotsNumber,
                DataPool = dataPool,
                HitOutputs = new List<IHitOutput> { new FileSystemHitOutput("UserData/Hits") },
                BotLimit = opts.BotsNumber,
                Skip = opts.Skip,
                CurrentBotDatas = new BotData[opts.BotsNumber]
            };
            
            // Ask custom inputs (if any)
            foreach (var input in config.Settings.InputSettings.CustomInputs)
            {
                System.Console.WriteLine($"{input.Description} ({input.DefaultAnswer}): ");
                var answer = System.Console.ReadLine();
                job.CustomInputsAnswers[input.VariableName] = string.IsNullOrWhiteSpace(answer)
                    ? input.DefaultAnswer
                    : answer;
            }

            // Hook event handlers
            job.OnCompleted += (sender, args) => completed = true;
            job.OnResult += PrintResult;
            job.OnTaskError += PrintTaskError;
            job.OnError += (sender, ex) => System.Console.WriteLine($"Error: {ex.Message}", Color.Tomato);

            // Start the job
            job.Start().Wait();

            // Wait until it finished
            while (!completed)
            {
                Thread.Sleep(100);
                UpdateTitle();
            }

            // Print colored finish message
            System.Console.Write($"Finished. Found: ");
            System.Console.Write($"{job.DataHits} hits, ", Color.GreenYellow);
            System.Console.Write($"{job.DataCustom} custom, ", Color.DarkOrange);
            System.Console.WriteLine($"{job.DataToCheck} to check.", Color.Aquamarine);

            // Prevent console from closing until the user presses return, then close
            System.Console.ReadLine();
            Environment.Exit(0);
        }

        private static void PrintResult(object sender, ResultDetails<MultiRunInput, CheckResult> details)
        {
            var botData = details.Result.BotData;
            var data = botData.Line.Data;

            if (botData.STATUS == "FAIL" && !options.Verbose)
                return;

            var color = botData.STATUS switch
            {
                "SUCCESS" => Color.YellowGreen,
                "FAIL" => Color.Tomato,
                "BAN" => Color.Plum,
                "RETRY" => Color.Yellow,
                "ERROR" => Color.Red,
                "NONE" => Color.SkyBlue,
                _ => Color.Orange
            };

            System.Console.WriteLine($"{botData.STATUS}: {data}", color);
        }

        private static void PrintTaskError(object sender, ErrorDetails<MultiRunInput> details)
        {
            if (!options.Verbose) return;

            var proxy = details.Item.BotData.Proxy;
            var data = details.Item.BotData.Line.Data;
            System.Console.WriteLine($"Task Error: ({proxy})({data})! {details.Exception.Message}", Color.Tomato);
        }

        private static void UpdateTitle()
        {
            try
            {
                System.Console.Title = $"OpenBullet 2 (Console POC) - {job.Status} | " +
                                       $"Config: {job.Config.Metadata.Name} | " +
                                       $"Wordlist: {Path.GetFileName(options.WordlistFile)} | " +
                                       $"Bots: {job.Bots} | " +
                                       $"CPM: {job.CPM} | " +
                                       $"Progress: {job.DataTested} / {job.DataPool.Size} ({job.Progress * 100:0.00}%) | " +
                                       $"Hits: {job.DataHits} Custom: {job.DataCustom} ToCheck: {job.DataToCheck} Fails: {job.DataFails} Retries: {job.DataRetried + job.DataBanned} | " +
                                       $"Proxies: {job.ProxiesAlive} / {job.ProxiesTotal}";
            }
            catch (System.InvalidOperationException)
            {
                /*
                    Unhandled exception. System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
                    at System.Collections.Generic.List`1.Enumerator.MoveNextRare()
                    at System.Linq.Enumerable.Count[TSource](IEnumerable`1 source, Func`2 predicate)
                    at OpenBullet2.Console.Program.UpdateTitle()
                    at OpenBullet2.Console.Program.Run(Options opts)
                    at OpenBullet2.Console.Program.<>c.<Main>b__4_0(Options opts)
                    at CommandLine.ParserResultExtensions.WithParsed[T](ParserResult`1 result, Action`1 action)
                    at OpenBullet2.Console.Program.Main(String[] args)
                 */
                // The exception above is not easily solvable.
                // For now we just simply ignore it.
            }
        }
    }
}