using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PuppeteerSharp;
using RuriLib.Functions.Browser;
using RuriLib.Models.Bots;

namespace RuriLib.Blocks.Puppeteer.Page;

/// <summary>
/// Compatibility wrappers for legacy Puppeteer page methods.
/// </summary>
[Obsolete("Use the generic Browser blocks instead. This compatibility API is kept for existing compiled configs.")]
public static class Methods
{
    /// <summary>
    /// Compatibility wrapper for navigating the current page to a URL.
    /// </summary>
    public static Task PuppeteerNavigateTo(BotData data, string url = "https://example.com",
        WaitUntilNavigation loadedEvent = WaitUntilNavigation.Load, string referer = "", int timeout = 30000)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserNavigateTo(data, url, ToBrowserWaitUntilNavigation(loadedEvent), referer, timeout);

    /// <summary>
    /// Compatibility wrapper for waiting for navigation to complete.
    /// </summary>
    public static Task PuppeteerWaitForNavigation(BotData data, WaitUntilNavigation loadedEvent = WaitUntilNavigation.Load, int timeout = 30000)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserWaitForNavigation(data, ToBrowserWaitUntilNavigation(loadedEvent), timeout);

    /// <summary>
    /// Compatibility wrapper for clearing cookies for a website.
    /// </summary>
    public static Task PuppeteerClearCookies(BotData data, string website)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserClearCookies(data, website);

    /// <summary>
    /// Compatibility wrapper for typing text into the page.
    /// </summary>
    public static Task PuppeteerPageType(BotData data, string text)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserPageType(data, text);

    /// <summary>
    /// Compatibility wrapper for pressing and releasing a page key.
    /// </summary>
    public static Task PuppeteerPageKeyPress(BotData data, string key)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserPageKeyPress(data, key);

    /// <summary>
    /// Compatibility wrapper for clicking at page coordinates.
    /// </summary>
    public static Task PuppeteerClickAtCoordinates(BotData data, int x, int y,
        PuppeteerSharp.Input.MouseButton mouseButton = PuppeteerSharp.Input.MouseButton.Left, int clickCount = 1,
        int timeBetweenClicks = 0)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserClickAtCoordinates(data, x, y, ToBrowserMouseButton(mouseButton), clickCount,
            timeBetweenClicks);

    /// <summary>
    /// Compatibility wrapper for pressing a page key without releasing it.
    /// </summary>
    public static Task PuppeteerPageKeyDown(BotData data, string key)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserPageKeyDown(data, key);

    /// <summary>
    /// Compatibility wrapper for releasing a held page key.
    /// </summary>
    public static Task PuppeteerKeyUp(BotData data, string key)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserKeyUp(data, key);

    /// <summary>
    /// Compatibility wrapper for saving a page screenshot to a file.
    /// </summary>
    public static Task PuppeteerScreenshotPage(BotData data, string file, bool fullPage = false, bool omitBackground = false)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserScreenshotPage(data, file, fullPage, omitBackground);

    /// <summary>
    /// Compatibility wrapper for returning a page screenshot as base64.
    /// </summary>
    public static Task<string> PuppeteerScreenshotPageBase64(BotData data, bool fullPage = false, bool omitBackground = false)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserScreenshotPageBase64(data, fullPage, omitBackground);

    /// <summary>
    /// Compatibility wrapper for scrolling to the top of the page.
    /// </summary>
    public static Task PuppeteerScrollToTop(BotData data)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserScrollToTop(data);

    /// <summary>
    /// Compatibility wrapper for scrolling to the bottom of the page.
    /// </summary>
    public static Task PuppeteerScrollToBottom(BotData data)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserScrollToBottom(data);

    /// <summary>
    /// Compatibility wrapper for scrolling the page by offsets.
    /// </summary>
    public static Task PuppeteerScrollBy(BotData data, int horizontalScroll, int verticalScroll)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserScrollBy(data, horizontalScroll, verticalScroll);

    /// <summary>
    /// Compatibility wrapper for setting the viewport options.
    /// </summary>
    public static Task PuppeteerSetViewport(BotData data, int width, int height, bool isMobile = false, bool isLandscape = false,
        float scaleFactor = 1f)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserSetViewport(data, width, height, isMobile, isLandscape, scaleFactor);

    /// <summary>
    /// Compatibility wrapper for getting the current page URL.
    /// </summary>
    public static string PuppeteerGetCurrentUrl(BotData data)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserGetCurrentUrl(data);

    /// <summary>
    /// Compatibility wrapper for getting the current page DOM.
    /// </summary>
    public static Task<string> PuppeteerGetDOM(BotData data)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserGetDOM(data);

    /// <summary>
    /// Compatibility wrapper for getting browser cookies.
    /// </summary>
    public static Task<Dictionary<string, string>> PuppeteerGetCookies(BotData data, string domain)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserGetCookies(data, domain);

    /// <summary>
    /// Compatibility wrapper for setting browser cookies.
    /// </summary>
    public static Task PuppeteerSetCookies(BotData data, string domain, Dictionary<string, string> cookies)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserSetCookies(data, domain, cookies);

    /// <summary>
    /// Compatibility wrapper for setting the page user agent.
    /// </summary>
    public static Task PuppeteerSetUserAgent(BotData data, string userAgent)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserSetUserAgent(data, userAgent);

    /// <summary>
    /// Compatibility wrapper for switching back to the main frame.
    /// </summary>
    public static void PuppeteerSwitchToMainFrame(BotData data)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserSwitchToMainFrame(data);

    /// <summary>
    /// Compatibility wrapper for executing JavaScript in the current frame.
    /// </summary>
    public static Task<string> PuppeteerExecuteJs(BotData data, string expression)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserExecuteJs(data, expression);

    /// <summary>
    /// Compatibility wrapper for waiting for a matching network response.
    /// </summary>
    public static Task PuppeteerWaitForResponse(BotData data, string url, int timeoutMilliseconds = 60000)
        => global::RuriLib.Blocks.Browser.Page.Methods.BrowserWaitForResponse(data, url, timeoutMilliseconds);

    private static BrowserMouseButton ToBrowserMouseButton(PuppeteerSharp.Input.MouseButton mouseButton)
        => Enum.Parse<BrowserMouseButton>(mouseButton.ToString());

    private static BrowserWaitUntilNavigation ToBrowserWaitUntilNavigation(WaitUntilNavigation waitUntil)
        => Enum.Parse<BrowserWaitUntilNavigation>(waitUntil.ToString());
}
