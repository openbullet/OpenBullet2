using System.Collections.Generic;
using System.Threading.Tasks;
using RuriLib.Functions.Browser;
using RuriLib.Functions.Puppeteer;
using RuriLib.Models.Bots;

namespace RuriLib.Providers.Browser;

/// <summary>
/// Executes browser automation actions behind the generic browser block surface.
/// </summary>
public interface IBrowserAutomationEngine
{
    /// <summary>
    /// Opens a new browser instance.
    /// </summary>
    Task OpenBrowser(BotData data, string extraCmdLineArgs = "");
    /// <summary>
    /// Closes the current browser instance.
    /// </summary>
    Task CloseBrowser(BotData data);
    /// <summary>
    /// Opens a new tab.
    /// </summary>
    Task NewTab(BotData data);
    /// <summary>
    /// Closes the active tab.
    /// </summary>
    Task CloseTab(BotData data);
    /// <summary>
    /// Switches to a tab by index.
    /// </summary>
    Task SwitchToTab(BotData data, int index);
    /// <summary>
    /// Reloads the current page.
    /// </summary>
    Task Reload(BotData data);
    /// <summary>
    /// Navigates back in history.
    /// </summary>
    Task GoBack(BotData data);
    /// <summary>
    /// Navigates forward in history.
    /// </summary>
    Task GoForward(BotData data);

    /// <summary>
    /// Navigates the current page to a URL.
    /// </summary>
    Task NavigateTo(BotData data, string url = "https://example.com",
        BrowserWaitUntilNavigation loadedEvent = BrowserWaitUntilNavigation.Load, string referer = "", int timeout = 30000);
    /// <summary>
    /// Waits for the current navigation to complete.
    /// </summary>
    Task WaitForNavigation(BotData data, BrowserWaitUntilNavigation loadedEvent = BrowserWaitUntilNavigation.Load, int timeout = 30000);
    /// <summary>
    /// Clears cookies for the specified website.
    /// </summary>
    Task ClearCookies(BotData data, string website);
    /// <summary>
    /// Types text directly into the page.
    /// </summary>
    Task PageType(BotData data, string text);
    /// <summary>
    /// Presses and releases a key on the page.
    /// </summary>
    Task PageKeyPress(BotData data, string key);
    /// <summary>
    /// Clicks the page at the given coordinates.
    /// </summary>
    Task ClickAtCoordinates(BotData data, int x, int y, BrowserMouseButton mouseButton = BrowserMouseButton.Left, int clickCount = 1,
        int timeBetweenClicks = 0);
    /// <summary>
    /// Presses a key on the page without releasing it.
    /// </summary>
    Task PageKeyDown(BotData data, string key);
    /// <summary>
    /// Releases a previously pressed page key.
    /// </summary>
    Task KeyUp(BotData data, string key);
    /// <summary>
    /// Saves a screenshot of the page to a file.
    /// </summary>
    Task ScreenshotPage(BotData data, string file, bool fullPage = false, bool omitBackground = false);
    /// <summary>
    /// Captures a page screenshot and returns it as a base64 string.
    /// </summary>
    Task<string> ScreenshotPageBase64(BotData data, bool fullPage = false, bool omitBackground = false);
    /// <summary>
    /// Scrolls to the top of the page.
    /// </summary>
    Task ScrollToTop(BotData data);
    /// <summary>
    /// Scrolls to the bottom of the page.
    /// </summary>
    Task ScrollToBottom(BotData data);
    /// <summary>
    /// Scrolls the page by the specified offsets.
    /// </summary>
    Task ScrollBy(BotData data, int horizontalScroll, int verticalScroll);
    /// <summary>
    /// Sets the viewport size and emulation options.
    /// </summary>
    Task SetViewport(BotData data, int width, int height, bool isMobile = false, bool isLandscape = false, float scaleFactor = 1f);
    /// <summary>
    /// Gets the current page URL.
    /// </summary>
    string GetCurrentUrl(BotData data);
    /// <summary>
    /// Gets the current page DOM.
    /// </summary>
    Task<string> GetDOM(BotData data);
    /// <summary>
    /// Gets the cookies for the page or a specific domain.
    /// </summary>
    Task<Dictionary<string, string>> GetCookies(BotData data, string domain);
    /// <summary>
    /// Sets cookies for a specific domain.
    /// </summary>
    Task SetCookies(BotData data, string domain, Dictionary<string, string> cookies);
    /// <summary>
    /// Sets the page user agent.
    /// </summary>
    Task SetUserAgent(BotData data, string userAgent);
    /// <summary>
    /// Switches the current context back to the main frame.
    /// </summary>
    void SwitchToMainFrame(BotData data);
    /// <summary>
    /// Executes JavaScript in the current frame and returns the serialized result.
    /// </summary>
    Task<string> ExecuteJs(BotData data, string expression);
    /// <summary>
    /// Waits for a matching network response.
    /// </summary>
    Task WaitForResponse(BotData data, string url, int timeoutMilliseconds = 60000);

