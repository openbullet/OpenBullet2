using System;
using System.Threading.Tasks;
using RuriLib.Models.Bots;

namespace RuriLib.Blocks.Puppeteer.Browser;

/// <summary>
/// Compatibility wrappers for legacy Puppeteer browser methods.
/// </summary>
[Obsolete("Use the generic Browser blocks instead. This compatibility API is kept for existing compiled configs.")]
public static class Methods
{
    /// <summary>
    /// Compatibility wrapper for opening a browser instance.
    /// </summary>
    public static Task PuppeteerOpenBrowser(BotData data, string extraCmdLineArgs = "")
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserOpen(data, extraCmdLineArgs);

    /// <summary>
    /// Compatibility wrapper for closing the current browser instance.
    /// </summary>
    public static Task PuppeteerCloseBrowser(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserClose(data);

    /// <summary>
    /// Compatibility wrapper for opening a new tab.
    /// </summary>
    public static Task PuppeteerNewTab(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserNewTab(data);

    /// <summary>
    /// Compatibility wrapper for closing the active tab.
    /// </summary>
    public static Task PuppeteerCloseTab(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserCloseTab(data);

    /// <summary>
    /// Compatibility wrapper for switching tabs by index.
    /// </summary>
    public static Task PuppeteerSwitchToTab(BotData data, int index)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserSwitchToTab(data, index);

    /// <summary>
    /// Compatibility wrapper for reloading the current page.
    /// </summary>
    public static Task PuppeteerReload(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserReload(data);

    /// <summary>
    /// Compatibility wrapper for navigating back in history.
    /// </summary>
    public static Task PuppeteerGoBack(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserGoBack(data);

    /// <summary>
    /// Compatibility wrapper for navigating forward in history.
    /// </summary>
    public static Task PuppeteerGoForward(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserGoForward(data);
}
