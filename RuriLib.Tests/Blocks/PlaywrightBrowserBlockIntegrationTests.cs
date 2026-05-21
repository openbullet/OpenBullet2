using Microsoft.Playwright;
using Newtonsoft.Json;
using RuriLib.Exceptions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
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

namespace RuriLib.Tests.Blocks;

[Collection(nameof(BrowserProxyServerCollection))]
[Trait("Category", "BrowserIntegration")]
public class PlaywrightBrowserBlockIntegrationTests
{
    [Fact]
    public async Task BrowserBlocks_WithPlaywrightRemoteBrowser_CoverViewportNavigationTabsAndClose()
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var (playwright, browser) = await OpenBrowser(connection, UniquePool());
        try
        {
            var data = NewBotData();
            await SetPlaywrightObjects(data, playwright, browser);

            await BrowserPageMethods.BrowserSetViewport(data, 1024, 768);
            await BrowserPageMethods.BrowserNavigateTo(data, BuildDataUrl("<body data-page='one'>one</body>"), timeout: 20000);
            await BrowserPageMethods.BrowserNavigateTo(data, BuildDataUrl("<body data-page='two'>two</body>"), timeout: 20000);

            await BrowserBrowserMethods.BrowserGoBack(data);
            Assert.Equal("one", await BrowserPageMethods.BrowserExecuteJs(data, "document.body.getAttribute('data-page');"));

            await BrowserBrowserMethods.BrowserGoForward(data);
            Assert.Equal("two", await BrowserPageMethods.BrowserExecuteJs(data, "document.body.getAttribute('data-page');"));

            await BrowserBrowserMethods.BrowserReload(data);
            Assert.Equal("two", await BrowserPageMethods.BrowserExecuteJs(data, "document.body.getAttribute('data-page');"));

            var originalPage = data.TryGetObject<IPage>("playwrightPage")!;
            var context = data.TryGetObject<IBrowserContext>("playwrightContext")!;

            await BrowserBrowserMethods.BrowserNewTab(data);
            Assert.Equal(2, context.Pages.Count);

            var pages = context.Pages.ToList();
            var originalPageIndex = pages.FindIndex(p => p == originalPage);
            var newPageIndex = pages.FindIndex(p => p != originalPage);

            await BrowserBrowserMethods.BrowserSwitchToTab(data, originalPageIndex);
            Assert.Equal("two", await BrowserPageMethods.BrowserExecuteJs(data, "document.body.getAttribute('data-page');"));

            await BrowserBrowserMethods.BrowserSwitchToTab(data, newPageIndex);
            await BrowserBrowserMethods.BrowserCloseTab(data);
            Assert.Single(context.Pages);

            await BrowserBrowserMethods.BrowserClose(data);
            Assert.Null(data.TryGetObject<IBrowser>("playwrightBrowser"));
            Assert.Null(data.TryGetObject<IPage>("playwrightPage"));
        }
        finally
        {
            if (browser.IsConnected)
            {
                await browser.CloseAsync();
            }

            playwright.Dispose();
        }
    }

    [Fact]
    public async Task BrowserPageBlocks_WithPlaywrightRemoteBrowser_CoverPageActionsCookiesFramesResponsesAndScreenshots()
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var (playwright, browser) = await OpenBrowser(connection, UniquePool());

        try
        {
            var data = NewBotData();
            await SetPlaywrightObjects(data, playwright, browser);

            await BrowserPageMethods.BrowserSetViewport(data, 1024, 768);
            await BrowserPageMethods.BrowserNavigateTo(data, BuildDataUrl(PageBlocksHtml()), timeout: 20000);
            await BrowserElementMethods.BrowserClick(data, FindElementBy.Id, "typing-target", 0);
            await BrowserPageMethods.BrowserPageType(data, "ab");
            await BrowserPageMethods.BrowserPageKeyDown(data, "Shift");
            await BrowserPageMethods.BrowserPageType(data, "a");
            await BrowserPageMethods.BrowserKeyUp(data, "Shift");
            await BrowserPageMethods.BrowserPageKeyPress(data, "Enter");
            await BrowserPageMethods.BrowserClickAtCoordinates(data, 150, 180);

            Assert.Contains("ab", await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('typing-target').value;"));
            Assert.Equal("main", await BrowserPageMethods.BrowserExecuteJs(data, "document.body.getAttribute('data-coordinate-click');"));

            await BrowserPageMethods.BrowserScrollBy(data, 0, 200);
            Assert.True(int.Parse(await BrowserPageMethods.BrowserExecuteJs(data, "String(window.scrollY);")) >= 0);
            await BrowserPageMethods.BrowserScrollToBottom(data);
            Assert.True(int.Parse(await BrowserPageMethods.BrowserExecuteJs(data, "String(window.scrollY);")) > 0);
            await BrowserPageMethods.BrowserScrollToTop(data);
            Assert.Equal("0", await BrowserPageMethods.BrowserExecuteJs(data, "String(window.scrollY);"));
            Assert.Contains("typing-target", await BrowserPageMethods.BrowserGetDOM(data));

            await BrowserPageMethods.BrowserNavigateTo(data, connection.BuildTargetUrl("html"), timeout: 20000);
            await BrowserPageMethods.BrowserExecuteJs(data, "history.replaceState(null, '', '/html?dynamic=1');");
            Assert.EndsWith("/html", data.ADDRESS);
            Assert.EndsWith("/html?dynamic=1", BrowserPageMethods.BrowserGetCurrentUrl(data));
            Assert.Equal(BrowserPageMethods.BrowserGetCurrentUrl(data), data.ADDRESS);

            var screenshotPath = Path.Combine(Path.GetTempPath(), $"ob2-playwright-page-{Guid.NewGuid():N}.jpg");

            try
            {
                await BrowserPageMethods.BrowserScreenshotPage(data, screenshotPath);
                Assert.True(new FileInfo(screenshotPath).Length > 0);
            }
            finally
            {
                if (File.Exists(screenshotPath))
                {
                    File.Delete(screenshotPath);
                }
            }

            Assert.NotEmpty(await BrowserPageMethods.BrowserScreenshotPageBase64(data));

            await BrowserPageMethods.BrowserSetUserAgent(data, "OpenBullet2-Playwright-Test");
            Assert.Equal("OpenBullet2-Playwright-Test", await BrowserPageMethods.BrowserExecuteJs(data, "navigator.userAgent;"));

            await BrowserPageMethods.BrowserNavigateTo(data, connection.BuildTargetUrl("anything"), timeout: 20000);
            await BrowserPageMethods.BrowserSetCookies(data, connection.TargetIpAddress, new Dictionary<string, string>
            {
                ["ob2-cookie"] = "playwright"
            });

            Assert.Equal("playwright", (await BrowserPageMethods.BrowserGetCookies(data, connection.TargetIpAddress))["ob2-cookie"]);
            await BrowserPageMethods.BrowserClearCookies(data, connection.BuildTargetUrl("/"));
            Assert.Empty(await BrowserPageMethods.BrowserGetCookies(data, connection.TargetIpAddress));

            var waitUrl = connection.BuildTargetUrl("anything?wait=response");
            var page = data.TryGetObject<IPage>("playwrightPage")!;
            var waitForResponse = BrowserPageMethods.BrowserWaitForResponse(data, waitUrl, 5000);
            _ = page.EvaluateAsync($"fetch('{waitUrl}')");
            await waitForResponse;
            Assert.Equal(200, data.RESPONSECODE);
            Assert.Contains("wait=response", data.ADDRESS);
            Assert.Contains("\"method\": \"GET\"", data.SOURCE);

            var noBodyUrl = connection.BuildTargetUrl("status/204?wait=no-body");
            var waitForNoBodyResponse = BrowserPageMethods.BrowserWaitForResponse(data, noBodyUrl, 5000);
            _ = page.EvaluateAsync($"fetch('{noBodyUrl}')");
            await waitForNoBodyResponse;
            Assert.Equal(204, data.RESPONSECODE);
            Assert.Contains("status/204", data.ADDRESS);
            Assert.Equal(string.Empty, data.SOURCE);
            Assert.Empty(data.RAWSOURCE);

            await BrowserPageMethods.BrowserNavigateTo(data, BuildDataUrl(PageBlocksHtml()), timeout: 20000);
            await BrowserElementMethods.BrowserSwitchToFrame(data, FindElementBy.Id, "inner-frame", 0);
            await BrowserElementMethods.BrowserWaitForElement(data, FindElementBy.Id, "inside", timeout: 5000);
            await BrowserPageMethods.BrowserClickAtCoordinates(data, 60, 50);
            Assert.Equal("inside", await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('inside').innerText;"));
            Assert.Equal("frame", await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('frame-button').getAttribute('data-coordinate-click');"));
            Assert.Equal("true", await BrowserPageMethods.BrowserExecuteJs(data, "String(document.getElementById('g-recaptcha-response') === null);"));
            BrowserPageMethods.BrowserSwitchToMainFrame(data);
            await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('g-recaptcha-response').value = 'solved';");
            Assert.Equal("solved", await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('g-recaptcha-response').value;"));
            Assert.Equal("page-block-test", await BrowserPageMethods.BrowserExecuteJs(data, "document.body.getAttribute('data-page');"));
        }
        finally
        {
            await browser.CloseAsync();
            playwright.Dispose();
        }
    }

    [Fact]
    public async Task BrowserElementBlocks_WithPlaywrightRemoteBrowser_CoverElementActionsUploadAndScreenshots()
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var (playwright, browser) = await OpenBrowser(connection, UniquePool());
        var uploadPath = Path.Combine(Path.GetTempPath(), $"ob2-playwright-upload-{Guid.NewGuid():N}.txt");

        try
        {
            await File.WriteAllTextAsync(uploadPath, "openbullet", TestContext.Current.CancellationToken);

            var data = NewBotData();
            await SetPlaywrightObjects(data, playwright, browser);

            await BrowserPageMethods.BrowserNavigateTo(data, BuildDataUrl(ElementBlocksHtml()), timeout: 20000);
            await BrowserElementMethods.BrowserWaitForElement(data, FindElementBy.Id, "name", timeout: 5000);

            Assert.True(await BrowserElementMethods.BrowserExists(data, FindElementBy.Id, "name", 0));
            Assert.False(await BrowserElementMethods.BrowserExists(data, FindElementBy.Id, "missing", 0));
            Assert.True(await BrowserElementMethods.BrowserIsDisplayed(data, FindElementBy.Id, "name", 0));

            await BrowserElementMethods.BrowserSetAttributeValue(data, FindElementBy.Id, "name", 0, "data-test", "updated");
            Assert.Equal("updated", await BrowserElementMethods.BrowserGetAttributeValue(data, FindElementBy.Id, "name", 0, "dataset.test"));

            await BrowserElementMethods.BrowserTypeElement(data, FindElementBy.Id, "name", 0, "open");
            Assert.Equal("open", await BrowserElementMethods.BrowserGetAttributeValue(data, FindElementBy.Id, "name", 0, "value"));

            await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('name').value = '';");
            await BrowserElementMethods.BrowserTypeElementHuman(data, FindElementBy.Id, "name", 0, "bullet");
            Assert.Equal("bullet", await BrowserElementMethods.BrowserGetAttributeValue(data, FindElementBy.Id, "name", 0, "value"));

            await BrowserElementMethods.BrowserClick(data, FindElementBy.Id, "copy", 0);
            Assert.Equal("bullet", await BrowserElementMethods.BrowserGetAttributeValue(data, FindElementBy.Id, "result", 0));

            await BrowserElementMethods.BrowserSubmit(data, FindElementBy.Id, "test-form", 0);
            Assert.Equal("submitted", await BrowserElementMethods.BrowserGetAttributeValue(data, FindElementBy.Id, "submitted", 0));

            await BrowserElementMethods.BrowserSelect(data, FindElementBy.Id, "choice", 0, "two");
            Assert.Equal("two", await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('choice').value;"));

            await BrowserElementMethods.BrowserSelectByIndex(data, FindElementBy.Id, "choice", 0, 0);
            Assert.Equal("one", await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('choice').value;"));

            await BrowserElementMethods.BrowserSelectByText(data, FindElementBy.Id, "choice", 0, "Two");
            Assert.Equal("two", await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('choice').value;"));

            Assert.Equal(new[] { "One", "Two" }, await BrowserElementMethods.BrowserGetAttributeValueAll(data, FindElementBy.Selector, "#choice option"));
            Assert.True(await BrowserElementMethods.BrowserGetWidth(data, FindElementBy.Id, "name", 0) > 0);
            Assert.True(await BrowserElementMethods.BrowserGetHeight(data, FindElementBy.Id, "name", 0) > 0);
            Assert.True(await BrowserElementMethods.BrowserGetPositionX(data, FindElementBy.Id, "name", 0) >= 0);
            Assert.True(await BrowserElementMethods.BrowserGetPositionY(data, FindElementBy.Id, "name", 0) >= 0);

            var screenshotPath = Path.Combine(Path.GetTempPath(), $"ob2-playwright-element-{Guid.NewGuid():N}.jpg");

            try
            {
                await BrowserElementMethods.BrowserScreenshotElement(data, FindElementBy.Id, "selector-target", 0, screenshotPath);
                Assert.True(new FileInfo(screenshotPath).Length > 0);
            }
            finally
            {
                if (File.Exists(screenshotPath))
                {
                    File.Delete(screenshotPath);
                }
            }

            Assert.NotEmpty(await BrowserElementMethods.BrowserScreenshotBase64(data, FindElementBy.Id, "selector-target", 0));

            await BrowserElementMethods.BrowserSwitchToFrame(data, FindElementBy.Id, "inner-frame", 0);
            await BrowserElementMethods.BrowserWaitForElement(data, FindElementBy.Id, "inside", timeout: 5000);
            Assert.Equal("inside", await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('inside').innerText;"));
            BrowserPageMethods.BrowserSwitchToMainFrame(data);

            await BrowserElementMethods.BrowserUploadFiles(data, FindElementBy.Id, "upload", 0, [uploadPath]);
            Assert.EndsWith(Path.GetFileName(uploadPath), await BrowserPageMethods.BrowserExecuteJs(data, "document.getElementById('upload').value;"));
        }
        finally
        {
            if (File.Exists(uploadPath))
            {
                File.Delete(uploadPath);
            }

            await browser.CloseAsync();
            playwright.Dispose();
        }
    }

    private static async Task<(IPlaywright Playwright, IBrowser Browser)> OpenBrowser(ProxyServerConnectionInfo connection, string pool)
    {
        var browserUrl = await TestPuppeteerServer.GetBrowserUrl(connection.Network, pool);
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.ConnectOverCDPAsync(browserUrl.ToString());
        return (playwright, browser);
    }

    private static async Task SetPlaywrightObjects(BotData data, IPlaywright playwright, IBrowser browser)
    {
        data.SetObject("playwright", playwright, disposeExisting: false);
        data.SetObject("playwrightBrowser", browser, disposeExisting: false);

        var context = browser.Contexts.FirstOrDefault() ?? await browser.NewContextAsync();
        data.SetObject("playwrightContext", context, disposeExisting: false);

        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();
        data.SetObject("playwrightPage", page, disposeExisting: false);
        data.SetObject("playwrightFrame", page.MainFrame, disposeExisting: false);
    }

    private static BotData NewBotData()
    {
        var configSettings = new ConfigSettings();
        configSettings.BrowserSettings.Engine = BrowserAutomationEngine.Playwright;

        return new(
            new BotProviders(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            configSettings,
            new BotLogger(),
            new DataLine("browser-test", new WordlistType()))
        {
            CancellationToken = TestContext.Current.CancellationToken
        };
    }

    private static string UniquePool()
        => $"playwright-{Guid.NewGuid():N}";

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

    private static HttpBinResponse DeserializeHttpBinResponse(string source)
        => JsonConvert.DeserializeObject<HttpBinResponse>(source)
           ?? throw new InvalidOperationException("httpbin response could not be deserialized");
}