    /// <summary>
    /// Sets an attribute value on an element.
    /// </summary>
    Task SetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index, string attributeName, string value);
    /// <summary>
    /// Types text into an element.
    /// </summary>
    Task TypeElement(BotData data, FindElementBy findBy, string identifier, int index, string text, int timeBetweenKeystrokes = 0);
    /// <summary>
    /// Types text into an element with human-like delays.
    /// </summary>
    Task TypeElementHuman(BotData data, FindElementBy findBy, string identifier, int index, string text);
    /// <summary>
    /// Clicks an element.
    /// </summary>
    Task Click(BotData data, FindElementBy findBy, string identifier, int index, BrowserMouseButton mouseButton = BrowserMouseButton.Left,
        int clickCount = 1, int timeBetweenClicks = 0);
    /// <summary>
    /// Submits the form that contains the element.
    /// </summary>
    Task Submit(BotData data, FindElementBy findBy, string identifier, int index);
    /// <summary>
    /// Selects an option by value.
    /// </summary>
    Task Select(BotData data, FindElementBy findBy, string identifier, int index, string value);
    /// <summary>
    /// Selects an option by index.
    /// </summary>
    Task SelectByIndex(BotData data, FindElementBy findBy, string identifier, int index, int selectionIndex);
    /// <summary>
    /// Selects an option by visible text.
    /// </summary>
    Task SelectByText(BotData data, FindElementBy findBy, string identifier, int index, string text);
    /// <summary>
    /// Gets an attribute value from an element.
    /// </summary>
    Task<string> GetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index, string attributeName = "innerText");
    /// <summary>
    /// Gets an attribute value from all matching elements.
    /// </summary>
    Task<List<string>> GetAttributeValueAll(BotData data, FindElementBy findBy, string identifier, string attributeName = "innerText");
    /// <summary>
    /// Checks whether an element is displayed.
    /// </summary>
    Task<bool> IsDisplayed(BotData data, FindElementBy findBy, string identifier, int index);
    /// <summary>
    /// Checks whether an element exists.
    /// </summary>
    Task<bool> Exists(BotData data, FindElementBy findBy, string identifier, int index);
    /// <summary>
    /// Uploads files through an element.
    /// </summary>
    Task UploadFiles(BotData data, FindElementBy findBy, string identifier, int index, List<string> filePaths);
    /// <summary>
    /// Gets the horizontal position of an element.
    /// </summary>
    Task<int> GetPositionX(BotData data, FindElementBy findBy, string identifier, int index);
    /// <summary>
    /// Gets the vertical position of an element.
    /// </summary>
    Task<int> GetPositionY(BotData data, FindElementBy findBy, string identifier, int index);
    /// <summary>
    /// Gets the width of an element.
    /// </summary>
    Task<int> GetWidth(BotData data, FindElementBy findBy, string identifier, int index);
    /// <summary>
    /// Gets the height of an element.
    /// </summary>
    Task<int> GetHeight(BotData data, FindElementBy findBy, string identifier, int index);
    /// <summary>
    /// Saves a screenshot of an element to a file.
    /// </summary>
    Task ScreenshotElement(BotData data, FindElementBy findBy, string identifier, int index, string fileName, bool fullPage = false,
        bool omitBackground = false);
    /// <summary>
    /// Captures an element screenshot and returns it as a base64 string.
    /// </summary>
    Task<string> ScreenshotElementBase64(BotData data, FindElementBy findBy, string identifier, int index, bool fullPage = false,
        bool omitBackground = false);
    /// <summary>
    /// Switches the current context to an iframe.
    /// </summary>
    Task SwitchToFrame(BotData data, FindElementBy findBy, string identifier, int index);
    /// <summary>
    /// Waits for an element to reach the requested visibility state.
    /// </summary>
    Task WaitForElement(BotData data, FindElementBy findBy, string identifier, bool hidden = false, bool visible = true,
        int timeout = 30000);
}
