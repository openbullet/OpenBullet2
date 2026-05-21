using Microsoft.Playwright;
using Newtonsoft.Json;
using RuriLib.Exceptions;
using RuriLib.Functions.Browser;
using RuriLib.Functions.Files;
using RuriLib.Functions.Puppeteer;
using RuriLib.Helpers;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Settings;
using RuriLib.Providers.Playwright;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ProxyType = RuriLib.Models.Proxies.ProxyType;

namespace RuriLib.Providers.Browser;

/// <summary>
/// Browser automation engine backed by Playwright.
/// </summary>
public class PlaywrightBrowserAutomationEngine : IBrowserAutomationEngine
{
    private const string PlaywrightInstanceObject = "playwright";
    private const string BrowserObject = "playwrightBrowser";
    private const string ContextObject = "playwrightContext";
    private const string PageObject = "playwrightPage";
    private const string FrameObject = "playwrightFrame";
    private const string UserAgentObject = "playwrightUserAgent";

    private readonly IPlaywrightBrowserProvider _playwrightBrowserProvider;

    /// <summary>
    /// Creates the engine with the configured Playwright provider.
    /// </summary>
    public PlaywrightBrowserAutomationEngine(IPlaywrightBrowserProvider playwrightBrowserProvider)
    {
        _playwrightBrowserProvider = playwrightBrowserProvider;
    }

    /// <inheritdoc />
    public async Task OpenBrowser(BotData data, string extraCmdLineArgs = "")
    {
        data.Logger.LogHeader();

        var oldBrowser = data.TryGetObject<IBrowser>(BrowserObject);
        if (oldBrowser is not null && oldBrowser.IsConnected)
        {
            data.Logger.Log("The browser is already open, close it if you want to open a new browser", LogColors.DarkSalmon);
            return;
        }

        var args = CommandLineArgumentParser.ParseMany(
            data.ConfigSettings.BrowserSettings.CommandLineArgs,
            extraCmdLineArgs).ToList();

        if (Utils.IsDocker())
        {
            args.Add("--no-sandbox");
        }

        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browserType = GetBrowserType(playwright, _playwrightBrowserProvider.BrowserType);

        if (_playwrightBrowserProvider.Source == PlaywrightBrowserSource.Managed)
        {
            // Playwright-managed browsers are versioned alongside the installed
            // Microsoft.Playwright package. Updating the NuGet package is what
            // moves the expected browser build forward; this install step only
            // fetches the build required by the current package version.
            await PlaywrightBrowserInstaller.EnsureBrowserInstalledAsync(
                browserType,
                GetPlaywrightBrowserName(_playwrightBrowserProvider.BrowserType),
                data.CancellationToken);
        }

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Args = args,
            ExecutablePath = _playwrightBrowserProvider.Source == PlaywrightBrowserSource.ExecutablePath
                ? _playwrightBrowserProvider.ExecutablePath
                : null,
            Headless = data.ConfigSettings.BrowserSettings.Headless,
            Proxy = BuildProxy(data)
        };

