using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using RuriLib.Exceptions;
using RuriLib.Extensions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Proxies;
using RuriLib.Tests.Utils;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;
using FindElementBy = RuriLib.Functions.Puppeteer.FindElementBy;
using RuriProxy = RuriLib.Models.Proxies.Proxy;
using SeleniumBrowserMethods = RuriLib.Blocks.Selenium.Browser.Methods;
using SeleniumElementMethods = RuriLib.Blocks.Selenium.Elements.Methods;
using SeleniumPageMethods = RuriLib.Blocks.Selenium.Page.Methods;

namespace RuriLib.Tests.Blocks;

[Collection(nameof(BrowserProxyServerCollection))]
[Trait("Category", "BrowserIntegration")]
public class SeleniumBrowserBlockIntegrationTests
{
    public static IEnumerable<object[]> BrowserKinds()
    {
        yield return [TestSeleniumBrowser.Chromium];
        yield return [TestSeleniumBrowser.Firefox];
    }

    public static IEnumerable<object[]> BrowserAndProxyKinds()
    {
        foreach (var browser in new[] { TestSeleniumBrowser.Chromium, TestSeleniumBrowser.Firefox })
        {
            yield return [browser, ProxyType.Http, false];

            // Firefox + SOCKS5 hangs before session creation in selenium/standalone-firefox.
            // Firefox authenticated proxies are not supported by the block yet.
            // Chrome authenticated SOCKS5 bypasses the proxy because Chrome auth hooks do not cover SOCKS auth.
            if (browser != TestSeleniumBrowser.Firefox)
            {
                yield return [browser, ProxyType.Http, true];
                yield return [browser, ProxyType.Socks5, false];
            }
        }
    }

    [Theory]
    [MemberData(nameof(BrowserKinds))]
    public async Task SeleniumBrowserBlocks_WithRemoteBrowser_CoverWindowNavigationTabsAndClose(TestSeleniumBrowser browserKind)
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        using var browser = await OpenBrowser(connection, browserKind, proxy: null);
        var data = NewBotData();
        data.SetObject("selenium", browser, disposeExisting: false);

        SeleniumBrowserMethods.SeleniumSetWindowSize(data, 1024, 768);
        SeleniumPageMethods.SeleniumNavigateTo(data, BuildDataUrl("<body data-page='one'>one</body>"), timeout: 20);
        SeleniumPageMethods.SeleniumNavigateTo(data, BuildDataUrl("<body data-page='two'>two</body>"), timeout: 20);

