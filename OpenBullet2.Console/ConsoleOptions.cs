using CommandLine;
using CommandLine.Text;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using System.Collections.Generic;

namespace OpenBullet2.Console;

internal class ConsoleOptions
{
    [Option('c', "config", Required = true, HelpText = "Configuration file to be processed.")]
    public string ConfigFile { get; set; } = string.Empty;

    [Option('w', "wordlist", Required = false, HelpText = "Wordlist file to be processed.")]
    public string? WordlistFile { get; set; }

    [Option("wordlist-range", Required = false, HelpText = "Wordlist range to be processed, syntax: start,amount,step,pad(True or False). Only start and amount are necessary.")]
    public string? WordlistRange { get; set; }

    [Option("wltype", Required = true, HelpText = "Type of the wordlist loaded (see Environment.ini for all allowed types).")]
    public string WordlistType { get; set; } = string.Empty;

    [Option("data", Required = false, HelpText = "Single test data to run once. When provided, the console runs a single debug session instead of a MultiRunJob.")]
    public string? SingleRunData { get; set; }

    [Option('p', "proxies", Default = null, HelpText = "Proxy file to be processed.")]
    public string? ProxyFile { get; set; }

    [Option("proxy", Required = false, HelpText = "Single proxy to use in single-run mode.")]
    public string? SingleRunProxy { get; set; }

    [Option("ptype", Default = ProxyType.Http, HelpText = "Type of proxies loaded (Http, Socks4, Socks5).")]
    public ProxyType ProxyType { get; set; }

    [Option("pmode", Default = JobProxyMode.Default, HelpText = "The proxy mode (On, Off, Default).")]
    public JobProxyMode ProxyMode { get; set; }

    [Option('s', "skip", Default = 1, HelpText = "Number of lines to skip in the Wordlist.")]
    public int Skip { get; set; }

    [Option('b', "bots", Default = 0, HelpText = "Number of concurrent bots working. If not specified, the config default will be used.")]
    public int BotsNumber { get; set; }

    [Option("step", Default = false, HelpText = "In single-run mode, waits for ENTER before executing each step.")]
    public bool StepByStep { get; set; }

    [Option('v', "verbose", Default = false, HelpText = "Prints fails and task errors.")]
    public bool Verbose { get; set; }

    [Usage(ApplicationAlias = "dotnet OpenBullet2.Console.dll")]
    public static IEnumerable<Example> Examples => new List<Example>
    {
        new("Simple POC CLI implementation of RuriLib that executes a MultiRunJob.",
            new ConsoleOptions
            {
                ConfigFile = "config.opk",
                WordlistFile = "rockyou.txt",
                WordlistType = "Default",
                ProxyFile = "proxies.txt",
                ProxyType = ProxyType.Http,
                ProxyMode = JobProxyMode.Default,
                Skip = 1,
                BotsNumber = 1,
                Verbose = false
            }),
        new("Runs a single debug session against one input.",
            new ConsoleOptions
            {
                ConfigFile = "config.opk",
                SingleRunData = "12345",
                SingleRunProxy = "127.0.0.1:8080",
                WordlistType = "Default",
                ProxyType = ProxyType.Http,
                StepByStep = false
            })
    };
}
