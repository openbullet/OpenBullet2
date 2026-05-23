using System.Collections.Generic;
using System.Threading.Tasks;
using RuriLib.Attributes;
using RuriLib.Functions.Browser;
using RuriLib.Models.Bots;

namespace RuriLib.Blocks.Browser.Page;

/// <summary>
/// Blocks for interacting with a browser page.
/// </summary>
[BlockCategory("Page", "Blocks for interacting with a browser page", "#e9967a")]
public static class Methods
{
    /// <summary>
    /// Navigates the current page to a URL.
    /// </summary>
    [Block("Navigates to a given URL in the current page", name = "Navigate To", aliases = ["PuppeteerNavigateTo"])]
    public static Task BrowserNavigateTo(BotData data, string url = "https://example.com",
        BrowserWaitUntilNavigation loadedEvent = BrowserWaitUntilNavigation.Load, string referer = "", int timeout = 30000)
        => data.Providers.BrowserAutomation.Resolve(data).NavigateTo(data, url, loadedEvent, referer, timeout);

    /// <summary>
    /// Waits for the current navigation to complete.
    /// </summary>
    [Block("Waits for navigation to complete", name = "Wait for Navigation", aliases = ["PuppeteerWaitForNavigation"])]
    public static Task BrowserWaitForNavigation(BotData data, BrowserWaitUntilNavigation loadedEvent = BrowserWaitUntilNavigation.Load,
        int timeout = 30000)
        => data.Providers.BrowserAutomation.Resolve(data).WaitForNavigation(data, loadedEvent, timeout);

    /// <summary>
    /// Clears cookies for the specified website.
    /// </summary>
    [Block("Clears cookies in the page stored for a specific website", name = "Clear Cookies", aliases = ["PuppeteerClearCookies"])]
    public static Task BrowserClearCookies(BotData data, string website)
        => data.Providers.BrowserAutomation.Resolve(data).ClearCookies(data, website);

    /// <summary>
    /// Types text directly into the page.
    /// </summary>
    [Block("Sends keystrokes to the browser page", name = "Type in Page", aliases = ["PuppeteerPageType"])]
    public static Task BrowserPageType(BotData data, string text)
        => data.Providers.BrowserAutomation.Resolve(data).PageType(data, text);

    /// <summary>
    /// Presses and releases a key on the page.
    /// </summary>
    [Block("Presses and releases a key in the browser page", name = "Key Press in Page",
        extraInfo = "Full list of keys here: https://github.com/puppeteer/puppeteer/blob/v1.14.0/lib/USKeyboardLayout.js",
        aliases = ["PuppeteerPageKeyPress"])]
    public static Task BrowserPageKeyPress(BotData data, string key)
        => data.Providers.BrowserAutomation.Resolve(data).PageKeyPress(data, key);

    /// <summary>
    /// Clicks the page at the given coordinates.
    /// </summary>
    [Block("Clicks the page at the given coordinates", name = "Click at Coordinates", aliases = ["PuppeteerClickAtCoordinates"])]
    public static Task BrowserClickAtCoordinates(BotData data, int x, int y, BrowserMouseButton mouseButton = BrowserMouseButton.Left,
        int clickCount = 1, int timeBetweenClicks = 0)
        => data.Providers.BrowserAutomation.Resolve(data).ClickAtCoordinates(data, x, y, mouseButton, clickCount, timeBetweenClicks);

    /// <summary>
    /// Moves the cursor to the given page coordinates.
    /// </summary>
    [Block("Moves the cursor to the given page coordinates without clicking", name = "Move Cursor to Coordinates")]
    public static Task BrowserMoveCursorToCoordinates(BotData data, int x, int y)
        => data.Providers.BrowserAutomation.Resolve(data).MoveCursorToCoordinates(data, x, y);