        SeleniumBrowserMethods.SeleniumGoBack(data);
        Assert.Equal("one", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.body.getAttribute('data-page');"));

        SeleniumBrowserMethods.SeleniumGoForward(data);
        Assert.Equal("two", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.body.getAttribute('data-page');"));

        SeleniumBrowserMethods.SeleniumReload(data);
        Assert.Equal("two", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.body.getAttribute('data-page');"));

        SeleniumBrowserMethods.SeleniumMinimize(data);
        SeleniumBrowserMethods.SeleniumMaximize(data);
        SeleniumBrowserMethods.SeleniumFullScreen(data);

        SeleniumBrowserMethods.SeleniumNewTab(data);
        Assert.Equal(2, browser.WindowHandles.Count);

        SeleniumBrowserMethods.SeleniumSwitchToTab(data, 0);
        Assert.Equal("two", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.body.getAttribute('data-page');"));

        SeleniumBrowserMethods.SeleniumSwitchToTab(data, 1);
        SeleniumBrowserMethods.SeleniumCloseTab(data);
        Assert.Single(browser.WindowHandles);

        browser.SwitchTo().Window(browser.WindowHandles[0]);
        SeleniumBrowserMethods.SeleniumCloseBrowser(data);
        Assert.Null(data.TryGetObject<WebDriver>("selenium"));
    }

    [Theory]
    [MemberData(nameof(BrowserKinds))]
    public async Task SeleniumPageBlocks_WithRemoteBrowser_CoverPageActionsCookiesAlertsAndScreenshots(TestSeleniumBrowser browserKind)
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        using var browser = await OpenBrowser(connection, browserKind, proxy: null);
        var data = NewBotData();
        data.SetObject("selenium", browser, disposeExisting: false);

        SeleniumPageMethods.SeleniumNavigateTo(data, BuildDataUrl(PageBlocksHtml()), timeout: 20);
        SeleniumElementMethods.SeleniumClick(data, FindElementBy.Id, "typing-target", 0);
        SeleniumPageMethods.SeleniumPageType(data, "ab");
        SeleniumPageMethods.SeleniumPageKeyDown(data, "Shift");
        SeleniumPageMethods.SeleniumPageType(data, "a");
        SeleniumPageMethods.SeleniumKeyUp(data, "Shift");
        SeleniumPageMethods.SeleniumPageKeyPress(data, "Enter");
        SeleniumPageMethods.SeleniumClickAtCoordinates(data, 150, 180);

        Assert.Contains("ab", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.getElementById('typing-target').value;"));
        Assert.Equal("main", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.body.getAttribute('data-coordinate-click');"));

        SeleniumPageMethods.SeleniumScrollBy(data, 0, 200);
        Assert.True(int.Parse(SeleniumPageMethods.SeleniumExecuteJs(data, "return String(window.scrollY);")) >= 0);
        Assert.Contains("typing-target", SeleniumPageMethods.SeleniumGetDOM(data));

        SeleniumPageMethods.SeleniumNavigateTo(data, connection.BuildTargetUrl("html"), timeout: 20);
        SeleniumPageMethods.SeleniumExecuteJs(data, "history.replaceState(null, '', '/html?dynamic=1');");
        Assert.EndsWith("/html?dynamic=1", SeleniumPageMethods.SeleniumGetCurrentUrl(data));
        Assert.Equal(SeleniumPageMethods.SeleniumGetCurrentUrl(data), data.ADDRESS);

        var screenshotPath = Path.Combine(Path.GetTempPath(), $"ob2-selenium-page-{Guid.NewGuid():N}.jpg");
        try
        {
            SeleniumPageMethods.SeleniumScreenshotPage(data, screenshotPath);
            Assert.True(new FileInfo(screenshotPath).Length > 0);
        }
        finally
        {
            if (File.Exists(screenshotPath))
            {
                File.Delete(screenshotPath);
            }
        }

        Assert.NotEmpty(SeleniumPageMethods.SeleniumScreenshotPageBase64(data));

        SeleniumPageMethods.SeleniumNavigateTo(data, connection.BuildTargetUrl("anything"), timeout: 20);
        SeleniumPageMethods.SeleniumSetCookies(data, connection.TargetIpAddress, new Dictionary<string, string>
        {
            ["ob2-cookie"] = "selenium"
        });

        Assert.Equal("selenium", SeleniumPageMethods.SeleniumGetCookies(data, connection.TargetIpAddress)["ob2-cookie"]);
        SeleniumPageMethods.SeleniumClearCookies(data);
        Assert.Empty(SeleniumPageMethods.SeleniumGetCookies(data, connection.TargetIpAddress));

        SeleniumPageMethods.SeleniumNavigateTo(data, BuildDataUrl(PageBlocksHtml()), timeout: 20);
        SeleniumElementMethods.SeleniumSwitchToFrame(data, FindElementBy.Id, "inner-frame", 0);
        SeleniumPageMethods.SeleniumClickAtCoordinates(data, 60, 50);
        Assert.Equal("inside", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.getElementById('inside').innerText;"));
        Assert.Equal("frame", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.getElementById('frame-button').getAttribute('data-coordinate-click');"));
        SeleniumPageMethods.SeleniumSwitchToMainFrame(data);
        Assert.Equal("page-block-test", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.body.getAttribute('data-page');"));

        ((IJavaScriptExecutor)browser).ExecuteScript("setTimeout(() => alert('hello'), 50);");
        await Task.Delay(200, TestContext.Current.CancellationToken);
        SeleniumPageMethods.SeleniumSwitchToAlert(data);
        browser.SwitchTo().Alert().Accept();
    }

    [Theory]
    [MemberData(nameof(BrowserKinds))]
    public async Task SeleniumElementBlocks_WithRemoteBrowser_CoverElementActionsAndScreenshots(TestSeleniumBrowser browserKind)
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        using var browser = await OpenBrowser(connection, browserKind, proxy: null);
        var data = NewBotData();
        data.SetObject("selenium", browser, disposeExisting: false);

        SeleniumPageMethods.SeleniumNavigateTo(data, BuildDataUrl(ElementBlocksHtml()), timeout: 20);
        await SeleniumElementMethods.SeleniumWaitForElement(data, FindElementBy.Id, "name", timeout: 5000);

        Assert.True(SeleniumElementMethods.SeleniumExists(data, FindElementBy.Id, "name", 0));
        Assert.False(SeleniumElementMethods.SeleniumExists(data, FindElementBy.Id, "missing", 0));
        Assert.True(SeleniumElementMethods.SeleniumIsDisplayed(data, FindElementBy.Id, "name", 0));
        Assert.True(SeleniumElementMethods.SeleniumIsEnabled(data, FindElementBy.Id, "name", 0));

        SeleniumElementMethods.SeleniumSetAttributeValue(data, FindElementBy.Id, "name", 0, "data-test", "updated");
        Assert.Equal("updated", SeleniumElementMethods.SeleniumGetAttributeValue(data, FindElementBy.Id, "name", 0, "data-test"));

        await SeleniumElementMethods.SeleniumTypeElement(data, FindElementBy.Id, "name", 0, "open");
        Assert.Equal("open", SeleniumElementMethods.SeleniumGetAttributeValue(data, FindElementBy.Id, "name", 0, "value"));

        SeleniumElementMethods.SeleniumClearField(data, FindElementBy.Id, "name", 0);
        await SeleniumElementMethods.SeleniumTypeElementHuman(data, FindElementBy.Id, "name", 0, "bullet");
        Assert.Equal("bullet", SeleniumElementMethods.SeleniumGetAttributeValue(data, FindElementBy.Id, "name", 0, "value"));

        SeleniumElementMethods.SeleniumClick(data, FindElementBy.Id, "copy", 0);
        Assert.Equal("bullet", SeleniumElementMethods.SeleniumGetAttributeValue(data, FindElementBy.Id, "result", 0));

        SeleniumElementMethods.SeleniumSubmit(data, FindElementBy.Id, "test-form", 0);
        Assert.Equal("submitted", SeleniumElementMethods.SeleniumGetAttributeValue(data, FindElementBy.Id, "submitted", 0));

        SeleniumElementMethods.SeleniumSelect(data, FindElementBy.Id, "choice", 0, "two");
        Assert.Equal("two", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.getElementById('choice').value;"));

        SeleniumElementMethods.SeleniumSelectByIndex(data, FindElementBy.Id, "choice", 0, 0);
        Assert.Equal("one", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.getElementById('choice').value;"));

        SeleniumElementMethods.SeleniumSelectByText(data, FindElementBy.Id, "choice", 0, "Two");
        Assert.Equal("two", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.getElementById('choice').value;"));

        Assert.Equal(new[] { "One", "Two" }, SeleniumElementMethods.SeleniumGetAttributeValueAll(data, FindElementBy.Selector, "#choice option"));
        Assert.True(SeleniumElementMethods.SeleniumGetWidth(data, FindElementBy.Id, "name", 0) > 0);
        Assert.True(SeleniumElementMethods.SeleniumGetHeight(data, FindElementBy.Id, "name", 0) > 0);
        Assert.True(SeleniumElementMethods.SeleniumGetPositionX(data, FindElementBy.Id, "name", 0) >= 0);
        Assert.True(SeleniumElementMethods.SeleniumGetPositionY(data, FindElementBy.Id, "name", 0) >= 0);

        var screenshotPath = Path.Combine(Path.GetTempPath(), $"ob2-selenium-element-{Guid.NewGuid():N}.jpg");
        try
        {
            SeleniumElementMethods.SeleniumScreenshotElement(data, FindElementBy.Id, "name", 0, screenshotPath);
            Assert.True(new FileInfo(screenshotPath).Length > 0);
        }
        finally
        {
            if (File.Exists(screenshotPath))
            {
                File.Delete(screenshotPath);
            }
        }

        Assert.NotEmpty(SeleniumElementMethods.SeleniumScreenshotBase64(data, FindElementBy.Id, "name", 0));

        SeleniumElementMethods.SeleniumSwitchToFrame(data, FindElementBy.Id, "inner-frame", 0);
        Assert.Equal("inside", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.getElementById('inside').innerText;"));
        SeleniumPageMethods.SeleniumSwitchToParent(data);
        Assert.Equal("element-block-test", SeleniumPageMethods.SeleniumExecuteJs(data, "return document.body.getAttribute('data-page');"));
    }

    [Theory]
    [MemberData(nameof(BrowserKinds))]
    public async Task SeleniumElementBlocks_WithRemoteBrowser_CoverSelectorKindsAndFailurePaths(TestSeleniumBrowser browserKind)
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        using var browser = await OpenBrowser(connection, browserKind, proxy: null);
        var data = NewBotData();
        data.SetObject("selenium", browser, disposeExisting: false);

        SeleniumPageMethods.SeleniumNavigateTo(data, BuildDataUrl(ElementBlocksHtml()), timeout: 20);

        Assert.Equal("selector-target", SeleniumElementMethods.SeleniumGetAttributeValue(data, FindElementBy.Class, "selector-class", 0, "id"));
        Assert.Equal("selector-target", SeleniumElementMethods.SeleniumGetAttributeValue(data, FindElementBy.Selector, "[data-selector='css']", 0, "id"));
        Assert.Equal("selector-target", SeleniumElementMethods.SeleniumGetAttributeValue(data, FindElementBy.XPath, "//*[@data-selector='css']", 0, "id"));

        var missing = Assert.Throws<BlockExecutionException>(() =>
            SeleniumElementMethods.SeleniumGetAttributeValue(data, FindElementBy.Id, "missing", 0));
        Assert.Equal("Expected at least 1 elements to be found but 0 were found", missing.Message);

        var outOfRange = Assert.Throws<BlockExecutionException>(() =>
            SeleniumElementMethods.SeleniumGetAttributeValue(data, FindElementBy.Class, "multi", 2));
        Assert.Equal("Expected at least 3 elements to be found but 2 were found", outOfRange.Message);

        await Assert.ThrowsAsync<TimeoutException>(() =>
            SeleniumElementMethods.SeleniumWaitForElement(data, FindElementBy.Id, "never-appears", timeout: 200));
    }

    [Theory]
    [MemberData(nameof(BrowserAndProxyKinds))]
    public async Task SeleniumNavigateTo_WithContainerProxy_RoutesBrowserRequestThroughProxy(
        TestSeleniumBrowser browserKind,
        ProxyType proxyType,
        bool authenticated)
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var proxy = authenticated
            ? connection.CreateAuthenticatedContainerProxy(proxyType)
            : connection.CreateContainerProxy(proxyType);

        var pool = $"proxy-{browserKind}-{proxyType}-{authenticated}";
        using var browser = await OpenBrowser(connection, browserKind, proxy, pool);
        var data = NewBotData();
        data.SetObject("selenium", browser, disposeExisting: false);

        var queryValue = $"selenium-{browserKind}-{proxyType}-{(authenticated ? "auth" : "noauth")}".ToLowerInvariant();
        SeleniumPageMethods.SeleniumNavigateTo(
            data,
            connection.BuildTargetUrl($"anything?proxy={queryValue}"),
            timeout: 20);

        var json = SeleniumPageMethods.SeleniumExecuteJs(data, "return document.body.innerText;");
        var response = DeserializeHttpBinResponse(json);
        var actualUri = new Uri(response.Url);

        Assert.Equal("GET", response.Method);
        Assert.Equal("/anything", actualUri.AbsolutePath);
        Assert.Equal($"?proxy={queryValue}", actualUri.Query);
        Assert.Contains(authenticated ? connection.AuthenticatedProxyIpAddress : connection.ProxyIpAddress, response.Origin);
    }

    private static async Task<RemoteWebDriver> OpenBrowser(
        ProxyServerConnectionInfo connection,
        TestSeleniumBrowser browserKind,
        RuriProxy? proxy,
        string pool = "default")
    {
        var serverUrl = await TestSeleniumServer.GetServerUrl(connection.Network, browserKind, pool);
        var options = CreateOptions(browserKind, proxy);
        return new RemoteWebDriver(serverUrl, options.ToCapabilities(), TimeSpan.FromSeconds(60));
    }

    private static DriverOptions CreateOptions(TestSeleniumBrowser browserKind, RuriProxy? proxy)
    {
        DriverOptions options = browserKind switch
        {
            TestSeleniumBrowser.Chromium => CreateChromeOptions(),
            TestSeleniumBrowser.Firefox => CreateFirefoxOptions(),
            _ => throw new NotSupportedException($"Unsupported Selenium browser {browserKind}")
        };

        if (proxy is not null)
        {
            if (options is FirefoxOptions firefoxOptions)
            {
                ApplyFirefoxProxy(firefoxOptions, proxy);
            }
            else if (options is ChromeOptions chromeOptions)
            {
                if (proxy.NeedsAuthentication)
                {
                    chromeOptions = CreateChromeOptions(headless: false);
                    options = chromeOptions;
                }

                chromeOptions.AddProxy(proxy);
            }
            else
            {
                options.Proxy = proxy.Type == ProxyType.Http
                    ? new OpenQA.Selenium.Proxy
                    {
                        HttpProxy = $"{proxy.Host}:{proxy.Port}",
                        SslProxy = $"{proxy.Host}:{proxy.Port}"
                    }
                    : new OpenQA.Selenium.Proxy
                    {
                        SocksProxy = $"{proxy.Host}:{proxy.Port}",
                        SocksVersion = proxy.Type == ProxyType.Socks4 ? 4 : 5,
                        SocksUserName = proxy.Username,
                        SocksPassword = proxy.Password
                    };
            }
        }

        return options;
    }

    private static void ApplyFirefoxProxy(FirefoxOptions options, RuriProxy proxy)
    {
        options.SetPreference("network.proxy.type", 1);
        options.SetPreference("network.proxy.no_proxies_on", "localhost,127.0.0.1");

        if (proxy.Type == ProxyType.Http)
        {
            options.SetPreference("network.proxy.http", proxy.Host);
            options.SetPreference("network.proxy.http_port", proxy.Port);
            options.SetPreference("network.proxy.ssl", proxy.Host);
            options.SetPreference("network.proxy.ssl_port", proxy.Port);
            return;
        }

        options.SetPreference("network.proxy.socks", proxy.Host);
        options.SetPreference("network.proxy.socks_port", proxy.Port);
        options.SetPreference("network.proxy.socks_remote_dns", true);
        options.SetPreference("network.proxy.socks_version", proxy.Type == ProxyType.Socks4 ? 4 : 5);
    }

    private static ChromeOptions CreateChromeOptions(bool headless = true)
    {
        var options = new ChromeOptions();

        if (headless)
        {
            options.AddArgument("--headless=new");
        }

        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--window-size=1024,768");
        return options;
    }

    private static FirefoxOptions CreateFirefoxOptions()
    {
        var options = new FirefoxOptions();
        options.AddArgument("-headless");
        options.AddArgument("--width=1024");
        options.AddArgument("--height=768");
        options.SetPreference("devtools.jsonview.enabled", false);
        return options;
    }

    private static BotData NewBotData()
        => new(
            new BotProviders(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("browser-test", new WordlistType()))
        {
            CancellationToken = TestContext.Current.CancellationToken
        };

    private static string BuildDataUrl(string html)
        => $"data:text/html;charset=utf-8,{Uri.EscapeDataString($"<!doctype html><html>{html}</html>")}";

    private static string PageBlocksHtml()
        => """
           <body data-page="page-block-test" style="height: 1800px; margin: 0; position: relative;">
             <input id="typing-target" value="">
             <button id="coordinate-target"
                     type="button"
                     style="position: absolute; left: 120px; top: 150px; width: 60px; height: 40px;"
                     onclick="document.body.setAttribute('data-coordinate-click', 'main')">Main</button>
             <iframe id="inner-frame"
                     style="position: absolute; left: 260px; top: 120px; width: 220px; height: 160px;"
                     srcdoc="<body style='margin:0; position:relative;'><button id='frame-button' type='button' style='position:absolute; left:20px; top:20px; width:80px; height:40px;' onclick=&quot;this.setAttribute('data-coordinate-click','frame')&quot;>Frame</button><div id='inside'>inside</div></body>"></iframe>
           </body>
           """;

    private static string ElementBlocksHtml()
        => """
           <body data-page="element-block-test">
             <form id="test-form" action="#" onsubmit="document.getElementById('submitted').innerText='submitted'; return false;">
               <input id="name" name="name" value="" style="width: 160px; height: 24px">
               <button id="copy" type="button" onclick="document.getElementById('result').innerText = document.getElementById('name').value">Copy</button>
               <select id="choice">
                 <option value="one">One</option>
                 <option value="two">Two</option>
               </select>
               <span id="result"></span>
               <span id="submitted"></span>
             </form>
             <div id="selector-target" class="selector-class" data-selector="css">selector</div>
             <div class="multi">first</div>
             <div class="multi">second</div>
             <iframe id="inner-frame" srcdoc="<div id='inside'>inside</div>"></iframe>
           </body>
           """;

    private static HttpBinResponse DeserializeHttpBinResponse(string source)
        => JsonConvert.DeserializeObject<HttpBinResponse>(source)
           ?? throw new InvalidOperationException("httpbin response could not be deserialized");
}

[CollectionDefinition(nameof(BrowserProxyServerCollection), DisableParallelization = true)]
public class BrowserProxyServerCollection;
