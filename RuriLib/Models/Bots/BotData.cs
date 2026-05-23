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
using System.Threading.Tasks;

namespace RuriLib.Models.Bots;

/// <summary>
/// Holds the runtime state of a single bot execution.
/// </summary>
public class BotData
{
    /// <summary>
    /// Gets or sets the current input line assigned to the bot.
    /// </summary>
    public DataLine Line { get; set; }

    /// <summary>
    /// Gets or sets the proxy currently assigned to the bot.
    /// </summary>
    public Proxy? Proxy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the bot should use the assigned proxy.
    /// </summary>
    public bool UseProxy { get; set; }

    /// <summary>
    /// Gets the config settings used by the bot.
    /// </summary>
    public ConfigSettings ConfigSettings { get; }

    /// <summary>
    /// Gets the runtime providers available to the bot.
    /// </summary>
    public Providers Providers { get; }

    /// <summary>
    /// Gets or sets the logger used for bot output.
    /// </summary>
    public IBotLogger Logger { get; set; }

    /// <summary>
    /// Gets the random number generator reserved for this bot.
    /// </summary>
    public Random Random { get; }

    /// <summary>
    /// Gets or sets the cancellation token observed by the bot.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Gets or sets the asynchronous locker used by long-running blocks.
    /// </summary>
    public AsyncLocker? AsyncLocker { get; set; }

    /// <summary>
    /// Gets or sets the optional stepper used while debugging.
    /// </summary>
    public Stepper? Stepper { get; set; }

    /// <summary>
    /// Gets or sets the remaining captcha balance associated with the bot.
    /// </summary>
    public decimal CaptchaCredit { get; set; } = 0;

    /// <summary>
    /// Gets or sets a short description of the current execution state.
    /// </summary>
    public string ExecutionInfo { get; set; } = "IDLE";

    // Fixed properties
    /// <summary>
    /// Gets or sets the current bot status.
    /// </summary>
    public string STATUS { get; set; } = "NONE";

    /// <summary>
    /// Gets or sets the current textual response source.
    /// </summary>
    public string SOURCE { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current raw response source.
    /// </summary>
    public byte[] RAWSOURCE { get; set; } = [];

    /// <summary>
    /// Gets or sets the current request address.
    /// </summary>
    public string ADDRESS { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current response status code.
    /// </summary>
    public int RESPONSECODE { get; set; } = 0;

    /// <summary>
    /// Gets or sets the cookies captured during execution.
    /// </summary>
    public Dictionary<string, string> COOKIES { get; set; } = new();

    /// <summary>
    /// Gets or sets the headers captured during execution.
    /// </summary>
    public Dictionary<string, string> HEADERS { get; set; } = new();

    /// <summary>
    /// Gets or sets the current error message.
    /// </summary>
    public string ERROR { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the logical bot number in the running job.
    /// </summary>
    public int BOTNUM { get; set; } = 0;

    // This dictionary will hold stateful objects like a captcha provider, a TCP client, a selenium webdriver...
    private readonly Dictionary<string, object?> _objects = new();

    /// <summary>
    /// Gets the legacy object map used for backward compatibility.
    /// </summary>
    [Obsolete("Do not use this property, it's only here for retro compatibility but it can cause memory leaks." +
              " Use the SetObject and TryGetObject methods instead!")]
    public Dictionary<string, object> Objects => _objects.ToDictionary(kvp => kvp.Key, kvp => kvp.Value!);

    // This list will hold the names of all variables that are marked for capture
    /// <summary>
    /// Gets the variables that should be captured when they are assigned.
    /// </summary>
    public List<string> MarkedForCapture { get; } = new List<string>();

    /// <summary>
    /// Creates a new bot runtime state container.
    /// </summary>
    /// <param name="providers">The providers available to the bot.</param>
    /// <param name="configSettings">The config settings used by the bot.</param>
    /// <param name="logger">The logger used by the bot.</param>
    /// <param name="line">The current input line.</param>
    /// <param name="proxy">The optional proxy assigned to the bot.</param>
    /// <param name="useProxy">Whether the assigned proxy should be used.</param>
    public BotData(Providers providers, ConfigSettings configSettings, IBotLogger logger,
        DataLine line, Proxy? proxy = null, bool useProxy = false)
    {
        Providers = providers ?? throw new ArgumentNullException(nameof(providers));
        ConfigSettings = configSettings ?? throw new ArgumentNullException(nameof(configSettings));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Create a new local RNG seeded with a random seed from the global RNG
        // This is needed because when multiple threads try to access the same RNG it stops giving
        // random values after a while!
        Random = providers.RNG.GetNew();

        Line = line ?? throw new ArgumentNullException(nameof(line));
        Proxy = proxy;
        UseProxy = useProxy;
    }

    /// <summary>
    /// Logs that a variable assignment has occurred.
    /// </summary>
    /// <param name="name">The assigned variable name.</param>
    public void LogVariableAssignment(string name)
        => Logger.Log($"Assigned value to variable '{name}'", LogColors.Yellow);

    /// <summary>
    /// Marks a variable so that future assignments are captured.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public void MarkForCapture(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty");
        }

        if (MarkedForCapture.Contains(name))
        {
            return;
        }

        MarkedForCapture.Add(name);
        Logger.Log($"Variable '{name}' marked for capture", LogColors.Tomato);
    }

    /// <summary>
    /// Removes a variable from the capture list.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public void UnmarkCapture(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty");

        if (!MarkedForCapture.Contains(name))
        {
            return;
        }

        MarkedForCapture.Remove(name);
        Logger.Log($"Variable '{name}' removed from capture", LogColors.Yellow);
    }

