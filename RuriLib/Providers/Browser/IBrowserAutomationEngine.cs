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
    Task OpenBrowser(BotData data, string extraCmdLineArgs = "");
    Task CloseBrowser(BotData data);
    Task NewTab(BotData data);
    Task CloseTab(BotData data);
    Task SwitchToTab(BotData data, int index);
    Task Reload(BotData data);
    Task GoBack(BotData data);
    Task GoForward(BotData data);

    Task NavigateTo(BotData data, string url = "https://example.com",
        BrowserWaitUntilNavigation loadedEvent = BrowserWaitUntilNavigation.Load, string referer = "", int timeout = 30000);
    Task WaitForNavigation(BotData data, BrowserWaitUntilNavigation loadedEvent = BrowserWaitUntilNavigation.Load, int timeout = 30000);
    Task ClearCookies(BotData data, string website);
    Task PageType(BotData data, string text);
    Task PageKeyPress(BotData data, string key);
    Task ClickAtCoordinates(BotData data, int x, int y, BrowserMouseButton mouseButton = BrowserMouseButton.Left, int clickCount = 1,
        int timeBetweenClicks = 0);
    Task PageKeyDown(BotData data, string key);
    Task KeyUp(BotData data, string key);
    Task ScreenshotPage(BotData data, string file, bool fullPage = false, bool omitBackground = false);
    Task<string> ScreenshotPageBase64(BotData data, bool fullPage = false, bool omitBackground = false);
    Task ScrollToTop(BotData data);
    Task ScrollToBottom(BotData data);
    Task ScrollBy(BotData data, int horizontalScroll, int verticalScroll);
    Task SetViewport(BotData data, int width, int height, bool isMobile = false, bool isLandscape = false, float scaleFactor = 1f);
    string GetCurrentUrl(BotData data);
    Task<string> GetDOM(BotData data);
    Task<Dictionary<string, string>> GetCookies(BotData data, string domain);
    Task SetCookies(BotData data, string domain, Dictionary<string, string> cookies);
    Task SetUserAgent(BotData data, string userAgent);
    void SwitchToMainFrame(BotData data);
    Task<string> ExecuteJs(BotData data, string expression);
    Task WaitForResponse(BotData data, string url, int timeoutMilliseconds = 60000);

    Task SetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index, string attributeName, string value);
    Task TypeElement(BotData data, FindElementBy findBy, string identifier, int index, string text, int timeBetweenKeystrokes = 0);
    Task TypeElementHuman(BotData data, FindElementBy findBy, string identifier, int index, string text);
    Task Click(BotData data, FindElementBy findBy, string identifier, int index, BrowserMouseButton mouseButton = BrowserMouseButton.Left,
        int clickCount = 1, int timeBetweenClicks = 0);
    Task Submit(BotData data, FindElementBy findBy, string identifier, int index);
    Task Select(BotData data, FindElementBy findBy, string identifier, int index, string value);
    Task SelectByIndex(BotData data, FindElementBy findBy, string identifier, int index, int selectionIndex);
    Task SelectByText(BotData data, FindElementBy findBy, string identifier, int index, string text);
    Task<string> GetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index, string attributeName = "innerText");
    Task<List<string>> GetAttributeValueAll(BotData data, FindElementBy findBy, string identifier, string attributeName = "innerText");
    Task<bool> IsDisplayed(BotData data, FindElementBy findBy, string identifier, int index);
    Task<bool> Exists(BotData data, FindElementBy findBy, string identifier, int index);
    Task UploadFiles(BotData data, FindElementBy findBy, string identifier, int index, List<string> filePaths);
    Task<int> GetPositionX(BotData data, FindElementBy findBy, string identifier, int index);
    Task<int> GetPositionY(BotData data, FindElementBy findBy, string identifier, int index);
    Task<int> GetWidth(BotData data, FindElementBy findBy, string identifier, int index);
    Task<int> GetHeight(BotData data, FindElementBy findBy, string identifier, int index);
    Task ScreenshotElement(BotData data, FindElementBy findBy, string identifier, int index, string fileName, bool fullPage = false,
        bool omitBackground = false);
    Task<string> ScreenshotElementBase64(BotData data, FindElementBy findBy, string identifier, int index, bool fullPage = false,
        bool omitBackground = false);
    Task SwitchToFrame(BotData data, FindElementBy findBy, string identifier, int index);
    Task WaitForElement(BotData data, FindElementBy findBy, string identifier, bool hidden = false, bool visible = true,
        int timeout = 30000);
}
