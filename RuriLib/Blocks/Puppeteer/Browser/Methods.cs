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
    public static Task PuppeteerOpenBrowser(BotData data, string extraCmdLineArgs = "")
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserOpen(data, extraCmdLineArgs);

    public static Task PuppeteerCloseBrowser(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserClose(data);

    public static Task PuppeteerNewTab(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserNewTab(data);

    public static Task PuppeteerCloseTab(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserCloseTab(data);

    public static Task PuppeteerSwitchToTab(BotData data, int index)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserSwitchToTab(data, index);

    public static Task PuppeteerReload(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserReload(data);

    public static Task PuppeteerGoBack(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserGoBack(data);

    public static Task PuppeteerGoForward(BotData data)
        => global::RuriLib.Blocks.Browser.Browser.Methods.BrowserGoForward(data);
}
