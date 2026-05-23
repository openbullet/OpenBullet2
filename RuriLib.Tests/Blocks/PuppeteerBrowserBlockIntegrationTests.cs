using Newtonsoft.Json;
using PuppeteerSharp;
using RuriLib.Exceptions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
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
using BrowserBrowserMethods = RuriLib.Blocks.Browser.Browser.Methods;
using BrowserElementMethods = RuriLib.Blocks.Browser.Elements.Methods;
using BrowserPageMethods = RuriLib.Blocks.Browser.Page.Methods;
using FindElementBy = RuriLib.Functions.Puppeteer.FindElementBy;
using PuppeteerBrowserMethods = RuriLib.Blocks.Puppeteer.Browser.Methods;
using PuppeteerElementMethods = RuriLib.Blocks.Puppeteer.Elements.Methods;
using PuppeteerPageMethods = RuriLib.Blocks.Puppeteer.Page.Methods;
using RuriProxy = RuriLib.Models.Proxies.Proxy;

namespace RuriLib.Tests.Blocks;

[Collection(nameof(BrowserProxyServerCollection))]
[Trait("Category", "BrowserIntegration")]
public class PuppeteerBrowserBlockIntegrationTests
{
    public static IEnumerable<object[]> ProxyKinds()
    {
        yield return [ProxyType.Http, false];
        yield return [ProxyType.Http, true];
        // With remote Chromium over CDP, plain --proxy-server works for SOCKS4 and SOCKS5 without auth.
        // The remaining combinations are intentionally excluded here:
        // - authenticated SOCKS4/SOCKS5 fail without the local Yove.Proxy bridge
        // - SOCKS4a is rejected by Chromium as an unsupported proxy scheme in this setup
        // - credentials embedded directly in the proxy URI are also rejected by Chromium
        yield return [ProxyType.Socks4, false];
        yield return [ProxyType.Socks5, false];
    }

    [Fact]
    public async Task PuppeteerBrowserBlocks_WithRemoteBrowser_CoverViewportNavigationTabsAndClose()
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var browser = await OpenBrowser(connection, proxy: null, pool: UniquePool());
        var data = NewBotData();
        await SetPuppeteerObjects(data, browser);

        await PuppeteerPageMethods.PuppeteerSetViewport(data, 1024, 768);
        await PuppeteerPageMethods.PuppeteerNavigateTo(data, BuildDataUrl("<body data-page='one'>one</body>"), timeout: 20000);
        await PuppeteerPageMethods.PuppeteerNavigateTo(data, BuildDataUrl("<body data-page='two'>two</body>"), timeout: 20000);