    /// <summary>
    /// Presses a mouse button at the current cursor location.
    /// </summary>
    [Block("Presses a mouse button at the current cursor location without releasing it", name = "Mouse Down")]
    public static Task BrowserMouseDown(BotData data, BrowserMouseButton mouseButton = BrowserMouseButton.Left, int clickCount = 1)
        => data.Providers.BrowserAutomation.Resolve(data).MouseDown(data, mouseButton, clickCount);

    /// <summary>
    /// Releases a mouse button at the current cursor location.
    /// </summary>
    [Block("Releases a mouse button at the current cursor location", name = "Mouse Up")]
    public static Task BrowserMouseUp(BotData data, BrowserMouseButton mouseButton = BrowserMouseButton.Left, int clickCount = 1)
        => data.Providers.BrowserAutomation.Resolve(data).MouseUp(data, mouseButton, clickCount);

    /// <summary>
    /// Injects the Ghost Cursor mouse position helper into the current page for debugging.
    /// </summary>
    [Block("Injects the Ghost Cursor mouse position helper into the current page for debugging", name = "Inject Mouse Position Helper")]
    public static Task BrowserInjectMousePositionHelper(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).InjectMousePositionHelper(data);

    /// <summary>
    /// Presses a key on the page without releasing it.
    /// </summary>
    [Block("Presses a key in the browser page without releasing it", name = "Key Down in Page",
        extraInfo = "Full list of keys here: https://github.com/puppeteer/puppeteer/blob/v1.14.0/lib/USKeyboardLayout.js",
        aliases = ["PuppeteerPageKeyDown"])]
    public static Task BrowserPageKeyDown(BotData data, string key)
        => data.Providers.BrowserAutomation.Resolve(data).PageKeyDown(data, key);

    /// <summary>
    /// Releases a key that is currently held down on the page.
    /// </summary>
    [Block("Releases a key that was previously pressed in the browser page", name = "Key Up in Page",
        extraInfo = "Full list of keys here: https://github.com/puppeteer/puppeteer/blob/v1.14.0/lib/USKeyboardLayout.js",
        aliases = ["PuppeteerKeyUp"])]
    public static Task BrowserKeyUp(BotData data, string key)
        => data.Providers.BrowserAutomation.Resolve(data).KeyUp(data, key);

    /// <summary>
    /// Saves a screenshot of the page to a file.
    /// </summary>
    [Block("Takes a screenshot of the entire browser page and saves it to an output file", name = "Screenshot Page",
        aliases = ["PuppeteerScreenshotPage"])]
    public static Task BrowserScreenshotPage(BotData data, string file, bool fullPage = false, bool omitBackground = false)
        => data.Providers.BrowserAutomation.Resolve(data).ScreenshotPage(data, file, fullPage, omitBackground);

    /// <summary>
    /// Captures a page screenshot and returns it as a base64 string.
    /// </summary>
    [Block("Takes a screenshot of the entire browser page and converts it to a base64 string", name = "Screenshot Page Base64",
        aliases = ["PuppeteerScreenshotPageBase64"])]
    public static Task<string> BrowserScreenshotPageBase64(BotData data, bool fullPage = false, bool omitBackground = false)
        => data.Providers.BrowserAutomation.Resolve(data).ScreenshotPageBase64(data, fullPage, omitBackground);

    /// <summary>
    /// Scrolls to the top of the page.
    /// </summary>
    [Block("Scrolls to the top of the page", name = "Scroll to Top", aliases = ["PuppeteerScrollToTop"])]
    public static Task BrowserScrollToTop(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).ScrollToTop(data);

    /// <summary>
    /// Scrolls to the bottom of the page.
    /// </summary>
    [Block("Scrolls to the bottom of the page", name = "Scroll to Bottom", aliases = ["PuppeteerScrollToBottom"])]
    public static Task BrowserScrollToBottom(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).ScrollToBottom(data);

