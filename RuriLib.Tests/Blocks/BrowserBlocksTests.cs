using RuriLib.Exceptions;
using RuriLib.Functions.Browser;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Tests.Utils.Mockup;
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
using SeleniumBrowserMethods = RuriLib.Blocks.Selenium.Browser.Methods;
using SeleniumElementMethods = RuriLib.Blocks.Selenium.Elements.Methods;
using SeleniumPageMethods = RuriLib.Blocks.Selenium.Page.Methods;

namespace RuriLib.Tests.Blocks;

public class BrowserBlocksTests
{
    [Fact]
    public void SeleniumReload_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() =>
            SeleniumBrowserMethods.SeleniumReload(data));

        Assert.Equal("The browser is not open!", ex.Message);
    }

    [Fact]
    public void SeleniumGetWidth_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() =>
            SeleniumElementMethods.SeleniumGetWidth(
                data,
                FindElementBy.Id,
                "main",
                0));

        Assert.Equal("The browser is not open!", ex.Message);
    }

    [Fact]
    public async Task PuppeteerReload_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            PuppeteerBrowserMethods.PuppeteerReload(data));

        Assert.Equal("No pages open!", ex.Message);
    }

    [Fact]
    public async Task BrowserReload_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            BrowserBrowserMethods.BrowserReload(data));

        Assert.Equal("No pages open!", ex.Message);
    }

    [Fact]
    public async Task PuppeteerGetWidth_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            PuppeteerElementMethods.PuppeteerGetWidth(
                data,
                FindElementBy.Id,
                "main",
                0));

        Assert.Equal("No pages open!", ex.Message);
    }

    [Fact]
    public async Task BrowserGetWidth_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            BrowserElementMethods.BrowserGetWidth(
                data,
                FindElementBy.Id,
                "main",
                0));

        Assert.Equal("No pages open!", ex.Message);
    }

    [Fact]
    public void SeleniumNavigateTo_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() =>
            SeleniumPageMethods.SeleniumNavigateTo(data));

        Assert.Equal("The browser is not open!", ex.Message);
    }

    [Fact]
    public void SeleniumGetCurrentUrl_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() =>
            SeleniumPageMethods.SeleniumGetCurrentUrl(data));

        Assert.Equal("The browser is not open!", ex.Message);
    }

    [Fact]
    public void SeleniumClickAtCoordinates_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() =>
            SeleniumPageMethods.SeleniumClickAtCoordinates(data, 10, 20));

        Assert.Equal("The browser is not open!", ex.Message);
    }

    [Fact]
    public async Task PuppeteerNavigateTo_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            PuppeteerPageMethods.PuppeteerNavigateTo(data));

        Assert.Equal("No pages open!", ex.Message);
    }

    [Fact]
    public async Task BrowserNavigateTo_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            BrowserPageMethods.BrowserNavigateTo(data));

        Assert.Equal("No pages open!", ex.Message);
    }

    [Fact]
    public void PuppeteerGetCurrentUrl_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() =>
            PuppeteerPageMethods.PuppeteerGetCurrentUrl(data));

        Assert.Equal("No pages open!", ex.Message);
    }

    [Fact]
    public void BrowserGetCurrentUrl_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = Assert.Throws<BlockExecutionException>(() =>
            BrowserPageMethods.BrowserGetCurrentUrl(data));

        Assert.Equal("No pages open!", ex.Message);
    }

    [Fact]
    public async Task PuppeteerClickAtCoordinates_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            PuppeteerPageMethods.PuppeteerClickAtCoordinates(data, 10, 20));

        Assert.Equal("No pages open!", ex.Message);
    }

    [Fact]
    public async Task BrowserClickAtCoordinates_WithoutBrowser_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            BrowserPageMethods.BrowserClickAtCoordinates(data, 10, 20, BrowserMouseButton.Left));

        Assert.Equal("No pages open!", ex.Message);
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
            new DataLine("hello", new WordlistType()));
}