        await PuppeteerBrowserMethods.PuppeteerGoBack(data);
        Assert.Equal("one", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.body.getAttribute('data-page');"));

        await PuppeteerBrowserMethods.PuppeteerGoForward(data);
        Assert.Equal("two", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.body.getAttribute('data-page');"));

        await PuppeteerBrowserMethods.PuppeteerReload(data);
        Assert.Equal("two", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.body.getAttribute('data-page');"));

        var originalPage = data.TryGetObject<IPage>("puppeteerPage")!;
        await PuppeteerBrowserMethods.PuppeteerNewTab(data);
        var pages = await browser.PagesAsync();
        Assert.Equal(2, pages.Length);

        var originalPageIndex = Array.FindIndex(pages, p => p == originalPage);
        var newPageIndex = Array.FindIndex(pages, p => p != originalPage);

        await PuppeteerBrowserMethods.PuppeteerSwitchToTab(data, originalPageIndex);
        Assert.Equal("two", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.body.getAttribute('data-page');"));

        await PuppeteerBrowserMethods.PuppeteerSwitchToTab(data, newPageIndex);
        await PuppeteerBrowserMethods.PuppeteerCloseTab(data);
        Assert.Single(await browser.PagesAsync());

        await PuppeteerBrowserMethods.PuppeteerCloseBrowser(data);
        Assert.Null(data.TryGetObject<IBrowser>("puppeteer"));
    }

    [Fact]
    public async Task BrowserBlocks_WithPuppeteerGhostCursor_CoverHelperMoveMouseButtonsAndScroll()
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var browser = await OpenBrowser(connection, proxy: null, pool: UniquePool());
        try
        {
            var data = NewBotData();
            data.ConfigSettings.BrowserSettings.MouseAutomationMode = BrowserMouseAutomationMode.GhostCursor;
            await SetPuppeteerObjects(data, browser);

            await BrowserPageMethods.BrowserNavigateTo(data, BuildDataUrl(GhostCursorHtml()), timeout: 20000);
            await BrowserPageMethods.BrowserInjectMousePositionHelper(data);
            await BrowserElementMethods.BrowserMoveCursorToElement(data, FindElementBy.Id, "target", 0);
            Assert.Equal("true", await BrowserPageMethods.BrowserExecuteJs(data, "String(document.querySelector('p-mouse-pointer') !== null);"));
            await BrowserPageMethods.BrowserMouseDown(data);
            await BrowserPageMethods.BrowserMouseUp(data);

            Assert.Equal("yes", await BrowserPageMethods.BrowserExecuteJs(data, "document.body.getAttribute('data-down');"));
            Assert.Equal("yes", await BrowserPageMethods.BrowserExecuteJs(data, "document.body.getAttribute('data-up');"));

            await BrowserBrowserMethods.BrowserToggleRandomMouseMoves(data, true);
            await Task.Delay(50, TestContext.Current.CancellationToken);
            await BrowserBrowserMethods.BrowserToggleRandomMouseMoves(data, false);

            await BrowserPageMethods.BrowserScrollBy(data, 0, 200);
            Assert.True(int.Parse(await BrowserPageMethods.BrowserExecuteJs(data, "String(window.scrollY);")) > 0);
        }
        finally
        {
            browser.Disconnect();
        }
    }

    [Fact]
    public async Task PuppeteerPageBlocks_WithRemoteBrowser_CoverPageActionsCookiesFramesResponsesAndScreenshots()
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var browser = await OpenBrowser(connection, proxy: null, pool: UniquePool());
        try
        {
            var data = NewBotData();
            await SetPuppeteerObjects(data, browser);

            await PuppeteerPageMethods.PuppeteerSetViewport(data, 1024, 768);
            await PuppeteerPageMethods.PuppeteerNavigateTo(data, BuildDataUrl(PageBlocksHtml()), timeout: 20000);
            await PuppeteerElementMethods.PuppeteerClick(data, FindElementBy.Id, "typing-target", 0);
            await PuppeteerPageMethods.PuppeteerPageType(data, "ab");
            await PuppeteerPageMethods.PuppeteerPageKeyDown(data, "Shift");
            await PuppeteerPageMethods.PuppeteerPageType(data, "a");
            await PuppeteerPageMethods.PuppeteerKeyUp(data, "Shift");
            await PuppeteerPageMethods.PuppeteerPageKeyPress(data, "Enter");
            await PuppeteerPageMethods.PuppeteerClickAtCoordinates(data, 150, 180);

            Assert.Contains("ab", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('typing-target').value;"));
            Assert.Equal("main", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.body.getAttribute('data-coordinate-click');"));

            await PuppeteerPageMethods.PuppeteerScrollBy(data, 0, 200);
            Assert.True(int.Parse(await PuppeteerPageMethods.PuppeteerExecuteJs(data, "String(window.scrollY);")) >= 0);
            await PuppeteerPageMethods.PuppeteerScrollToBottom(data);
            Assert.True(int.Parse(await PuppeteerPageMethods.PuppeteerExecuteJs(data, "String(window.scrollY);")) > 0);
            await PuppeteerPageMethods.PuppeteerScrollToTop(data);
            Assert.Equal("0", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "String(window.scrollY);"));
            Assert.Contains("typing-target", await PuppeteerPageMethods.PuppeteerGetDOM(data));

            await PuppeteerPageMethods.PuppeteerNavigateTo(data, connection.BuildTargetUrl("html"), timeout: 20000);
            await PuppeteerPageMethods.PuppeteerExecuteJs(data, "history.replaceState(null, '', '/html?dynamic=1');");
            Assert.EndsWith("/html", data.ADDRESS);
            Assert.EndsWith("/html?dynamic=1", PuppeteerPageMethods.PuppeteerGetCurrentUrl(data));
            Assert.Equal(PuppeteerPageMethods.PuppeteerGetCurrentUrl(data), data.ADDRESS);

            var screenshotPath = Path.Combine(Path.GetTempPath(), $"ob2-puppeteer-page-{Guid.NewGuid():N}.jpg");
            try
            {
                await PuppeteerPageMethods.PuppeteerScreenshotPage(data, screenshotPath);
                Assert.True(new FileInfo(screenshotPath).Length > 0);
            }
            finally
            {
                if (File.Exists(screenshotPath))
                {
                    File.Delete(screenshotPath);
                }
            }

            Assert.NotEmpty(await PuppeteerPageMethods.PuppeteerScreenshotPageBase64(data));

            await PuppeteerPageMethods.PuppeteerSetUserAgent(data, "OpenBullet2-Puppeteer-Test");
            Assert.Equal("OpenBullet2-Puppeteer-Test", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "navigator.userAgent;"));

            await PuppeteerPageMethods.PuppeteerNavigateTo(data, connection.BuildTargetUrl("anything"), timeout: 20000);
            await PuppeteerPageMethods.PuppeteerSetCookies(data, connection.TargetIpAddress, new Dictionary<string, string>
            {
                ["ob2-cookie"] = "puppeteer"
            });

            Assert.Equal("puppeteer", (await PuppeteerPageMethods.PuppeteerGetCookies(data, connection.TargetIpAddress))["ob2-cookie"]);
            await PuppeteerPageMethods.PuppeteerClearCookies(data, connection.BuildTargetUrl("/"));
            Assert.Empty(await PuppeteerPageMethods.PuppeteerGetCookies(data, connection.TargetIpAddress));

            var waitUrl = connection.BuildTargetUrl("anything?wait=response");
            var page = data.TryGetObject<IPage>("puppeteerPage")!;
            var waitForResponse = PuppeteerPageMethods.PuppeteerWaitForResponse(data, waitUrl, 5000);
            await page.EvaluateExpressionAsync($"fetch('{waitUrl}')");
            await waitForResponse;
            Assert.Equal(200, data.RESPONSECODE);
            Assert.Contains("wait=response", data.ADDRESS);
            Assert.Contains("\"method\": \"GET\"", data.SOURCE);

            var noBodyUrl = connection.BuildTargetUrl("status/204?wait=no-body");
            var waitForNoBodyResponse = PuppeteerPageMethods.PuppeteerWaitForResponse(data, noBodyUrl, 5000);
            await page.EvaluateExpressionAsync($"fetch('{noBodyUrl}')");
            await waitForNoBodyResponse;
            Assert.Equal(204, data.RESPONSECODE);
            Assert.Contains("status/204", data.ADDRESS);
            Assert.Equal(string.Empty, data.SOURCE);
            Assert.Empty(data.RAWSOURCE);

            await PuppeteerPageMethods.PuppeteerNavigateTo(data, BuildDataUrl(PageBlocksHtml()), timeout: 20000);
            await PuppeteerElementMethods.PuppeteerSwitchToFrame(data, FindElementBy.Id, "inner-frame", 0);
            await PuppeteerElementMethods.PuppeteerWaitForElement(data, FindElementBy.Id, "inside", timeout: 5000);
            await PuppeteerPageMethods.PuppeteerClickAtCoordinates(data, 60, 50);
            Assert.Equal("inside", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('inside').innerText;"));
            Assert.Equal("frame", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('frame-button').getAttribute('data-coordinate-click');"));
            Assert.Equal("true", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "String(document.getElementById('g-recaptcha-response') === null);"));
            PuppeteerPageMethods.PuppeteerSwitchToMainFrame(data);
            await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('g-recaptcha-response').value = 'solved';");
            Assert.Equal("solved", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('g-recaptcha-response').value;"));
            Assert.Equal("page-block-test", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.body.getAttribute('data-page');"));
        }
        finally
        {
            browser.Disconnect();
        }
    }

    [Fact]
    public async Task PuppeteerElementBlocks_WithRemoteBrowser_CoverElementActionsUploadAndScreenshots()
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var browser = await OpenBrowser(connection, proxy: null, pool: UniquePool());
        var uploadPath = Path.Combine(Path.GetTempPath(), $"ob2-puppeteer-upload-{Guid.NewGuid():N}.txt");

        try
        {
            await File.WriteAllTextAsync(uploadPath, "openbullet", TestContext.Current.CancellationToken);

            var data = NewBotData();
            await SetPuppeteerObjects(data, browser);

            await PuppeteerPageMethods.PuppeteerNavigateTo(data, BuildDataUrl(ElementBlocksHtml()), timeout: 20000);
            await PuppeteerElementMethods.PuppeteerWaitForElement(data, FindElementBy.Id, "name", timeout: 5000);

            Assert.True(await PuppeteerElementMethods.PuppeteerExists(data, FindElementBy.Id, "name", 0));
            Assert.False(await PuppeteerElementMethods.PuppeteerExists(data, FindElementBy.Id, "missing", 0));
            Assert.True(await PuppeteerElementMethods.PuppeteerIsDisplayed(data, FindElementBy.Id, "name", 0));

            await PuppeteerElementMethods.PuppeteerSetAttributeValue(data, FindElementBy.Id, "name", 0, "data-test", "updated");
            Assert.Equal("updated", await PuppeteerElementMethods.PuppeteerGetAttributeValue(data, FindElementBy.Id, "name", 0, "dataset.test"));

            await PuppeteerElementMethods.PuppeteerTypeElement(data, FindElementBy.Id, "name", 0, "open");
            Assert.Equal("open", await PuppeteerElementMethods.PuppeteerGetAttributeValue(data, FindElementBy.Id, "name", 0, "value"));

            await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('name').value = '';");
            await PuppeteerElementMethods.PuppeteerTypeElementHuman(data, FindElementBy.Id, "name", 0, "bullet");
            Assert.Equal("bullet", await PuppeteerElementMethods.PuppeteerGetAttributeValue(data, FindElementBy.Id, "name", 0, "value"));

            await PuppeteerElementMethods.PuppeteerClick(data, FindElementBy.Id, "copy", 0);
            Assert.Equal("bullet", await PuppeteerElementMethods.PuppeteerGetAttributeValue(data, FindElementBy.Id, "result", 0));

            await PuppeteerElementMethods.PuppeteerSubmit(data, FindElementBy.Id, "test-form", 0);
            Assert.Equal("submitted", await PuppeteerElementMethods.PuppeteerGetAttributeValue(data, FindElementBy.Id, "submitted", 0));

            await PuppeteerElementMethods.PuppeteerSelect(data, FindElementBy.Id, "choice", 0, "two");
            Assert.Equal("two", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('choice').value;"));

            await PuppeteerElementMethods.PuppeteerSelectByIndex(data, FindElementBy.Id, "choice", 0, 0);
            Assert.Equal("one", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('choice').value;"));

            await PuppeteerElementMethods.PuppeteerSelectByText(data, FindElementBy.Id, "choice", 0, "Two");
            Assert.Equal("two", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('choice').value;"));

            Assert.Equal(new[] { "One", "Two" }, await PuppeteerElementMethods.PuppeteerGetAttributeValueAll(data, FindElementBy.Selector, "#choice option"));
            Assert.True(await PuppeteerElementMethods.PuppeteerGetWidth(data, FindElementBy.Id, "name", 0) > 0);
            Assert.True(await PuppeteerElementMethods.PuppeteerGetHeight(data, FindElementBy.Id, "name", 0) > 0);
            Assert.True(await PuppeteerElementMethods.PuppeteerGetPositionX(data, FindElementBy.Id, "name", 0) >= 0);
            Assert.True(await PuppeteerElementMethods.PuppeteerGetPositionY(data, FindElementBy.Id, "name", 0) >= 0);

            var screenshotPath = Path.Combine(Path.GetTempPath(), $"ob2-puppeteer-element-{Guid.NewGuid():N}.jpg");
            try
            {
                await PuppeteerElementMethods.PuppeteerScreenshotElement(data, FindElementBy.Id, "selector-target", 0, screenshotPath);
                Assert.True(new FileInfo(screenshotPath).Length > 0);
            }
            finally
            {
                if (File.Exists(screenshotPath))
                {
                    File.Delete(screenshotPath);
                }
            }

            Assert.NotEmpty(await PuppeteerElementMethods.PuppeteerScreenshotBase64(data, FindElementBy.Id, "selector-target", 0));

            await PuppeteerElementMethods.PuppeteerSwitchToFrame(data, FindElementBy.Id, "inner-frame", 0);
            await PuppeteerElementMethods.PuppeteerWaitForElement(data, FindElementBy.Id, "inside", timeout: 5000);
            Assert.Equal("inside", await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('inside').innerText;"));
            PuppeteerPageMethods.PuppeteerSwitchToMainFrame(data);

            await PuppeteerElementMethods.PuppeteerUploadFiles(data, FindElementBy.Id, "upload", 0, [uploadPath]);
            Assert.EndsWith(Path.GetFileName(uploadPath), await PuppeteerPageMethods.PuppeteerExecuteJs(data, "document.getElementById('upload').value;"));
        }
        finally
        {
            if (File.Exists(uploadPath))
            {
                File.Delete(uploadPath);
            }

            browser.Disconnect();
        }
    }

    [Fact]
    public async Task PuppeteerElementBlocks_WithRemoteBrowser_CoverSelectorKindsAndFailurePaths()
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var browser = await OpenBrowser(connection, proxy: null, pool: UniquePool());
        try
        {
            var data = NewBotData();
            await SetPuppeteerObjects(data, browser);

            await PuppeteerPageMethods.PuppeteerNavigateTo(data, BuildDataUrl(ElementBlocksHtml()), timeout: 20000);

            Assert.Equal("selector-target", await PuppeteerElementMethods.PuppeteerGetAttributeValue(data, FindElementBy.Class, "selector-class", 0, "id"));
            Assert.Equal("selector-target", await PuppeteerElementMethods.PuppeteerGetAttributeValue(data, FindElementBy.Selector, "[data-selector='css']", 0, "id"));
            Assert.Equal("selector-target", await PuppeteerElementMethods.PuppeteerGetAttributeValue(data, FindElementBy.XPath, "//*[@data-selector='css']", 0, "id"));

            var missing = await Assert.ThrowsAsync<BlockExecutionException>(() =>
                PuppeteerElementMethods.PuppeteerGetAttributeValue(data, FindElementBy.Id, "missing", 0));
            Assert.Equal("Expected at least 1 elements to be found but 0 were found", missing.Message);

            var outOfRange = await Assert.ThrowsAsync<BlockExecutionException>(() =>
                PuppeteerElementMethods.PuppeteerGetAttributeValue(data, FindElementBy.Class, "multi", 2));
            Assert.Equal("Expected at least 3 elements to be found but 2 were found", outOfRange.Message);

            await Assert.ThrowsAsync<WaitTaskTimeoutException>(() =>
                PuppeteerElementMethods.PuppeteerWaitForElement(data, FindElementBy.Id, "never-appears", timeout: 200));
        }
        finally
        {
            browser.Disconnect();
        }
    }

    [Theory]
    [MemberData(nameof(ProxyKinds))]
    public async Task PuppeteerNavigateTo_WithContainerProxy_RoutesBrowserRequestThroughProxy(
        ProxyType proxyType,
        bool authenticated)
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var proxy = authenticated
            ? connection.CreateAuthenticatedContainerProxy(proxyType)
            : connection.CreateContainerProxy(proxyType);

        var pool = $"proxy-puppeteer-{proxyType}-{authenticated}";
        var browser = await OpenBrowser(connection, proxy, pool);
        try
        {
            var data = NewBotData();
            await SetPuppeteerObjects(data, browser);

            if (authenticated)
            {
                var page = data.TryGetObject<IPage>("puppeteerPage")!;
                await page.AuthenticateAsync(new Credentials
                {
                    Username = proxy.Username,
                    Password = proxy.Password
                });
            }

            var queryValue = $"puppeteer-{proxyType}-{(authenticated ? "auth" : "noauth")}".ToLowerInvariant();
            await PuppeteerPageMethods.PuppeteerNavigateTo(
                data,
                connection.BuildTargetUrl($"anything?proxy={queryValue}"),
                timeout: 20000);

            var response = DeserializeHttpBinResponse(data.SOURCE);
            var actualUri = new Uri(response.Url);

            Assert.Equal("GET", response.Method);
            Assert.Equal("/anything", actualUri.AbsolutePath);
            Assert.Equal($"?proxy={queryValue}", actualUri.Query);
            Assert.Contains(authenticated ? connection.AuthenticatedProxyIpAddress : connection.ProxyIpAddress, response.Origin);
        }
        finally
        {
            browser.Disconnect();
        }
    }

    private static async Task<IBrowser> OpenBrowser(
        ProxyServerConnectionInfo connection,
        RuriProxy? proxy,
        string pool)
    {
        var browserUrl = proxy is null
            ? await TestPuppeteerServer.GetBrowserUrl(connection.Network, pool)
            : await TestPuppeteerServer.GetBrowserUrl(
                connection.Network,
                pool,
                $"--proxy-server={proxy.Type.ToString().ToLower()}://{proxy.Host}:{proxy.Port}");

        return await Puppeteer.ConnectAsync(new ConnectOptions
        {
            BrowserURL = browserUrl.ToString(),
            DefaultViewport = null
        });
    }

    private static async Task SetPuppeteerObjects(BotData data, IBrowser browser)
    {
        data.SetObject("puppeteer", browser, disposeExisting: false);
        var page = (await browser.PagesAsync()).FirstOrDefault() ?? await browser.NewPageAsync();
        data.SetObject("puppeteerPage", page, disposeExisting: false);
        data.SetObject("puppeteerFrame", page.MainFrame, disposeExisting: false);
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

    private static string UniquePool()
        => $"puppeteer-{Guid.NewGuid():N}";

    private static string BuildDataUrl(string html)
        => $"data:text/html;charset=utf-8,{Uri.EscapeDataString($"<!doctype html><html>{html}</html>")}";

    private static string PageBlocksHtml()
        => """
           <body data-page="page-block-test" style="height: 1800px; margin: 0; position: relative;">
             <input id="typing-target" value="">
             <textarea id="g-recaptcha-response" style="display:none;"></textarea>
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
               <input id="upload" type="file">
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

    private static string GhostCursorHtml()
        => """
           <body style="height: 1600px; margin: 0;">
             <button id="target"
                     type="button"
                     style="position:absolute; left:120px; top:140px; width:80px; height:40px;"
                     onmousedown="document.body.setAttribute('data-down', 'yes')"
                     onmouseup="document.body.setAttribute('data-up', 'yes')">
               Target
             </button>
           </body>
           """;

    private static HttpBinResponse DeserializeHttpBinResponse(string source)
        => JsonConvert.DeserializeObject<HttpBinResponse>(source)
           ?? throw new InvalidOperationException("httpbin response could not be deserialized");
}