        var browser = await browserType.LaunchAsync(launchOptions);
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = data.ConfigSettings.BrowserSettings.IgnoreHttpsErrors
        });
        var page = await context.NewPageAsync();
        await SetPageLoadingOptions(data, page);

        data.SetObject(PlaywrightInstanceObject, playwright, false);
        data.SetObject(BrowserObject, browser, false);
        data.SetObject(ContextObject, context, false);
        SetPageAndFrame(data, page);

        data.Logger.Log($"{(launchOptions.Headless is true ? "Headless " : "")}Browser opened successfully!", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task CloseBrowser(BotData data)
    {
        data.Logger.LogHeader();

        var browser = GetBrowser(data);
        await browser.CloseAsync();
        GetPlaywright(data).Dispose();
        data.SetObject(PlaywrightInstanceObject, null, false);
        data.SetObject(BrowserObject, null, false);
        data.SetObject(ContextObject, null, false);
        data.SetObject(PageObject, null, false);
        data.SetObject(FrameObject, null, false);
        data.SetObject(UserAgentObject, null, false);
        data.Logger.Log("Browser closed successfully!", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task NewTab(BotData data)
    {
        data.Logger.LogHeader();

        var context = GetContext(data);
        var page = await context.NewPageAsync();
        await SetPageLoadingOptions(data, page);

        SetPageAndFrame(data, page);
        data.Logger.Log("Opened a new page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task CloseTab(BotData data)
    {
        data.Logger.LogHeader();

        var context = GetContext(data);
        var page = GetPage(data);

        await page.CloseAsync();

        page = context.Pages.FirstOrDefault(p => !p.IsClosed);
        SetPageAndFrame(data, page);

        if (page is not null)
        {
            await page.BringToFrontAsync();
        }

        data.Logger.Log("Closed the active page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task SwitchToTab(BotData data, int index)
    {
        data.Logger.LogHeader();

        var context = GetContext(data);
        var page = context.Pages[index];

        await page.BringToFrontAsync();
        SetPageAndFrame(data, page);

        data.Logger.Log($"Switched to tab with index {index}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task Reload(BotData data)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.ReloadAsync();
        SwitchToMainFramePrivate(data);

        data.Logger.Log("Reloaded the page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task GoBack(BotData data)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.GoBackAsync(new PageGoBackOptions
        {
            WaitUntil = WaitUntilState.Load
        });
        SwitchToMainFramePrivate(data);

        data.Logger.Log("Went back to the previously visited page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task GoForward(BotData data)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.GoForwardAsync(new PageGoForwardOptions
        {
            WaitUntil = WaitUntilState.Load
        });
        SwitchToMainFramePrivate(data);

        data.Logger.Log("Went forward to the next visited page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task NavigateTo(BotData data, string url = "https://example.com",
        BrowserWaitUntilNavigation loadedEvent = BrowserWaitUntilNavigation.Load, string referer = "", int timeout = 30000)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        var response = await page.GotoAsync(url, new PageGotoOptions
        {
            Referer = referer,
            Timeout = timeout,
            WaitUntil = ToPlaywrightWaitUntilState(loadedEvent)
        });

        data.ADDRESS = response?.Url ?? page.Url;

        if (response is not null)
        {
            data.SOURCE = await response.TextAsync();
            data.RAWSOURCE = await response.BodyAsync();
        }
        else
        {
            data.SOURCE = await page.ContentAsync();
            data.RAWSOURCE = Encoding.UTF8.GetBytes(data.SOURCE);
        }

        SwitchToMainFramePrivate(data);

        data.Logger.Log($"Navigated to {url}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task WaitForNavigation(BotData data, BrowserWaitUntilNavigation loadedEvent = BrowserWaitUntilNavigation.Load, int timeout = 30000)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.WaitForLoadStateAsync(ToPlaywrightLoadState(loadedEvent), new PageWaitForLoadStateOptions
        {
            Timeout = timeout
        });

        data.ADDRESS = page.Url;
        data.SOURCE = await page.ContentAsync();
        data.RAWSOURCE = Encoding.UTF8.GetBytes(data.SOURCE);
        SwitchToMainFramePrivate(data);

        data.Logger.Log("Waited for navigation to complete", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task ClearCookies(BotData data, string website)
    {
        data.Logger.LogHeader();

        var context = GetContext(data);

        if (string.IsNullOrWhiteSpace(website))
        {
            await context.ClearCookiesAsync();
        }
        else
        {
            var domain = Uri.TryCreate(website, UriKind.Absolute, out var uri)
                ? uri.Host
                : website;

            await context.ClearCookiesAsync(new BrowserContextClearCookiesOptions
            {
                Domain = domain
            });
        }

        data.Logger.Log($"Cookies cleared for site {website}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task PageType(BotData data, string text)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.Keyboard.TypeAsync(text);
        data.Logger.Log($"Typed {text}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task PageKeyPress(BotData data, string key)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.Keyboard.PressAsync(key);
        data.Logger.Log($"Pressed and released {key}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task ClickAtCoordinates(BotData data, int x, int y, BrowserMouseButton mouseButton = BrowserMouseButton.Left, int clickCount = 1,
        int timeBetweenClicks = 0)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        var frame = GetFrame(data);
        var (clickX, clickY) = await ResolveClickPoint(frame, x, y);
        await page.Mouse.ClickAsync(clickX, clickY, new MouseClickOptions
        {
            Button = ToPlaywrightMouseButton(mouseButton),
            ClickCount = clickCount,
            Delay = timeBetweenClicks
        });

        data.Logger.Log($"Clicked {clickCount} time(s) with {mouseButton} button at ({x}, {y})", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task PageKeyDown(BotData data, string key)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.Keyboard.DownAsync(key);
        data.Logger.Log($"Pressed (and holding down) {key}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task KeyUp(BotData data, string key)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.Keyboard.UpAsync(key);
        data.Logger.Log($"Released {key}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task ScreenshotPage(BotData data, string file, bool fullPage = false, bool omitBackground = false)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = file,
            FullPage = fullPage,
            OmitBackground = omitBackground,
            Type = omitBackground ? ScreenshotType.Png : ScreenshotType.Jpeg,
            Quality = omitBackground ? null : 100
        });

        data.Logger.Log($"Took a screenshot of the {(fullPage ? "full" : "visible")} page and saved it to {file}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task<string> ScreenshotPageBase64(BotData data, bool fullPage = false, bool omitBackground = false)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        var bytes = await page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = fullPage,
            OmitBackground = omitBackground,
            Type = omitBackground ? ScreenshotType.Png : ScreenshotType.Jpeg,
            Quality = omitBackground ? null : 100
        });
        var base64 = Convert.ToBase64String(bytes);

        data.Logger.Log($"Took a screenshot of the {(fullPage ? "full" : "visible")} page as base64", LogColors.DarkSalmon);
        return base64;
    }

    /// <inheritdoc />
    public async Task ScrollToTop(BotData data)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.EvaluateAsync("window.scrollTo(0, 0);");
        data.Logger.Log("Scrolled to the top of the page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task ScrollToBottom(BotData data)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight);");
        data.Logger.Log("Scrolled to the bottom of the page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task ScrollBy(BotData data, int horizontalScroll, int verticalScroll)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.EvaluateAsync($"window.scrollBy({horizontalScroll}, {verticalScroll});");
        data.Logger.Log($"Scrolled by ({horizontalScroll}, {verticalScroll})", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task SetViewport(BotData data, int width, int height, bool isMobile = false, bool isLandscape = false, float scaleFactor = 1f)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.SetViewportSizeAsync(width, height);
        data.Logger.Log($"Set the viewport size to {width}x{height}", LogColors.DarkSalmon);
        _ = isMobile;
        _ = isLandscape;
        _ = scaleFactor;
    }

    /// <inheritdoc />
    public string GetCurrentUrl(BotData data)
    {
        data.Logger.LogHeader();

        var currentUrl = GetPage(data).Url;
        data.ADDRESS = currentUrl;

        data.Logger.Log($"Current URL: {currentUrl}", LogColors.DarkSalmon);
        return currentUrl;
    }

    /// <inheritdoc />
    public async Task<string> GetDOM(BotData data)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        var dom = await page.EvaluateAsync<string>("document.body.innerHTML");

        data.Logger.Log("Got the full page DOM", LogColors.DarkSalmon);
        data.Logger.Log(dom, LogColors.DarkSalmon, true);
        return dom;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> GetCookies(BotData data, string domain)
    {
        data.Logger.LogHeader();

        var context = GetContext(data);
        var cookies = await context.CookiesAsync();

        if (!string.IsNullOrWhiteSpace(domain))
        {
            cookies = cookies.Where(c => c.Domain.Contains(domain, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        data.Logger.Log($"Got {cookies.Count} cookies for {(string.IsNullOrWhiteSpace(domain) ? "all domains" : domain)}", LogColors.DarkSalmon);
        return cookies.ToDictionary(c => c.Name, c => c.Value);
    }

    /// <inheritdoc />
    public async Task SetCookies(BotData data, string domain, Dictionary<string, string> cookies)
    {
        data.Logger.LogHeader();

        var context = GetContext(data);
        var page = GetPage(data);
        await context.AddCookiesAsync(cookies.Select(c => new Cookie
        {
            Name = c.Key,
            Value = c.Value,
            Domain = string.IsNullOrWhiteSpace(domain) ? null : domain,
            Path = string.IsNullOrWhiteSpace(domain) ? null : "/",
            Url = string.IsNullOrWhiteSpace(domain) ? page.Url : null
        }));

        data.Logger.Log($"Set {cookies.Count} cookies for {domain}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task SetUserAgent(BotData data, string userAgent)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        data.SetObject(UserAgentObject, userAgent, false);
        await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
        {
            ["User-Agent"] = userAgent
        });
        await page.EvaluateAsync(
            """
            userAgent => {
                try {
                    Object.defineProperty(navigator, 'userAgent', { get: () => userAgent, configurable: true });
                } catch {
                    // Ignore browsers that don't allow overriding navigator.userAgent on the current document.
                }
            }
            """,
            userAgent);

        data.Logger.Log($"User Agent set to {userAgent}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public void SwitchToMainFrame(BotData data)
    {
        data.Logger.LogHeader();

        SwitchToMainFramePrivate(data);
        data.Logger.Log("Switched to main frame", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task<string> ExecuteJs(BotData data, string expression)
    {
        data.Logger.LogHeader();

        var frame = GetFrame(data);
        data.Logger.Log(frame.ParentFrame is null
            ? "Executing JS in the main frame context"
            : "Executing JS in the current iframe context",
            LogColors.DarkSalmon);
        var value = await frame.EvaluateAsync<object>(expression);
        var json = SerializeJavaScriptResult(value);
        data.Logger.Log($"Evaluated {expression}", LogColors.DarkSalmon);
        data.Logger.Log($"Got result: {json}", LogColors.DarkSalmon);

        return json;
    }

    /// <inheritdoc />
    public async Task WaitForResponse(BotData data, string url, int timeoutMilliseconds = 60000)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        var response = await page.WaitForResponseAsync(r => UrlsMatch(r.Url, url), new PageWaitForResponseOptions
        {
            Timeout = timeoutMilliseconds
        });

        data.ADDRESS = response.Url;
        data.RESPONSECODE = response.Status;
        data.HEADERS = response.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        data.SOURCE = string.Empty;
        data.RAWSOURCE = [];

        if (ResponseCanHaveBody(response))
        {
            await TryPopulateResponseBody(data, response);
        }

        data.Logger.Log($"Address: {data.ADDRESS}", LogColors.DodgerBlue);
        data.Logger.Log($"Response code: {data.RESPONSECODE}", LogColors.Citrine);

        data.Logger.Log("Received Headers:", LogColors.MediumPurple);
        data.Logger.Log(data.HEADERS.Select(h => $"{h.Key}: {h.Value}"), LogColors.Violet);

        data.Logger.Log("Received Payload:", LogColors.ForestGreen);
        data.Logger.Log(data.SOURCE, LogColors.GreenYellow, true);
    }

    /// <inheritdoc />
    public async Task SetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index, string attributeName, string value)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var script = "(element, arg) => element.setAttribute(arg.attributeName, arg.value)";
        await elem.EvaluateAsync(script, new { attributeName, value });

        data.Logger.Log($"Set value {value} of attribute {attributeName} by executing {script}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task TypeElement(BotData data, FindElementBy findBy, string identifier, int index, string text, int timeBetweenKeystrokes = 0)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        await elem.FocusAsync();
        await GetPage(data).Keyboard.TypeAsync(text, new KeyboardTypeOptions { Delay = timeBetweenKeystrokes });

        data.Logger.Log($"Typed {text}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task TypeElementHuman(BotData data, FindElementBy findBy, string identifier, int index, string text)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        await elem.FocusAsync();
        var keyboard = GetPage(data).Keyboard;

        foreach (var c in text)
        {
            await keyboard.TypeAsync(c.ToString());
            await Task.Delay(data.Random.Next(100, 300), data.CancellationToken);
        }

        data.Logger.Log($"Typed {text}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task Click(BotData data, FindElementBy findBy, string identifier, int index, BrowserMouseButton mouseButton = BrowserMouseButton.Left,
        int clickCount = 1, int timeBetweenClicks = 0)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        await elem.ClickAsync(new ElementHandleClickOptions
        {
            Button = ToPlaywrightMouseButton(mouseButton),
            ClickCount = clickCount,
            Delay = timeBetweenClicks
        });

        data.Logger.Log($"Clicked {clickCount} time(s) with {mouseButton} button", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task Submit(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var script = """
                     element => {
                         const form = element instanceof HTMLFormElement ? element : element.form;
                         if (!form) {
                             throw new Error('No parent form found');
                         }

                         if (typeof form.requestSubmit === 'function') {
                             form.requestSubmit();
                             return;
                         }

                         const submitEvent = new Event('submit', { bubbles: true, cancelable: true });
                         if (form.dispatchEvent(submitEvent)) {
                             form.submit();
                         }
                     }
                     """;
        await elem.EvaluateAsync(script);

        data.Logger.Log($"Submitted the form by executing {script}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task Select(BotData data, FindElementBy findBy, string identifier, int index, string value)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        await elem.EvaluateAsync(
            """
            (element, selectedValue) => {
                element.value = selectedValue;
                element.dispatchEvent(new Event('input', { bubbles: true }));
                element.dispatchEvent(new Event('change', { bubbles: true }));
            }
            """,
            value);

        data.Logger.Log($"Selected value {value}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task SelectByIndex(BotData data, FindElementBy findBy, string identifier, int index, int selectionIndex)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var value = await elem.EvaluateAsync<string>(
            """
            (element, selectedIndex) => {
                const option = element.getElementsByTagName('option')[selectedIndex];
                return option?.value ?? null;
            }
            """,
            selectionIndex);

        if (value is null)
        {
            throw new BlockExecutionException($"Expected an option at index {selectionIndex} but none was found");
        }

        await Select(data, findBy, identifier, index, value);
    }

    /// <inheritdoc />
    public async Task SelectByText(BotData data, FindElementBy findBy, string identifier, int index, string text)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        await elem.EvaluateAsync(
            """
            (element, selectedText) => {
                for (let i = 0; i < element.options.length; i++) {
                    if (element.options[i].text === selectedText) {
                        element.selectedIndex = i;
                        element.dispatchEvent(new Event('input', { bubbles: true }));
                        element.dispatchEvent(new Event('change', { bubbles: true }));
                        return;
                    }
                }
            }
            """,
            text);

        data.Logger.Log($"Selected text {text}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task<string> GetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index, string attributeName = "innerText")
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var script = """
                     (element, attributeName) => {
                         const value = attributeName
                             .split('.')
                             .reduce((current, part) => current?.[part], element);
                         return value?.toString() ?? '';
                     }
                     """;
        var value = await elem.EvaluateAsync<string>(script, attributeName);

        data.Logger.Log($"Got value {value} of attribute {attributeName} by executing {script}", LogColors.DarkSalmon);
        return value;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetAttributeValueAll(BotData data, FindElementBy findBy, string identifier, string attributeName = "innerText")
    {
        data.Logger.LogHeader();

        var elements = await GetElements(GetFrame(data), findBy, identifier);
        var values = new List<string>();

        foreach (var element in elements)
        {
            values.Add(await element.EvaluateAsync<string>(
                """
                (element, attributeName) => {
                    const value = attributeName
                        .split('.')
                        .reduce((current, part) => current?.[part], element);
                    return value?.toString() ?? '';
                }
                """,
                attributeName));
        }

        data.Logger.Log($"Got {values.Count} values for attribute {attributeName}", LogColors.DarkSalmon);
        return values;
    }

    /// <inheritdoc />
    public async Task<bool> IsDisplayed(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elements = await GetElements(GetFrame(data), findBy, identifier);
        var displayed = elements.Count > index && await elements[index].IsVisibleAsync();

        data.Logger.Log($"The element is{(displayed ? "" : " not")} displayed", LogColors.DarkSalmon);
        return displayed;
    }

    /// <inheritdoc />
    public async Task<bool> Exists(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var exists = (await GetElements(GetFrame(data), findBy, identifier)).Count > index;
        data.Logger.Log(exists ? "The element exists" : "The element does not exist", LogColors.DarkSalmon);
        return exists;
    }

    /// <inheritdoc />
    public async Task UploadFiles(BotData data, FindElementBy findBy, string identifier, int index, List<string> filePaths)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        await elem.SetInputFilesAsync(filePaths);

        data.Logger.Log($"Uploaded {filePaths.Count} files to the element", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task<int> GetPositionX(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var x = (int)(await elem.BoundingBoxAsync())!.X;

        data.Logger.Log($"The X coordinate of the element is {x}", LogColors.DarkSalmon);
        return x;
    }

    /// <inheritdoc />
    public async Task<int> GetPositionY(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var y = (int)(await elem.BoundingBoxAsync())!.Y;

        data.Logger.Log($"The Y coordinate of the element is {y}", LogColors.DarkSalmon);
        return y;
    }

    /// <inheritdoc />
    public async Task<int> GetWidth(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var width = (int)(await elem.BoundingBoxAsync())!.Width;

        data.Logger.Log($"The width of the element is {width}", LogColors.DarkSalmon);
        return width;
    }

    /// <inheritdoc />
    public async Task<int> GetHeight(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var height = (int)(await elem.BoundingBoxAsync())!.Height;

        data.Logger.Log($"The height of the element is {height}", LogColors.DarkSalmon);
        return height;
    }

    /// <inheritdoc />
    public async Task ScreenshotElement(BotData data, FindElementBy findBy, string identifier, int index, string fileName, bool fullPage = false,
        bool omitBackground = false)
    {
        data.Logger.LogHeader();
        _ = fullPage;

        if (data.Providers.Security.RestrictBlocksToCWD)
        {
            FileUtils.ThrowIfNotInCWD(fileName);
        }

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        await elem.ScreenshotAsync(new ElementHandleScreenshotOptions
        {
            Path = fileName,
            OmitBackground = omitBackground,
            Type = omitBackground ? ScreenshotType.Png : ScreenshotType.Jpeg,
            Quality = omitBackground ? null : 100
        });

        data.Logger.Log($"Took a screenshot of the element and saved it to {fileName}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task<string> ScreenshotElementBase64(BotData data, FindElementBy findBy, string identifier, int index, bool fullPage = false,
        bool omitBackground = false)
    {
        data.Logger.LogHeader();
        _ = fullPage;

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var bytes = await elem.ScreenshotAsync(new ElementHandleScreenshotOptions
        {
            OmitBackground = omitBackground,
            Type = omitBackground ? ScreenshotType.Png : ScreenshotType.Jpeg,
            Quality = omitBackground ? null : 100
        });

        data.Logger.Log("Took a screenshot of the element as base64", LogColors.DarkSalmon);
        return Convert.ToBase64String(bytes);
    }

    /// <inheritdoc />
    public async Task SwitchToFrame(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        data.SetObject(FrameObject, await elem.ContentFrameAsync(), false);

        data.Logger.Log("Switched to iframe", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task WaitForElement(BotData data, FindElementBy findBy, string identifier, bool hidden = false, bool visible = true,
        int timeout = 30000)
    {
        data.Logger.LogHeader();

        var frame = GetFrame(data);
        await frame.WaitForSelectorAsync(BuildSelector(findBy, identifier), new FrameWaitForSelectorOptions
        {
            State = hidden
                ? WaitForSelectorState.Hidden
                : visible
                    ? WaitForSelectorState.Visible
                    : WaitForSelectorState.Attached,
            Timeout = timeout
        });

        data.Logger.Log($"Waited for element with {findBy} {identifier}", LogColors.DarkSalmon);
    }

    private static Proxy? BuildProxy(BotData data)
    {
        if (data.Proxy is null || !data.UseProxy)
        {
            return null;
        }

        return new Proxy
        {
            Server = $"{data.Proxy.Type.ToString().ToLowerInvariant()}://{data.Proxy.Host}:{data.Proxy.Port}",
            Username = data.Proxy.NeedsAuthentication ? data.Proxy.Username : null,
            Password = data.Proxy.NeedsAuthentication ? data.Proxy.Password : null
        };
    }

    private static IBrowserType GetBrowserType(IPlaywright playwright, PlaywrightBrowserType browserType)
        => browserType switch
        {
            PlaywrightBrowserType.Chromium => playwright.Chromium,
            PlaywrightBrowserType.Firefox => playwright.Firefox,
            PlaywrightBrowserType.Webkit => playwright.Webkit,
            _ => throw new NotSupportedException($"Unsupported Playwright browser type {browserType}")
        };

    private static string GetPlaywrightBrowserName(PlaywrightBrowserType browserType)
        => browserType switch
        {
            PlaywrightBrowserType.Chromium => "chromium",
            PlaywrightBrowserType.Firefox => "firefox",
            PlaywrightBrowserType.Webkit => "webkit",
            _ => throw new NotSupportedException($"Unsupported Playwright browser type {browserType}")
        };

    private static MouseButton ToPlaywrightMouseButton(BrowserMouseButton mouseButton)
        => mouseButton switch
        {
            BrowserMouseButton.Left => MouseButton.Left,
            BrowserMouseButton.Middle => MouseButton.Middle,
            BrowserMouseButton.Right => MouseButton.Right,
            _ => throw new NotSupportedException($"Unsupported mouse button {mouseButton}")
        };

    private static WaitUntilState ToPlaywrightWaitUntilState(BrowserWaitUntilNavigation waitUntil)
        => waitUntil switch
        {
            BrowserWaitUntilNavigation.Load => WaitUntilState.Load,
            BrowserWaitUntilNavigation.DOMContentLoaded => WaitUntilState.DOMContentLoaded,
            BrowserWaitUntilNavigation.Networkidle0 => WaitUntilState.NetworkIdle,
            BrowserWaitUntilNavigation.Networkidle2 => WaitUntilState.NetworkIdle,
            _ => throw new NotSupportedException($"Unsupported navigation wait mode {waitUntil}")
        };

    private static LoadState ToPlaywrightLoadState(BrowserWaitUntilNavigation waitUntil)
        => waitUntil switch
        {
            BrowserWaitUntilNavigation.Load => LoadState.Load,
            BrowserWaitUntilNavigation.DOMContentLoaded => LoadState.DOMContentLoaded,
            BrowserWaitUntilNavigation.Networkidle0 => LoadState.NetworkIdle,
            BrowserWaitUntilNavigation.Networkidle2 => LoadState.NetworkIdle,
            _ => throw new NotSupportedException($"Unsupported navigation wait mode {waitUntil}")
        };

    private static IPlaywright GetPlaywright(BotData data)
        => data.TryGetObject<IPlaywright>(PlaywrightInstanceObject) ?? throw new BlockExecutionException("The browser is not open!");

    private static IBrowser GetBrowser(BotData data)
        => data.TryGetObject<IBrowser>(BrowserObject) ?? throw new BlockExecutionException("The browser is not open!");

    private static IBrowserContext GetContext(BotData data)
        => data.TryGetObject<IBrowserContext>(ContextObject) ?? throw new BlockExecutionException("The browser is not open!");

    private static IPage GetPage(BotData data)
        => data.TryGetObject<IPage>(PageObject) ?? throw new BlockExecutionException("No pages open!");

    private static IFrame GetFrame(BotData data)
        => data.TryGetObject<IFrame>(FrameObject) ?? GetPage(data).MainFrame;

    private static void SwitchToMainFramePrivate(BotData data)
        => data.SetObject(FrameObject, GetPage(data).MainFrame, false);

    private static void SetPageAndFrame(BotData data, IPage? page)
    {
        data.SetObject(PageObject, page, false);

        if (page is null)
        {
            data.SetObject(FrameObject, null, false);
            return;
        }

        SwitchToMainFramePrivate(data);
    }

    private static async Task SetPageLoadingOptions(BotData data, IPage page)
    {
        if (data.ConfigSettings.BrowserSettings.LoadOnlyDocumentAndScript ||
            data.ConfigSettings.BrowserSettings.BlockedUrls.Any(u => !string.IsNullOrWhiteSpace(u)))
        {
            await page.RouteAsync("**/*", async route =>
            {
                if (data.ConfigSettings.BrowserSettings.LoadOnlyDocumentAndScript &&
                    route.Request.ResourceType is not "document" and not "script")
                {
                    await route.AbortAsync();
                }
                else if (data.ConfigSettings.BrowserSettings.BlockedUrls
                         .Where(u => !string.IsNullOrWhiteSpace(u))
                         .Any(u => route.Request.Url.Contains(u, StringComparison.OrdinalIgnoreCase)))
                {
                    await route.AbortAsync();
                }
                else
                {
                    await route.ContinueAsync();
                }
            });
        }

        if (data.ConfigSettings.BrowserSettings.DismissDialogs)
        {
            page.Dialog += async (_, e) =>
            {
                data.Logger.Log($"Dialog automatically dismissed: {e.Message}", LogColors.DarkSalmon);
                await e.DismissAsync();
            };
        }

        if (data.TryGetObject<string>(UserAgentObject) is { Length: > 0 } userAgent)
        {
            await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
            {
                ["User-Agent"] = userAgent
            });
        }
    }

    private static async Task<(float X, float Y)> ResolveClickPoint(IFrame frame, int x, int y)
    {
        if (frame.ParentFrame is null)
        {
            return (x, y);
        }

        var frameElement = await frame.FrameElementAsync();
        var frameBounds = await frameElement.BoundingBoxAsync()
            ?? throw new BlockExecutionException("Could not determine the current frame bounds");
        var frameClientLeft = await frameElement.EvaluateAsync<float>("element => element.clientLeft || 0");
        var frameClientTop = await frameElement.EvaluateAsync<float>("element => element.clientTop || 0");

        return (frameBounds.X + frameClientLeft + x, frameBounds.Y + frameClientTop + y);
    }

    private static async Task<IElementHandle> GetElement(IFrame frame, FindElementBy findBy, string identifier, int index)
    {
        var elements = await GetElements(frame, findBy, identifier);

        if (elements.Count < index + 1)
        {
            throw new BlockExecutionException($"Expected at least {index + 1} elements to be found but {elements.Count} were found");
        }

        return elements[index];
    }

    private static Task<IReadOnlyList<IElementHandle>> GetElements(IFrame frame, FindElementBy findBy, string identifier)
        => frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier));

    private static string BuildSelector(FindElementBy findBy, string identifier)
        => findBy switch
        {
            FindElementBy.Id => '#' + identifier,
            FindElementBy.Class => '.' + string.Join('.', identifier.Split(' ')),
            FindElementBy.Selector => identifier,
            FindElementBy.XPath => $"xpath={identifier}",
            _ => throw new NotSupportedException()
        };

    private static bool UrlsMatch(string actual, string expected)
    {
        if (!Uri.TryCreate(actual, UriKind.Absolute, out var actualUri) ||
            !Uri.TryCreate(expected, UriKind.Absolute, out var expectedUri))
        {
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(actualUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.PathAndQuery, UriFormat.Unescaped),
                             expectedUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.PathAndQuery, UriFormat.Unescaped),
                             StringComparison.OrdinalIgnoreCase);
    }

    private static bool ResponseCanHaveBody(IResponse response)
    {
        var statusCode = response.Status;
        return statusCode is not (>= 100 and < 200 or 204 or 205 or 304) &&
               statusCode / 100 != 3;
    }

    private static async Task TryPopulateResponseBody(BotData data, IResponse response)
    {
        try
        {
            data.SOURCE = await response.TextAsync();
            data.RAWSOURCE = await response.BodyAsync();
        }
        catch (Exception ex) when (IsMissingResponseBodyException(ex))
        {
            data.SOURCE = string.Empty;
            data.RAWSOURCE = [];
            data.Logger.Log("Response body is not available", LogColors.Orange);
        }
    }

    private static bool IsMissingResponseBodyException(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains("Unable to retrieve body", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("No resource with given identifier found", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string SerializeJavaScriptResult(object? value)
        => value switch
        {
            null => "undefined",
            JsonElement jsonElement => SerializeJsonElement(jsonElement),
            string stringValue => stringValue,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => JsonConvert.SerializeObject(value)
        };

    private static string SerializeJsonElement(JsonElement value)
        => value.ValueKind switch
        {
            JsonValueKind.Undefined or JsonValueKind.Null => "undefined",
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.True or JsonValueKind.False => value.GetBoolean().ToString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => value.GetRawText()
        };

    private static class PlaywrightBrowserInstaller
    {
        private static readonly SemaphoreSlim InstallSemaphore = new(1, 1);

        public static async Task EnsureBrowserInstalledAsync(IBrowserType browserType, string browserName,
            CancellationToken cancellationToken)
        {
            if (File.Exists(browserType.ExecutablePath))
            {
                return;
            }

            await InstallSemaphore.WaitAsync(cancellationToken);

            try
            {
                if (File.Exists(browserType.ExecutablePath))
                {
                    return;
                }

                var exitCode = await Task.Run(() => Microsoft.Playwright.Program.Main(["install", browserName]), cancellationToken);

                if (exitCode != 0)
                {
                    throw new BlockExecutionException($"Playwright failed to install the managed {browserName} browser (exit code {exitCode})");
                }
            }
            finally
            {
                InstallSemaphore.Release();
            }
        }
    }
}
