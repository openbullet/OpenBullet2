using System;
using System.Collections.Generic;

namespace RuriLib.Models.Configs.Settings;

/// <summary>
/// Configures browser-based execution.
/// </summary>
public class BrowserSettings
{
    /// <summary>
    /// The browser automation engine that should execute generic browser blocks.
    /// </summary>
    public BrowserAutomationEngine Engine { get; set; } = BrowserAutomationEngine.Puppeteer;

    /// <summary>
    /// Statuses that should force the browser to close.
    /// </summary>
    public string[] QuitBrowserStatuses { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether the browser should run headless.
    /// </summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    /// Additional browser command-line arguments.
    /// </summary>
    public string CommandLineArgs { get; set; } = "--disable-notifications";

    /// <summary>
    /// Whether HTTPS certificate errors should be ignored.
    /// </summary>
    public bool IgnoreHttpsErrors { get; set; }

    /// <summary>
    /// Whether only document and script requests should be loaded.
    /// </summary>
    public bool LoadOnlyDocumentAndScript { get; set; }

    /// <summary>
    /// Whether JavaScript dialogs should be dismissed automatically.
    /// </summary>
    public bool DismissDialogs { get; set; }

    /// <summary>
    /// URL patterns that should be blocked.
    /// </summary>
    public List<string> BlockedUrls { get; set; } = [];
}
