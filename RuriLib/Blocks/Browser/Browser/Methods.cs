using System.Threading.Tasks;
using RuriLib.Attributes;
using RuriLib.Models.Bots;

namespace RuriLib.Blocks.Browser.Browser;

/// <summary>
/// Blocks for interacting with a browser.
/// </summary>
[BlockCategory("Browser", "Blocks for interacting with a browser", "#e9967a")]
public static class Methods
{
    /// <summary>
    /// Opens a new browser instance.
    /// </summary>
    [Block("Opens a new browser", name = "Open Browser", aliases = ["PuppeteerOpenBrowser"])]
    public static Task BrowserOpen(BotData data, string extraCmdLineArgs = "")
        => data.Providers.BrowserAutomation.Resolve(data).OpenBrowser(data, extraCmdLineArgs);

    /// <summary>
    /// Closes the current browser instance.
    /// </summary>
    [Block("Closes an open browser", name = "Close Browser", aliases = ["PuppeteerCloseBrowser"])]
    public static Task BrowserClose(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).CloseBrowser(data);

    /// <summary>
    /// Opens a new tab in the current browser.
    /// </summary>
    [Block("Opens a new page in a new browser tab", name = "New Tab", aliases = ["PuppeteerNewTab"])]
    public static Task BrowserNewTab(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).NewTab(data);

    /// <summary>
    /// Closes the active browser tab.
    /// </summary>
    [Block("Closes the currently active browser tab", name = "Close Tab", aliases = ["PuppeteerCloseTab"])]
    public static Task BrowserCloseTab(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).CloseTab(data);

    /// <summary>
    /// Switches to a tab by index.
    /// </summary>
    [Block("Switches to the browser tab with a specified index", name = "Switch to Tab", aliases = ["PuppeteerSwitchToTab"])]
    public static Task BrowserSwitchToTab(BotData data, int index)
        => data.Providers.BrowserAutomation.Resolve(data).SwitchToTab(data, index);

    /// <summary>
    /// Reloads the current page.
    /// </summary>
    [Block("Reloads the current page", name = "Reload", aliases = ["PuppeteerReload"])]
    public static Task BrowserReload(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).Reload(data);

    /// <summary>
    /// Navigates back in the current tab history.
    /// </summary>
    [Block("Goes back to the previously visited page", name = "Go Back", aliases = ["PuppeteerGoBack"])]
    public static Task BrowserGoBack(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).GoBack(data);

    /// <summary>
    /// Navigates forward in the current tab history.
    /// </summary>
    [Block("Goes forward to the next visited page", name = "Go Forward", aliases = ["PuppeteerGoForward"])]
    public static Task BrowserGoForward(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).GoForward(data);

    /// <summary>
    /// Enables or disables background random mouse movement.
    /// </summary>
    [Block("Enables or disables background random mouse movement for the current browser page", name = "Toggle Random Mouse Moves")]
    public static Task BrowserToggleRandomMouseMoves(BotData data, bool enabled = true)
        => data.Providers.BrowserAutomation.Resolve(data).ToggleRandomMouseMoves(data, enabled);
}
