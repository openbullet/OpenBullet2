using RuriLib.Logging;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Proxies;
using RuriLib.Models.Settings;
using RuriLib.Models.UserAgents;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace RuriLib.Models.Bots
{
    public class BotData
    {
        public DataLine Line { get; set; }
        public Proxy Proxy { get; set; }
        public bool UseProxy { get; set; }

        public CookieContainer CookieContainer { get; set; } = new CookieContainer();
        public GlobalSettings GlobalSettings { get; }
        public ConfigSettings ConfigSettings { get; }
        public IRandomUAProvider RandomUAProvider { get; }
        public IBotLogger Logger { get; set; }
        public Random Random { get; }
        public CancellationToken CancellationToken { get; set; }
        public decimal CaptchaCredit { get; set; }
        public string ExecutionInfo { get; set; } = "IDLE";

        // Fixed properties
        public string STATUS { get; set; } = "NONE";
        public string SOURCE { get; set; } = string.Empty;
        public byte[] RAWSOURCE { get; set; } = new byte[0];
        public string ADDRESS { get; set; } = string.Empty;
        public int RESPONSECODE { get; set; } = 0;
        public Dictionary<string, string> COOKIES { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> HEADERS { get; set; } = new Dictionary<string, string>();

        // This dictionary will hold stateful objects like a captcha provider, a TCP client, a selenium webdriver...
        public Dictionary<string, object> Objects { get; } = new Dictionary<string, object>();

        // This list will hold the names of all variables that are marked for capture
        public List<string> MarkedForCapture { get; } = new List<string>();

        public BotData(GlobalSettings globalSettings, ConfigSettings configSettings,
            IBotLogger logger, IRandomUAProvider randomUAProvider, Random random, DataLine line, Proxy proxy = null, bool useProxy = false)
        {
            GlobalSettings = globalSettings;
            ConfigSettings = configSettings;
            Logger = logger;
            RandomUAProvider = randomUAProvider;

            // Create a new local RNG seeded with a random seed from the global RNG
            // This is needed because when multiple threads try to access the same RNG it stops giving
            // random values after a while!
            Random = new Random(random.Next(0, int.MaxValue));

            Line = line;
            Proxy = proxy;
            UseProxy = useProxy;
        }

        public void MarkForCapture(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty");

            if (!MarkedForCapture.Contains(name))
                MarkedForCapture.Add(name);
        }

        public void ExecutingBlock(string label)
            => ExecutionInfo = $"Executing block {label}";
    }
}
