using GhostCursorSharp;
using Newtonsoft.Json;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using RuriLib.Exceptions;
using RuriLib.Functions.Browser;
using RuriLib.Functions.Files;
using RuriLib.Functions.Puppeteer;
using RuriLib.Helpers;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs.Settings;
using RuriLib.Providers.Puppeteer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Yove.Proxy;

using ProxyType = RuriLib.Models.Proxies.ProxyType;

namespace RuriLib.Providers.Browser;

/// <summary>
/// Browser automation engine backed by PuppeteerSharp.
/// </summary>
public class PuppeteerBrowserAutomationEngine : IBrowserAutomationEngine
{
    private const string GhostCursorObject = "puppeteerGhostCursor";
    private const string RandomMovesObject = "browserGhostCursorRandomMovesEnabled";

    private readonly IPuppeteerBrowserProvider _puppeteerBrowserProvider;

    /// <summary>
    /// Creates the engine with the configured Puppeteer provider.
    /// </summary>
    public PuppeteerBrowserAutomationEngine(IPuppeteerBrowserProvider puppeteerBrowserProvider)
    {
        _puppeteerBrowserProvider = puppeteerBrowserProvider;
    }

    /// <inheritdoc />
    public async Task OpenBrowser(BotData data, string extraCmdLineArgs = "")
    {
        data.Logger.LogHeader();

        var oldBrowser = data.TryGetObject<IBrowser>("puppeteer");
        if (oldBrowser is not null && !oldBrowser.IsClosed)
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

        if (data.Proxy != null && data.UseProxy)
        {
            if (data.Proxy.Type is ProxyType.Http or ProxyType.Https || !data.Proxy.NeedsAuthentication)
            {
                args.Add($"--proxy-server={data.Proxy.Type.ToString().ToLower()}://{data.Proxy.Host}:{data.Proxy.Port}");
            }
            else
            {
                var proxyType = data.Proxy.Type == ProxyType.Socks5 ? Yove.Proxy.ProxyType.Socks5 : Yove.Proxy.ProxyType.Socks4;
                var proxyClient = new ProxyClient(
                    data.Proxy.Host, data.Proxy.Port,
                    data.Proxy.Username, data.Proxy.Password,
                    proxyType);
                data.SetObject("puppeteer.yoveproxy", proxyClient);
                args.Add($"--proxy-server={proxyClient.GetProxy(null!)!.Authority}");
            }
        }

        var launchOptions = new LaunchOptions
        {
            Args = [.. args],
            ExecutablePath = _puppeteerBrowserProvider.ChromeBinaryLocation,
            IgnoredDefaultArgs = ["--disable-extensions", "--enable-automation"],
            Headless = data.ConfigSettings.BrowserSettings.Headless,
            AcceptInsecureCerts = data.ConfigSettings.BrowserSettings.IgnoreHttpsErrors,
            DefaultViewport = null
        };

        var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);

        data.SetObject("puppeteer", browser);
        var page = (await browser.PagesAsync()).First();
        SetPageAndFrame(data, page);
        await SetPageLoadingOptions(data, page);

        if (data is { UseProxy: true, Proxy: { NeedsAuthentication: true, Type: ProxyType.Http or ProxyType.Https } proxy })
        {
            await page.AuthenticateAsync(new Credentials { Username = proxy.Username, Password = proxy.Password });
        }

        await EnsureGhostCursorReadyAsync(data);

