using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using RuriLib.Logging;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Data.Resources;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace RuriLib.Models.Bots
{
    public class BotData : IDisposable
    {
        public DataLine Line { get; set; }
        public Proxy Proxy { get; set; }
        public bool UseProxy { get; set; }

        public ConfigSettings ConfigSettings { get; }
        public Dictionary<string, ConfigResource> Resources { get; }
        public Providers Providers { get; }
        public IBotLogger Logger { get; set; }
        public Random Random { get; }
        public CancellationToken CancellationToken { get; set; }
        public decimal CaptchaCredit { get; set; } = 0;
        public string ExecutionInfo { get; set; } = "IDLE";

        // Fixed properties
        public string STATUS { get; set; } = "NONE";
        public string SOURCE { get; set; } = string.Empty;
        public byte[] RAWSOURCE { get; set; } = Array.Empty<byte>();
        public string ADDRESS { get; set; } = string.Empty;
        public int RESPONSECODE { get; set; } = 0;
        public Dictionary<string, string> COOKIES { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> HEADERS { get; set; } = new Dictionary<string, string>();

        // This dictionary will hold stateful objects like a captcha provider, a TCP client, a selenium webdriver...
        public Dictionary<string, object> Objects { get; } = new Dictionary<string, object>();

        // This list will hold the names of all variables that are marked for capture
        public List<string> MarkedForCapture { get; } = new List<string>();

        public BotData(Providers providers, ConfigSettings configSettings, IBotLogger logger,
            DataLine line, Proxy proxy = null, bool useProxy = false)
        {
            Providers = providers;
            ConfigSettings = configSettings;
            Logger = logger;

            // Create a new local RNG seeded with a random seed from the global RNG
            // This is needed because when multiple threads try to access the same RNG it stops giving
            // random values after a while!
            Random = providers.RNG.GetNew();

            Line = line;
            Proxy = proxy;
            UseProxy = useProxy;

            Objects.Add("httpClient", new HttpClient());
            var runtime = Python.CreateRuntime();
            var pyengine = runtime.GetEngine("py");
            var pco = (PythonCompilerOptions)pyengine.GetCompilerOptions();
            pco.Module &= ~ModuleOptions.Optimized;
            Objects.Add("ironPyEngine", pyengine);

            Resources = new();

            // Resources will need to be disposed of
            foreach (var resource in configSettings.DataSettings.Resources)
            {
                Resources[resource.Name] = resource switch
                {
                    LinesFromFileResourceOptions opt => new LinesFromFileResource(opt),
                    RandomLinesFromFileResourceOptions opt => new RandomLinesFromFileResource(opt),
                    _ => throw new NotImplementedException()
                };
            }
        }

        public void MarkForCapture(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty");

            if (!MarkedForCapture.Contains(name))
            {
                MarkedForCapture.Add(name);
                Logger.Log($"Variable {name} marked for capture", LogColors.Tomato);
            }
        }

        public void ExecutingBlock(string label)
        {
            ExecutionInfo = $"Executing block {label}";
            
            if (Logger != null)
            {
                Logger.ExecutingBlock = label;
            }
        }

        public void Dispose()
        {
            // Dispose all disposable objects
            foreach (var obj in Objects.Where(o => o.Value is IDisposable))
            {
                if (obj.Key.Contains("puppeteer"))
                    continue;

                try
                {
                    (obj.Value as IDisposable).Dispose();
                }
                catch
                {

                }
            }

            // Dispose resources
            foreach (var resource in Resources.Where(r => r.Value is IDisposable))
            {
                try
                {
                    (resource.Value as IDisposable).Dispose();
                }
                catch
                {

                }
            }
        }
    }
}
