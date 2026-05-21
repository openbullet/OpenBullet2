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
    [Block("Opens a new browser", name = "Open Browser", aliases = ["PuppeteerOpenBrowser"])]
    public static Task BrowserOpen(BotData data, string extraCmdLineArgs = "")
        => data.Providers.BrowserAutomation.Resolve(data).OpenBrowser(data, extraCmdLineArgs);

    [Block("Closes an open browser", name = "Close Browser", aliases = ["PuppeteerCloseBrowser"])]
    public static Task BrowserClose(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).CloseBrowser(data);

    [Block("Opens a new page in a new browser tab", name = "New Tab", aliases = ["PuppeteerNewTab"])]
    public static Task BrowserNewTab(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).NewTab(data);

    [Block("Closes the currently active browser tab", name = "Close Tab", aliases = ["PuppeteerCloseTab"])]
    public static Task BrowserCloseTab(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).CloseTab(data);

    [Block("Switches to the browser tab with a specified index", name = "Switch to Tab", aliases = ["PuppeteerSwitchToTab"])]
    public static Task BrowserSwitchToTab(BotData data, int index)
        => data.Providers.BrowserAutomation.Resolve(data).SwitchToTab(data, index);

    [Block("Reloads the current page", name = "Reload", aliases = ["PuppeteerReload"])]
    public static Task BrowserReload(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).Reload(data);

    [Block("Goes back to the previously visited page", name = "Go Back", aliases = ["PuppeteerGoBack"])]
    public static Task BrowserGoBack(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).GoBack(data);

    [Block("Goes forward to the next visited page", name = "Go Forward", aliases = ["PuppeteerGoForward"])]
    public static Task BrowserGoForward(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).GoForward(data);
}