        data.Logger.Log($"{(launchOptions.Headless ? "Headless " : "")}Browser opened successfully!", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task CloseBrowser(BotData data)
    {
        data.Logger.LogHeader();

        await ResetGhostCursorAsync(data, resetRandomMoves: true);
        var browser = GetBrowser(data);
        await browser.CloseAsync();
        StopYoveProxyInternalServer(data);
        data.SetObject("puppeteer", null, false);
        data.SetObject("puppeteerPage", null, false);
        data.SetObject("puppeteerFrame", null, false);
        data.Logger.Log("Browser closed successfully!", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task NewTab(BotData data)
    {
        data.Logger.LogHeader();

        var browser = GetBrowser(data);
        await ResetGhostCursorAsync(data, resetRandomMoves: false);
        var page = await browser.NewPageAsync();
        await SetPageLoadingOptions(data, page);

        SetPageAndFrame(data, page);
        await EnsureGhostCursorReadyAsync(data);
        data.Logger.Log("Opened a new page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task CloseTab(BotData data)
    {
        data.Logger.LogHeader();

        var browser = GetBrowser(data);
        var page = GetPage(data);

        await ResetGhostCursorAsync(data, resetRandomMoves: false);
        await page.CloseAsync();

        page = (await browser.PagesAsync()).FirstOrDefault(p => !p.IsClosed);
        SetPageAndFrame(data, page);

        if (page is not null)
        {
            await page.BringToFrontAsync();
        }

        await EnsureGhostCursorReadyAsync(data);

        data.Logger.Log("Closed the active page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task SwitchToTab(BotData data, int index)
    {
        data.Logger.LogHeader();

        var browser = GetBrowser(data);
        await browser.GetVersionAsync();

        var pages = await browser.PagesAsync();
        var page = pages[index];

        await ResetGhostCursorAsync(data, resetRandomMoves: false);
        await page.BringToFrontAsync();
        SetPageAndFrame(data, page);
        await EnsureGhostCursorReadyAsync(data);

        data.Logger.Log($"Switched to tab with index {index}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task Reload(BotData data)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.ReloadAsync();
        SwitchToMainFramePrivate(data);
        await EnsureGhostCursorReadyAsync(data);

        data.Logger.Log("Reloaded the page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task GoBack(BotData data)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.GoBackAsync();
        SwitchToMainFramePrivate(data);
        await EnsureGhostCursorReadyAsync(data);

        data.Logger.Log("Went back to the previously visited page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task GoForward(BotData data)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.GoForwardAsync();
        SwitchToMainFramePrivate(data);
        await EnsureGhostCursorReadyAsync(data);

        data.Logger.Log("Went forward to the next visited page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task NavigateTo(BotData data, string url = "https://example.com",
        BrowserWaitUntilNavigation loadedEvent = BrowserWaitUntilNavigation.Load, string referer = "", int timeout = 30000)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        var options = new NavigationOptions
        {
            Timeout = timeout,
            Referer = referer,
            WaitUntil = [ToPuppeteerWaitUntilNavigation(loadedEvent)]
        };
        var response = await page.GoToAsync(url, options);
        data.ADDRESS = response?.Url ?? page.Url;

        if (response is not null)
        {
            data.SOURCE = await response.TextAsync();
            data.RAWSOURCE = await response.BufferAsync();
        }
        else
        {
            data.SOURCE = await page.GetContentAsync();
            data.RAWSOURCE = Encoding.UTF8.GetBytes(data.SOURCE);
        }

        SwitchToMainFramePrivate(data);
        await EnsureGhostCursorReadyAsync(data);

        data.Logger.Log($"Navigated to {url}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task WaitForNavigation(BotData data, BrowserWaitUntilNavigation loadedEvent = BrowserWaitUntilNavigation.Load, int timeout = 30000)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        var options = new NavigationOptions
        {
            Timeout = timeout,
            WaitUntil = [ToPuppeteerWaitUntilNavigation(loadedEvent)]
        };

        await page.WaitForNavigationAsync(options);
        data.ADDRESS = page.Url;
        data.SOURCE = await page.GetContentAsync();
        data.RAWSOURCE = Encoding.UTF8.GetBytes(data.SOURCE);
        SwitchToMainFramePrivate(data);
        await EnsureGhostCursorReadyAsync(data);

        data.Logger.Log("Waited for navigation to complete", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task ClearCookies(BotData data, string website)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        var cookies = await page.GetCookiesAsync(website);
        await page.DeleteCookieAsync(cookies);
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

        var frame = GetFrame(data);
        var (clickX, clickY) = await ResolveClickPoint(frame, x, y);

        if (ShouldUseGhostCursor(data))
        {
            var cursor = await GetOrCreateGhostCursorAsync(data);
            await cursor.MoveToAsync(new Vector(Convert.ToDouble(clickX), Convert.ToDouble(clickY)),
                GhostCursorOptionsBuilder.BuildMoveToOptions(data.ConfigSettings.BrowserSettings));
            await cursor.ClickAsync(BuildGhostCursorClickOptions(data.ConfigSettings.BrowserSettings, mouseButton, clickCount, timeBetweenClicks));
        }
        else
        {
            var page = GetPage(data);
            await page.Mouse.ClickAsync(clickX, clickY, new PuppeteerSharp.Input.ClickOptions
            {
                Button = ToPuppeteerMouseButton(mouseButton),
                Count = clickCount,
                Delay = timeBetweenClicks
            });
        }

        data.Logger.Log($"Clicked {clickCount} time(s) with {mouseButton} button at ({x}, {y})", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task MoveCursorToCoordinates(BotData data, int x, int y)
    {
        data.Logger.LogHeader();

        var frame = GetFrame(data);
        var (moveX, moveY) = await ResolveClickPoint(frame, x, y);

        if (ShouldUseGhostCursor(data))
        {
            var cursor = await GetOrCreateGhostCursorAsync(data);
            await cursor.MoveToAsync(new Vector(Convert.ToDouble(moveX), Convert.ToDouble(moveY)),
                GhostCursorOptionsBuilder.BuildMoveToOptions(data.ConfigSettings.BrowserSettings));
        }
        else
        {
            await GetPage(data).Mouse.MoveAsync(moveX, moveY);
        }

        data.Logger.Log($"Moved the cursor to ({x}, {y})", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task MouseDown(BotData data, BrowserMouseButton mouseButton = BrowserMouseButton.Left, int clickCount = 1)
    {
        data.Logger.LogHeader();

        if (ShouldUseGhostCursor(data))
        {
            var cursor = await GetOrCreateGhostCursorAsync(data);
            await cursor.MouseDownAsync(BuildGhostCursorClickOptions(data.ConfigSettings.BrowserSettings, mouseButton, clickCount));
        }
        else
        {
            await GetPage(data).Mouse.DownAsync(new PuppeteerSharp.Input.ClickOptions
            {
                Button = ToPuppeteerMouseButton(mouseButton),
                Count = clickCount
            });
        }

        data.Logger.Log($"Pressed the {mouseButton} mouse button", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task MouseUp(BotData data, BrowserMouseButton mouseButton = BrowserMouseButton.Left, int clickCount = 1)
    {
        data.Logger.LogHeader();

        if (ShouldUseGhostCursor(data))
        {
            var cursor = await GetOrCreateGhostCursorAsync(data);
            await cursor.MouseUpAsync(BuildGhostCursorClickOptions(data.ConfigSettings.BrowserSettings, mouseButton, clickCount));
        }
        else
        {
            await GetPage(data).Mouse.UpAsync(new PuppeteerSharp.Input.ClickOptions
            {
                Button = ToPuppeteerMouseButton(mouseButton),
                Count = clickCount
            });
        }

        data.Logger.Log($"Released the {mouseButton} mouse button", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task ToggleRandomMouseMoves(BotData data, bool enabled)
    {
        data.Logger.LogHeader();

        SetGhostCursorRandomMovesEnabled(data, enabled);

        if (enabled)
        {
            var cursor = await GetOrCreateGhostCursorAsync(data);
            cursor.ToggleRandomMove(true);
        }
        else
        {
            data.TryGetObject<GhostCursor>(GhostCursorObject)?.ToggleRandomMove(false);
        }

        data.Logger.Log(enabled
            ? "Enabled random mouse movement"
            : "Disabled random mouse movement",
            LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task InjectMousePositionHelper(BotData data)
    {
        data.Logger.LogHeader();

        var cursor = await GetOrCreateGhostCursorAsync(data);
        await cursor.InstallMouseHelperAsync();

        data.Logger.Log("Injected the mouse position helper into the current page", LogColors.DarkSalmon);
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
        var options = new ScreenshotOptions
        {
            FullPage = fullPage,
            OmitBackground = omitBackground,
            Type = omitBackground ? ScreenshotType.Png : ScreenshotType.Jpeg,
            Quality = omitBackground ? null : 100
        };
        await page.ScreenshotAsync(file, options);
        data.Logger.Log($"Took a screenshot of the {(fullPage ? "full" : "visible")} page and saved it to {file}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task<string> ScreenshotPageBase64(BotData data, bool fullPage = false, bool omitBackground = false)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        var options = new ScreenshotOptions
        {
            FullPage = fullPage,
            OmitBackground = omitBackground,
            Type = omitBackground ? ScreenshotType.Png : ScreenshotType.Jpeg,
            Quality = omitBackground ? null : 100
        };
        var base64 = await page.ScreenshotBase64Async(options);
        data.Logger.Log($"Took a screenshot of the {(fullPage ? "full" : "visible")} page as base64", LogColors.DarkSalmon);
        return base64;
    }

    /// <inheritdoc />
    public async Task ScrollToTop(BotData data)
    {
        data.Logger.LogHeader();

        if (ShouldUseGhostCursor(data))
        {
            var cursor = await GetOrCreateGhostCursorAsync(data);
            await cursor.ScrollToAsync("top", GhostCursorOptionsBuilder.BuildScrollOptions(data.ConfigSettings.BrowserSettings));
        }
        else
        {
            var page = GetPage(data);
            await page.EvaluateExpressionAsync("window.scrollTo(0, 0);");
        }

        data.Logger.Log("Scrolled to the top of the page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task ScrollToBottom(BotData data)
    {
        data.Logger.LogHeader();

        if (ShouldUseGhostCursor(data))
        {
            var cursor = await GetOrCreateGhostCursorAsync(data);
            await cursor.ScrollToAsync("bottom", GhostCursorOptionsBuilder.BuildScrollOptions(data.ConfigSettings.BrowserSettings));
        }
        else
        {
            var page = GetPage(data);
            await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight);");
        }

        data.Logger.Log("Scrolled to the bottom of the page", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task ScrollBy(BotData data, int horizontalScroll, int verticalScroll)
    {
        data.Logger.LogHeader();

        if (ShouldUseGhostCursor(data))
        {
            var cursor = await GetOrCreateGhostCursorAsync(data);
            await cursor.ScrollAsync(
                new Vector(horizontalScroll, verticalScroll),
                GhostCursorOptionsBuilder.BuildScrollOptions(data.ConfigSettings.BrowserSettings));
        }
        else
        {
            var page = GetPage(data);
            await page.EvaluateExpressionAsync($"window.scrollBy({horizontalScroll}, {verticalScroll});");
        }

        data.Logger.Log($"Scrolled by ({horizontalScroll}, {verticalScroll})", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task SetViewport(BotData data, int width, int height, bool isMobile = false, bool isLandscape = false, float scaleFactor = 1f)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);

        var options = new ViewPortOptions
        {
            Width = width,
            Height = height,
            IsMobile = isMobile,
            IsLandscape = isLandscape,
            DeviceScaleFactor = scaleFactor
        };

        await page.SetViewportAsync(options);

        data.Logger.Log($"Set the viewport size to {width}x{height}", LogColors.DarkSalmon);
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
        var dom = await page.EvaluateExpressionAsync<string>("document.body.innerHTML");

        data.Logger.Log("Got the full page DOM", LogColors.DarkSalmon);
        data.Logger.Log(dom, LogColors.DarkSalmon, true);
        return dom;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> GetCookies(BotData data, string domain)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        var cookies = await page.GetCookiesAsync();

        if (!string.IsNullOrWhiteSpace(domain))
        {
            cookies = cookies.Where(c => c.Domain.Contains(domain, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        data.Logger.Log($"Got {cookies.Length} cookies for {(string.IsNullOrWhiteSpace(domain) ? "all domains" : domain)}", LogColors.DarkSalmon);
        return cookies.ToDictionary(c => c.Name, c => c.Value);
    }

    /// <inheritdoc />
    public async Task SetCookies(BotData data, string domain, Dictionary<string, string> cookies)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.SetCookieAsync(cookies.Select(c => new CookieParam { Domain = domain, Name = c.Key, Value = c.Value }).ToArray());

        data.Logger.Log($"Set {cookies.Count} cookies for {domain}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task SetUserAgent(BotData data, string userAgent)
    {
        data.Logger.LogHeader();

        var page = GetPage(data);
        await page.SetUserAgentAsync(new SetUserAgentOptions
        {
            UserAgent = userAgent
        });

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
        await using var response = await frame.EvaluateExpressionHandleAsync(expression);
        var value = await response.JsonValueAsync<object>();
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
        var options = new WaitForOptions
        {
            Timeout = timeoutMilliseconds
        };

        var response = await page.WaitForResponseAsync(r => UrlsMatch(r.Url, url), options);

        data.ADDRESS = response.Url;
        data.RESPONSECODE = (int)response.Status;
        data.HEADERS = response.Headers;
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
        var script = "(element, attributeName, value) => element.setAttribute(attributeName, value)";
        await elem.EvaluateFunctionAsync(script, attributeName, value);

        data.Logger.Log($"Set value {value} of attribute {attributeName} by executing {script}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task TypeElement(BotData data, FindElementBy findBy, string identifier, int index, string text, int timeBetweenKeystrokes = 0)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        await elem.TypeAsync(text, new PuppeteerSharp.Input.TypeOptions { Delay = timeBetweenKeystrokes });

        data.Logger.Log($"Typed {text}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task TypeElementHuman(BotData data, FindElementBy findBy, string identifier, int index, string text)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);

        foreach (var c in text)
        {
            await elem.TypeAsync(c.ToString());
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

        if (ShouldUseGhostCursor(data))
        {
            var cursor = await GetOrCreateGhostCursorAsync(data);
            await cursor.ClickAsync(elem, BuildGhostCursorClickOptions(data.ConfigSettings.BrowserSettings, mouseButton, clickCount, timeBetweenClicks));
        }
        else
        {
            await elem.ClickAsync(new PuppeteerSharp.Input.ClickOptions
            {
                Button = ToPuppeteerMouseButton(mouseButton),
                Count = clickCount,
                Delay = timeBetweenClicks
            });
        }

        data.Logger.Log($"Clicked {clickCount} time(s) with {mouseButton} button", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task MoveCursorToElement(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);

        if (ShouldUseGhostCursor(data))
        {
            var cursor = await GetOrCreateGhostCursorAsync(data);
            await cursor.MoveAsync(elem, GhostCursorOptionsBuilder.BuildMoveOptions(data.ConfigSettings.BrowserSettings));
        }
        else
        {
            var box = await elem.BoundingBoxAsync() ?? throw new BlockExecutionException("Could not determine the element bounds");
            await GetPage(data).Mouse.MoveAsync(box.X + (box.Width / 2), box.Y + (box.Height / 2));
        }

        data.Logger.Log("Moved the cursor to the element", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task Submit(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var script = """
                     element => {
                         const form = element.tagName === 'FORM' ? element : element.form;
                         if (!form) {
                             throw new Error('The selected element is not associated with a form');
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
        await elem.EvaluateFunctionAsync(script);

        data.Logger.Log($"Submitted the form by executing {script}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task Select(BotData data, FindElementBy findBy, string identifier, int index, string value)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        await elem.SelectAsync(value);

        data.Logger.Log($"Selected value {value}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task SelectByIndex(BotData data, FindElementBy findBy, string identifier, int index, int selectionIndex)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var value = await elem.EvaluateFunctionAsync<string>(
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

        await elem.SelectAsync(value);

        data.Logger.Log($"Selected value {value}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task SelectByText(BotData data, FindElementBy findBy, string identifier, int index, string text)
    {
        data.Logger.LogHeader();

        var frame = GetFrame(data);
        var elemScript = GetElementScript(findBy, identifier, index);
        var script = $"el={elemScript};for(let i=0;i<el.options.length;i++){{if(el.options[i].text=={ToJavaScriptStringLiteral(text)}){{el.selectedIndex = i;break;}}}}";
        await frame.EvaluateExpressionAsync(script);

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
        var value = await elem.EvaluateFunctionAsync<string>(script, attributeName);

        data.Logger.Log($"Got value {value} of attribute {attributeName} by executing {script}", LogColors.DarkSalmon);
        return value;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetAttributeValueAll(BotData data, FindElementBy findBy, string identifier, string attributeName = "innerText")
    {
        data.Logger.LogHeader();

        var elemScript = GetElementsScript(findBy, identifier);
        var frame = GetFrame(data);
        var script = $"Array.prototype.slice.call({elemScript}).map((item) => item.{attributeName})";
        var values = await frame.EvaluateExpressionAsync<string[]>(script);

        data.Logger.Log($"Got {values.Length} values for attribute {attributeName} by executing {script}", LogColors.DarkSalmon);
        return values.ToList();
    }

    /// <inheritdoc />
    public async Task<bool> IsDisplayed(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elemScript = GetElementScript(findBy, identifier, index);
        var frame = GetFrame(data);
        var script = $"window.getComputedStyle({elemScript}).display !== 'none';";
        var displayed = await frame.EvaluateExpressionAsync<bool>(script);

        data.Logger.Log($"Found out the element is{(displayed ? "" : " not")} displayed by executing {script}", LogColors.DarkSalmon);
        return displayed;
    }

    /// <inheritdoc />
    public async Task<bool> Exists(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elemScript = GetElementScript(findBy, identifier, index);
        var frame = GetFrame(data);
        var script = $"window.getComputedStyle({elemScript}).display !== 'none';";

        try
        {
            await frame.EvaluateExpressionAsync<bool>(script);
            data.Logger.Log("The element exists", LogColors.DarkSalmon);
            return true;
        }
        catch
        {
            data.Logger.Log("The element does not exist", LogColors.DarkSalmon);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task UploadFiles(BotData data, FindElementBy findBy, string identifier, int index, List<string> filePaths)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        await elem.UploadFileAsync(filePaths.ToArray());

        data.Logger.Log($"Uploaded {filePaths.Count} files to the element", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task<int> GetPositionX(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var x = (int)(await elem.BoundingBoxAsync()).X;

        data.Logger.Log($"The X coordinate of the element is {x}", LogColors.DarkSalmon);
        return x;
    }

    /// <inheritdoc />
    public async Task<int> GetPositionY(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var y = (int)(await elem.BoundingBoxAsync()).Y;

        data.Logger.Log($"The Y coordinate of the element is {y}", LogColors.DarkSalmon);
        return y;
    }

    /// <inheritdoc />
    public async Task<int> GetWidth(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var width = (int)(await elem.BoundingBoxAsync()).Width;

        data.Logger.Log($"The width of the element is {width}", LogColors.DarkSalmon);
        return width;
    }

    /// <inheritdoc />
    public async Task<int> GetHeight(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var height = (int)(await elem.BoundingBoxAsync()).Height;

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
            FileUtils.ThrowIfNotInCWD(fileName);

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var page = GetPage(data);
        var options = await BuildElementScreenshotOptions(elem, omitBackground);
        await page.ScreenshotAsync(fileName, options);

        data.Logger.Log($"Took a screenshot of the element and saved it to {fileName}", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task<string> ScreenshotElementBase64(BotData data, FindElementBy findBy, string identifier, int index, bool fullPage = false,
        bool omitBackground = false)
    {
        data.Logger.LogHeader();
        _ = fullPage;

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        var page = GetPage(data);
        var options = await BuildElementScreenshotOptions(elem, omitBackground);
        var base64 = await page.ScreenshotBase64Async(options);

        data.Logger.Log("Took a screenshot of the element as base64", LogColors.DarkSalmon);
        return base64;
    }

    /// <inheritdoc />
    public async Task SwitchToFrame(BotData data, FindElementBy findBy, string identifier, int index)
    {
        data.Logger.LogHeader();

        var elem = await GetElement(GetFrame(data), findBy, identifier, index);
        data.SetObject("puppeteerFrame", await elem.ContentFrameAsync());

        data.Logger.Log("Switched to iframe", LogColors.DarkSalmon);
    }

    /// <inheritdoc />
    public async Task WaitForElement(BotData data, FindElementBy findBy, string identifier, bool hidden = false, bool visible = true,
        int timeout = 30000)
    {
        data.Logger.LogHeader();

        var frame = GetFrame(data);
        var options = new WaitForSelectorOptions { Hidden = hidden, Visible = visible, Timeout = timeout };

        if (findBy == FindElementBy.XPath)
        {
            await frame.WaitForXPathAsync(identifier, options);
        }
        else
        {
            await frame.WaitForSelectorAsync(BuildSelector(findBy, identifier), options);
        }

        data.Logger.Log($"Waited for element with {findBy} {identifier}", LogColors.DarkSalmon);
    }

    private static PuppeteerSharp.Input.MouseButton ToPuppeteerMouseButton(BrowserMouseButton mouseButton)
        => mouseButton switch
        {
            BrowserMouseButton.Left => PuppeteerSharp.Input.MouseButton.Left,
            BrowserMouseButton.Middle => PuppeteerSharp.Input.MouseButton.Middle,
            BrowserMouseButton.Right => PuppeteerSharp.Input.MouseButton.Right,
            _ => throw new NotSupportedException($"Unsupported mouse button {mouseButton}")
        };

    private static WaitUntilNavigation ToPuppeteerWaitUntilNavigation(BrowserWaitUntilNavigation waitUntil)
        => waitUntil switch
        {
            BrowserWaitUntilNavigation.Load => WaitUntilNavigation.Load,
            BrowserWaitUntilNavigation.DOMContentLoaded => WaitUntilNavigation.DOMContentLoaded,
            BrowserWaitUntilNavigation.Networkidle0 => WaitUntilNavigation.Networkidle0,
            BrowserWaitUntilNavigation.Networkidle2 => WaitUntilNavigation.Networkidle2,
            _ => throw new NotSupportedException($"Unsupported navigation wait mode {waitUntil}")
        };

    private static IBrowser GetBrowser(BotData data)
        => data.TryGetObject<IBrowser>("puppeteer") ?? throw new BlockExecutionException("The browser is not open!");

    private static IPage GetPage(BotData data)
        => data.TryGetObject<IPage>("puppeteerPage") ?? throw new BlockExecutionException("No pages open!");

    private static IFrame GetFrame(BotData data)
        => data.TryGetObject<IFrame>("puppeteerFrame") ?? GetPage(data).MainFrame;

    private static void SwitchToMainFramePrivate(BotData data)
        => data.SetObject("puppeteerFrame", GetPage(data).MainFrame);

    private static void SetPageAndFrame(BotData data, IPage? page)
    {
        data.SetObject("puppeteerPage", page, false);

        if (page is null)
        {
            data.SetObject("puppeteerFrame", null, false);
            return;
        }

        SwitchToMainFramePrivate(data);
    }

    private static void StopYoveProxyInternalServer(BotData data)
        => data.TryGetObject<ProxyClient>("puppeteer.yoveproxy")?.Dispose();

    private async Task EnsureGhostCursorReadyAsync(BotData data)
    {
        if (ShouldUseGhostCursor(data) && IsGhostCursorRandomMovesEnabled(data))
        {
            _ = await GetOrCreateGhostCursorAsync(data);
        }
    }

    private async Task<GhostCursor> GetOrCreateGhostCursorAsync(BotData data)
    {
        var page = GetPage(data);
        var existing = data.TryGetObject<GhostCursor>(GhostCursorObject);

        if (existing?.Page == page)
        {
            return existing;
        }

        if (existing is not null)
        {
            await ResetGhostCursorAsync(data, resetRandomMoves: false);
        }

        var cursor = new GhostCursor(page, GhostCursorOptionsBuilder.BuildCursorOptions(
            data.ConfigSettings.BrowserSettings,
            IsGhostCursorRandomMovesEnabled(data)));

        data.SetObject(GhostCursorObject, cursor, false);
        return cursor;
    }

    private async Task ResetGhostCursorAsync(BotData data, bool resetRandomMoves)
    {
        var cursor = data.TryGetObject<GhostCursor>(GhostCursorObject);

        if (cursor is not null)
        {
            try
            {
                cursor.ToggleRandomMove(false);
                await cursor.RemoveMouseHelperAsync();
            }
            catch
            {
                // ignored
            }
        }

        data.SetObject(GhostCursorObject, null, false);

        if (resetRandomMoves)
        {
            SetGhostCursorRandomMovesEnabled(data, false);
        }
    }

    private static bool ShouldUseGhostCursor(BotData data)
        => data.ConfigSettings.BrowserSettings.MouseAutomationMode == BrowserMouseAutomationMode.GhostCursor;

    private static ClickOptions BuildGhostCursorClickOptions(
        BrowserSettings settings,
        BrowserMouseButton mouseButton,
        int clickCount,
        int? waitForClick = null)
        => GhostCursorOptionsBuilder.BuildClickOptions(settings, ToGhostCursorMouseButton(mouseButton), clickCount, waitForClick);

    private static GhostCursorSharp.MouseButton ToGhostCursorMouseButton(BrowserMouseButton mouseButton)
        => mouseButton switch
        {
            BrowserMouseButton.Left => GhostCursorSharp.MouseButton.Left,
            BrowserMouseButton.Middle => GhostCursorSharp.MouseButton.Middle,
            BrowserMouseButton.Right => GhostCursorSharp.MouseButton.Right,
            _ => throw new NotSupportedException($"Unsupported mouse button {mouseButton}")
        };

    private static bool IsGhostCursorRandomMovesEnabled(BotData data)
        => data.TryGetObject<StrongBox<bool>>(RandomMovesObject)?.Value ?? false;

    private static void SetGhostCursorRandomMovesEnabled(BotData data, bool enabled)
        => data.SetObject(RandomMovesObject, new StrongBox<bool>(enabled), false);

    private async Task SetPageLoadingOptions(BotData data, IPage page)
    {
        page.Load += async (_, _) => await TryRestoreGhostCursorAfterNavigationAsync(data);
        await page.SetRequestInterceptionAsync(true);
        page.Request += (_, e) =>
        {
            if (data.ConfigSettings.BrowserSettings.LoadOnlyDocumentAndScript &&
                e.Request.ResourceType != ResourceType.Document && e.Request.ResourceType != ResourceType.Script)
            {
                e.Request.AbortAsync();
            }
            else if (data.ConfigSettings.BrowserSettings.BlockedUrls
                     .Where(u => !string.IsNullOrWhiteSpace(u))
                     .Any(u => e.Request.Url.Contains(u, StringComparison.OrdinalIgnoreCase)))
            {
                e.Request.AbortAsync();
            }
            else
            {
                e.Request.ContinueAsync();
            }
        };

        if (data.ConfigSettings.BrowserSettings.DismissDialogs)
        {
            page.Dialog += (_, e) =>
            {
                data.Logger.Log($"Dialog automatically dismissed: {e.Dialog.Message}", LogColors.DarkSalmon);
                e.Dialog.Dismiss();
            };
        }
    }

    private async Task TryRestoreGhostCursorAfterNavigationAsync(BotData data)
    {
        try
        {
            await EnsureGhostCursorReadyAsync(data);
        }
        catch
        {
        }
    }

    private static async Task<(decimal X, decimal Y)> ResolveClickPoint(IFrame frame, int x, int y)
    {
        if (frame.ParentFrame is null)
        {
            return (x, y);
        }

        var frameElement = await frame.FrameElementAsync();
        var frameBounds = await frameElement.BoundingBoxAsync()
            ?? throw new BlockExecutionException("Could not determine the current frame bounds");
        var frameClientLeft = await frameElement.EvaluateFunctionAsync<decimal>("element => element.clientLeft || 0");
        var frameClientTop = await frameElement.EvaluateFunctionAsync<decimal>("element => element.clientTop || 0");

        return (frameBounds.X + frameClientLeft + x, frameBounds.Y + frameClientTop + y);
    }

    private static async Task<IElementHandle> GetElement(IFrame frame, FindElementBy findBy, string identifier, int index)
    {
        var elements = findBy is FindElementBy.XPath
            ? await frame.XPathAsync(identifier)
            : await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier));

        if (elements.Length < index + 1)
        {
            throw new BlockExecutionException($"Expected at least {index + 1} elements to be found but {elements.Length} were found");
        }

        return elements[index];
    }

    private static string GetElementsScript(FindElementBy findBy, string identifier)
    {
        if (findBy == FindElementBy.XPath)
        {
            var script = $"document.evaluate({ToJavaScriptStringLiteral(identifier)}, document, null, XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null)";
            return $"Array.from({{ length: {script}.snapshotLength }}, (_, index) => {script}.snapshotItem(index))";
        }

        return $"document.querySelectorAll({ToJavaScriptStringLiteral(BuildSelector(findBy, identifier))})";
    }

    private static string GetElementScript(FindElementBy findBy, string identifier, int index)
        => findBy == FindElementBy.XPath
            ? $"document.evaluate({ToJavaScriptStringLiteral(identifier)}, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue"
            : $"document.querySelectorAll({ToJavaScriptStringLiteral(BuildSelector(findBy, identifier))})[{index}]";

    private static string BuildSelector(FindElementBy findBy, string identifier)
        => findBy switch
        {
            FindElementBy.Id => '#' + identifier,
            FindElementBy.Class => '.' + string.Join('.', identifier.Split(' ')),
            FindElementBy.Selector => identifier,
            _ => throw new NotSupportedException()
        };

    private static string ToJavaScriptStringLiteral(string value)
        => $"'{value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n")}'";

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
        var statusCode = (int)response.Status;
        return statusCode is not (>= 100 and < 200 or 204 or 205 or 304) &&
               statusCode / 100 != 3;
    }

    private static async Task TryPopulateResponseBody(BotData data, IResponse response)
    {
        try
        {
            data.SOURCE = await response.TextAsync();
            data.RAWSOURCE = await response.BufferAsync();
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
            if (current.Message.Contains("Unable to get response body", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string SerializeJavaScriptResult(object value)
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

    private static async Task<ScreenshotOptions> BuildElementScreenshotOptions(IElementHandle element, bool omitBackground)
    {
        await element.ScrollIntoViewAsync();
        var boundingBox = await element.BoundingBoxAsync() ?? throw new BlockExecutionException("Could not determine the element bounds");

        return new ScreenshotOptions
        {
            Clip = new Clip
            {
                X = boundingBox.X,
                Y = boundingBox.Y,
                Width = boundingBox.Width,
                Height = boundingBox.Height
            },
            CaptureBeyondViewport = false,
            FromSurface = false,
            OptimizeForSpeed = true,
            OmitBackground = omitBackground,
            Type = omitBackground ? ScreenshotType.Png : ScreenshotType.Jpeg,
            Quality = omitBackground ? null : 100
        };
    }
}