    /// <summary>
    /// Scrolls the page by the specified offsets.
    /// </summary>
    [Block("Scrolls the page by a certain amount horizontally and vertically", name = "Scroll by", aliases = ["PuppeteerScrollBy"])]
    public static Task BrowserScrollBy(BotData data, int horizontalScroll, int verticalScroll)
        => data.Providers.BrowserAutomation.Resolve(data).ScrollBy(data, horizontalScroll, verticalScroll);

    /// <summary>
    /// Sets the viewport size and emulation options.
    /// </summary>
    [Block("Sets the viewport dimensions and options", name = "Set Viewport", aliases = ["PuppeteerSetViewport"])]
    public static Task BrowserSetViewport(BotData data, int width, int height, bool isMobile = false, bool isLandscape = false,
        float scaleFactor = 1f)
        => data.Providers.BrowserAutomation.Resolve(data).SetViewport(data, width, height, isMobile, isLandscape, scaleFactor);

    /// <summary>
    /// Gets the current page URL.
    /// </summary>
    [Block("Gets the current URL of the page", name = "Get Current URL", aliases = ["PuppeteerGetCurrentUrl"])]
    public static string BrowserGetCurrentUrl(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).GetCurrentUrl(data);

    /// <summary>
    /// Gets the current page DOM.
    /// </summary>
    [Block("Gets the full DOM of the page", name = "Get DOM", aliases = ["PuppeteerGetDOM"])]
    public static Task<string> BrowserGetDOM(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).GetDOM(data);

    /// <summary>
    /// Gets cookies for the current page or a specific domain.
    /// </summary>
    [Block("Gets the cookies for a given domain from the browser. If the domain is empty, gets all cookies from the page.", name = "Get Cookies",
        aliases = ["PuppeteerGetCookies"])]
    public static Task<Dictionary<string, string>> BrowserGetCookies(BotData data, string domain)
        => data.Providers.BrowserAutomation.Resolve(data).GetCookies(data, domain);

    /// <summary>
    /// Sets cookies for a specific domain.
    /// </summary>
    [Block("Sets the cookies for a given domain in the browser page", name = "Set Cookies", aliases = ["PuppeteerSetCookies"])]
    public static Task BrowserSetCookies(BotData data, string domain, Dictionary<string, string> cookies)
        => data.Providers.BrowserAutomation.Resolve(data).SetCookies(data, domain, cookies);

    /// <summary>
    /// Sets the page user agent.
    /// </summary>
    [Block("Sets the User Agent of the browser page", name = "Set User-Agent", aliases = ["PuppeteerSetUserAgent"])]
    public static Task BrowserSetUserAgent(BotData data, string userAgent)
        => data.Providers.BrowserAutomation.Resolve(data).SetUserAgent(data, userAgent);

    /// <summary>
    /// Switches the current context back to the main frame.
    /// </summary>
    [Block("Switches to the main frame of the page", name = "Switch to Main Frame", aliases = ["PuppeteerSwitchToMainFrame"])]
    public static void BrowserSwitchToMainFrame(BotData data)
        => data.Providers.BrowserAutomation.Resolve(data).SwitchToMainFrame(data);

    /// <summary>
    /// Executes JavaScript in the current frame and returns the serialized result.
    /// </summary>
    [Block("Evaluates a js expression in the current frame context and returns a json response", name = "Execute JS",
        aliases = ["PuppeteerExecuteJs"])]
    public static Task<string> BrowserExecuteJs(BotData data, [MultiLine] string expression)
        => data.Providers.BrowserAutomation.Resolve(data).ExecuteJs(data, expression);

    /// <summary>
    /// Waits for a matching network response.
    /// </summary>
    [Block("Captures the response from the given URL", name = "Wait for Response", aliases = ["PuppeteerWaitForResponse"])]
    public static Task BrowserWaitForResponse(BotData data, string url, int timeoutMilliseconds = 60000)
        => data.Providers.BrowserAutomation.Resolve(data).WaitForResponse(data, url, timeoutMilliseconds);
}
