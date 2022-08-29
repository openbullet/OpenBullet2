using AngleSharp.Common;
using AngleSharp.Text;
using RuriLib.Helpers;
using RuriLib.Logging;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RuriLib.Models.Bots
{
    public class BotData
    {
        public DataLine Line { get; set; }
        public Proxy Proxy { get; set; }
        public bool UseProxy { get; set; }

        public ConfigSettings ConfigSettings { get; }
        public Providers Providers { get; }
        public IBotLogger Logger { get; set; }
        public Random Random { get; }
        public CancellationToken CancellationToken { get; set; }
        public AsyncLocker AsyncLocker { get; set; }
        public Stepper Stepper { get; set; }
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
        public string ERROR { get; set; } = string.Empty;
        public int BOTNUM { get; set; } = 0;

        // This dictionary will hold stateful objects like a captcha provider, a TCP client, a selenium webdriver...
        private readonly Dictionary<string, object> objects = new Dictionary<string, object>();
        
        [Obsolete("Do not use this property, it's only here for retro compatibility but it can cause memory leaks." +
            " Use the SetObject and TryGetObject methods instead!")]
        public Dictionary<string, object> Objects => objects;

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
        }

        public void LogVariableAssignment(string name)
            => Logger.Log($"Assigned value to variable '{name}'", LogColors.Yellow);

        public void MarkForCapture(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty");

            if (!MarkedForCapture.Contains(name))
            {
                MarkedForCapture.Add(name);
                Logger.Log($"Variable '{name}' marked for capture", LogColors.Tomato);
            }
        }

        public void UnmarkCapture(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty");

            if (MarkedForCapture.Contains(name))
            {
                MarkedForCapture.Remove(name);
                Logger.Log($"Variable '{name}' removed from capture", LogColors.Yellow);
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

        public void ResetState()
        {
            ExecutionInfo = "Retrying";
            STATUS = "NONE";
            SOURCE = string.Empty;
            RAWSOURCE = Array.Empty<byte>();
            ADDRESS = string.Empty;
            ERROR = string.Empty;
            RESPONSECODE = 0;
            COOKIES.Clear();
            HEADERS.Clear();
            MarkedForCapture.Clear();

            // We need to dispose of objects created in each retry, because jobs should
            // only dispose of them after the bot has completed its work
            DisposeObjectsExcept(new[] { "puppeteer", "puppeteerPage", "puppeteerFrame", "httpClient", "ironPyEngine" });
        }

        public void SetObject(string name, object obj, bool disposeExisting = true)
        {
            if (objects.ContainsKey(name))
            {
                var existing = objects[name];

                if (existing is IDisposable d && disposeExisting)
                {
                    d.Dispose();
                }
            }

            objects[name] = obj;
        }

        public T TryGetObject<T>(string name) where T : class 
            => objects.ContainsKey(name) && objects[name] is T t ? t : null;

        public void DisposeObjectsExcept(string[] except = null)
        {
            except ??= Array.Empty<string>();

            foreach (var obj in objects.Where(o => o.Value is IDisposable && !except.Contains(o.Key)))
            {
                try
                {
                    (obj.Value as IDisposable).Dispose();
                }
                catch
                {

                }
            }
        }
    }
}