    /// <summary>
    /// Updates the execution info to indicate which block is running.
    /// </summary>
    /// <param name="label">The label of the executing block.</param>
    public void ExecutingBlock(string label)
    {
        ExecutionInfo = $"Executing block {label}";

        if (Logger != null)
        {
            Logger.ExecutingBlock = label;
        }
    }

    /// <summary>
    /// Resets transient bot state before a retry.
    /// </summary>
    public void ResetState()
    {
        ExecutionInfo = "Retrying";
        STATUS = "NONE";
        SOURCE = string.Empty;
        RAWSOURCE = [];
        ADDRESS = string.Empty;
        ERROR = string.Empty;
        RESPONSECODE = 0;
        COOKIES.Clear();
        HEADERS.Clear();
        MarkedForCapture.Clear();

        // We need to dispose of objects created in each retry, because jobs should
        // only dispose of them after the bot has completed its work
        DisposeObjectsExcept(
        [
            "puppeteer",
            "puppeteerPage",
            "puppeteerFrame",
            "puppeteerGhostCursor",
            "playwright",
            "playwrightBrowser",
            "playwrightContext",
            "playwrightPage",
            "playwrightFrame",
            "playwrightGhostCursor",
            "playwrightUserAgent",
            "browserGhostCursorRandomMovesEnabled",
            "httpClient",
            "ironPyEngine",
            "pythonRuntime"
        ]);
    }

    /// <summary>
    /// Stores or replaces a named runtime object.
    /// </summary>
    /// <param name="name">The object name.</param>
    /// <param name="obj">The object instance to store.</param>
    /// <param name="disposeExisting">Whether an existing disposable instance should be disposed first.</param>
    public void SetObject(string name, object? obj, bool disposeExisting = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (_objects.TryGetValue(name, out var existing))
        {
            if (existing is IDisposable d && disposeExisting)
            {
                d.Dispose();
            }
        }

        _objects[name] = obj;
    }

    /// <summary>
    /// Tries to retrieve a named runtime object of the requested type.
    /// </summary>
    /// <typeparam name="T">The expected object type.</typeparam>
    /// <param name="name">The object name.</param>
    /// <returns>The stored object if it exists and matches the requested type; otherwise <see langword="null"/>.</returns>
    public T? TryGetObject<T>(string name) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _objects.TryGetValue(name, out var value) && value is T t ? t : null;
    }

    /// <summary>
    /// Disposes a tracked runtime object and removes it from the object map.
    /// </summary>
    /// <param name="name">The object key.</param>
    public async ValueTask DisposeObjectAsync(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_objects.TryGetValue(name, out var value))
        {
            return;
        }

        try
        {
            switch (value)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
        catch
        {
            // ignored
        }

        _objects.Remove(name);
    }

    /// <summary>
    /// Disposes all tracked runtime objects except the specified keys.
    /// </summary>
    /// <param name="except">The object keys to preserve.</param>
    public void DisposeObjectsExcept(string[]? except = null)
    {
        except ??= [];

        foreach (var obj in _objects.Where(o => !except.Contains(o.Key)).ToList())
        {
            try
            {
                (obj.Value as IDisposable)?.Dispose();
            }
            catch
            {
                // ignored
            }

            _objects.Remove(obj.Key);
        }
    }
}
