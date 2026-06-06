using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Proxies;
using RuriLib.Models.Settings;
using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks;

/// <summary>
/// A block that can interact with a selenium-driven browser.
/// </summary>
public class SBlockBrowserAction : BlockBase
{
    /// <summary>
    /// The actions that can be performed on a browser.
    /// </summary>
    public enum BrowserAction
    {
        /// <summary>Starts the driver and opens the browser.</summary>
        Open,

        /// <summary>Closes the browser but not the driver.</summary>
        Close,

        /// <summary>Closes the browser and disposes the driver.</summary>
        Quit,

        /// <summary>Clears the cookies in the browser.</summary>
        ClearCookies,

        /// <summary>Sends some keystrokes to the browser. Special keys like &lt;ENTER&gt; can be sent, separated by two pipe characters e.g. &lt;TAB&gt;||&lt;ENTER&gt;</summary>
        SendKeys,

        /// <summary>Takes a screenshot of the visible part of the page.</summary>
        Screenshot,

        /// <summary>Scrolls to the top of the page.</summary>
        ScrollToTop,

        /// <summary>Scrolls to the bottom of the page.</summary>
        ScrollToBottom,

        /// <summary>Scrolls down by a given number of pixels from the top of the page.</summary>
        Scroll,

        /// <summary>Opens a new tab.</summary>
        OpenNewTab,

        /// <summary>Closes the current tab.</summary>
        CloseCurrentTab,

        /// <summary>Switches to the browser tab with a given index.</summary>
        SwitchToTab,

        /// <summary>Refreshes the current page.</summary>
        Refresh,

        /// <summary>Goes to the previous page.</summary>
        Back,

        /// <summary>Goes to the next page.</summary>
        Forward,

        /// <summary>Maximizes the browser window.</summary>
        Maximize,

        /// <summary>Minimizes the browser window.</summary>
        Minimize,

        /// <summary>Sets the browser window as full screen.</summary>
        FullScreen,

        /// <summary>Sets the browser window's width.</summary>
        SetWidth,

        /// <summary>Sets the browser window's height.</summary>
        SetHeight,

        /// <summary>Sets the DOM (the page source modified by the javascript code) into the SOURCE variable.</summary>
        DOMtoSOURCE,

        /// <summary>Transfers the cookies from the browser to the HTTP cookie jar.</summary>
        GetCookies,

        /// <summary>Transfers the cookies from the HTTP cookie jar to the browser.</summary>
        SetCookies,

        /// <summary>Switches to the default content.</summary>
        SwitchToDefault,

        /// <summary>Switches to the alert message.</summary>
        SwitchToAlert,

        /// <summary>Switches to the default frame (useful to get out of an iframe).</summary>
        SwitchToParentFrame
    }

    /// <summary>The action that is performed on the browser.</summary>
    public BrowserAction Action { get; set; } = BrowserAction.Open;

    /// <summary>The input string.</summary>
    public string Input { get; set; } = "";

    // Constructor
    /// <summary>
    /// Creates a BrowserAction block.
    /// </summary>
    public SBlockBrowserAction()
    {
        Label = "BROWSER ACTION";
    }

    /// <summary>
    /// Parses the block from a legacy LoliScript line.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <returns>The current block instance.</returns>
    public override BlockBase FromLS(string line)
    {
        // Trim the line
        var input = line.Trim();

        // Parse the label
        if (input.StartsWith("#"))
            Label = LineParser.ParseLabel(ref input);

        /*
         * Syntax:
         * BROWSERACTION ACTION ["INPUT"]
         * */

        Action = (BrowserAction)LineParser.ParseEnum(ref input, "ACTION", typeof(BrowserAction));

        if (input != string.Empty) Input = LineParser.ParseLiteral(ref input, "INPUT");

        return this;
    }

    /// <inheritdoc />
    public override string ToLS(bool indent = true)
    {
        var writer = new BlockWriter(GetType(), indent, Disabled);
        writer
            .Label(Label)
            .Token("BROWSERACTION")
            .Token(Action)
            .Literal(Input, "Input");
        return writer.ToString();
    }

    /// <inheritdoc />
    public override async Task Process(LSGlobals ls)
    {
        var data = ls.BotData;
        await base.Process(ls);

        var browser = data.TryGetObject<WebDriver>("selenium");

        if (browser == null && Action != BrowserAction.Open)
        {
            throw new Exception("Open a browser first!");
        }

        var replacedInput = ReplaceValues(Input, ls);
        var validatedBrowser = browser;
        Actions? keyActions = null;

        switch (Action)
        {
            case BrowserAction.Open:
                OpenBrowser(data);
                UpdateSeleniumData(data);
                break;

            case BrowserAction.Close:
                validatedBrowser!.Close();
                data.SetObject("selenium", null);
                break;

            case BrowserAction.Quit:
                validatedBrowser!.Quit();
                data.SetObject("selenium", null);
                break;

            case BrowserAction.ClearCookies:
                validatedBrowser!.Manage().Cookies.DeleteAllCookies();
                break;

            case BrowserAction.SendKeys:
                keyActions = new Actions(validatedBrowser!);
                foreach (var s in replacedInput.Split(new string[] { "||" }, StringSplitOptions.None))
                {
                    switch (s)
                    {
                        case "<TAB>":
                            keyActions.SendKeys(Keys.Tab);
                            break;

                        case "<ENTER>":
                            keyActions.SendKeys(Keys.Enter);
                            break;

                        case "<BACKSPACE>":
                            keyActions.SendKeys(Keys.Backspace);
                            break;

                        case "<ESC>":
                            keyActions.SendKeys(Keys.Escape);
                            break;

                        default:
                            // List of available keys https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriver/Keys.cs
                            var keyFields = typeof(Keys).GetFields();
                            var matchingField = keyFields.FirstOrDefault(f =>
                                $"<{f.Name}>".Equals(s, StringComparison.InvariantCultureIgnoreCase));

                            if (matchingField != null)
                            {
                                keyActions.SendKeys(matchingField.GetValue(null)?.ToString() ?? string.Empty);
                            }
                            else
                            {
                                keyActions.SendKeys(s);
                            }
                            break;
                    }
                }
                keyActions.Perform();
                await Task.Delay(1000);
                if (replacedInput.Contains("<ENTER>") || replacedInput.Contains("<BACKSPACE>")) // These might lead to a page change
                    UpdateSeleniumData(data);
                break;

            case BrowserAction.Screenshot:
                var screenshotFile = Utils.GetScreenshotPath(data);
                validatedBrowser!.GetScreenshot().SaveAsFile(screenshotFile);
                break;

            case BrowserAction.OpenNewTab:
                ((IJavaScriptExecutor)validatedBrowser!).ExecuteScript("window.open();");
                validatedBrowser.SwitchTo().Window(validatedBrowser.WindowHandles.Last());
                break;

            case BrowserAction.SwitchToTab:
                validatedBrowser!.SwitchTo().Window(validatedBrowser.WindowHandles[int.Parse(replacedInput)]);
                UpdateSeleniumData(data);
                break;

            case BrowserAction.CloseCurrentTab:
                ((IJavaScriptExecutor)validatedBrowser!).ExecuteScript("window.close();");
                break;

            case BrowserAction.Refresh:
                validatedBrowser!.Navigate().Refresh();
                break;

            case BrowserAction.Back:
                validatedBrowser!.Navigate().Back();
                break;

            case BrowserAction.Forward:
                validatedBrowser!.Navigate().Forward();
                break;

            case BrowserAction.Maximize:
                validatedBrowser!.Manage().Window.Maximize();
                break;

            case BrowserAction.Minimize:
                validatedBrowser!.Manage().Window.Minimize();
                break;

            case BrowserAction.FullScreen:
                validatedBrowser!.Manage().Window.FullScreen();
                break;

            case BrowserAction.SetWidth:
                validatedBrowser!.Manage().Window.Size = new Size(int.Parse(replacedInput), validatedBrowser.Manage().Window.Size.Height);
                break;

            case BrowserAction.SetHeight:
                validatedBrowser!.Manage().Window.Size = new Size(validatedBrowser.Manage().Window.Size.Width, int.Parse(replacedInput));
                break;

            case BrowserAction.DOMtoSOURCE:
                data.SOURCE = validatedBrowser!.FindElement(By.TagName("body")).GetAttribute("innerHTML")!;
                break;

            case BrowserAction.GetCookies:
                foreach (var cookie in validatedBrowser!.Manage().Cookies.AllCookies)
                {
                    if (!string.IsNullOrWhiteSpace(cookie.Name))
                    {
                        data.COOKIES[cookie.Name] = cookie.Value;
                    }
                }
                break;

            case BrowserAction.SetCookies:
                var baseURL = Regex.Match(replacedInput, "^(?:https?:\\/\\/)?(?:[^@\\/\n]+@)?([^:\\/?\n]+)").Groups[1].Value;
                foreach (var cookie in data.COOKIES)
                {
                    try
                    {
                        validatedBrowser!.Manage().Cookies.AddCookie(new Cookie(cookie.Key, cookie.Value, baseURL, "/", DateTime.MaxValue));
                    }
                    catch
                    {

                    }
                }
                break;

            case BrowserAction.SwitchToDefault:
                validatedBrowser!.SwitchTo().DefaultContent();
                break;

            case BrowserAction.SwitchToAlert:
                validatedBrowser!.SwitchTo().Alert();
                break;

            case BrowserAction.SwitchToParentFrame:
                validatedBrowser!.SwitchTo().ParentFrame();
                break;
        }

        data.Logger.Log(string.Format("Executed browser action {0} on input {1}", Action, replacedInput), LogColors.White);
    }

    /// <summary>
    /// Opens and configures the Selenium browser for the provided bot.
    /// </summary>
    /// <param name="data">The bot data that will receive the browser instance.</param>
    public static void OpenBrowser(BotData data)
    {
        var browser = data.TryGetObject<WebDriver>("selenium");

        if (browser != null)
        {
            data.Logger.Log("The browser is already open", LogColors.White);
            UpdateSeleniumData(data);
            return;
        }

        data.Logger.Log("Opening browser...", LogColors.White);

        var provider = data.Providers.SeleniumBrowser;

        switch (provider.BrowserType)
        {
            case SeleniumBrowserType.Chrome:
                var chromeOptions = new ChromeOptions();
                var chromeService = ChromeDriverService.CreateDefaultService();
                chromeService.SuppressInitialDiagnosticInformation = true;
                chromeService.HideCommandPromptWindow = true;
                chromeService.EnableVerboseLogging = false;
                chromeOptions.AddArgument("--log-level=3");
                chromeOptions.BinaryLocation = provider.ChromeBinaryLocation;

                if (Helpers.Utils.IsDocker())
                {
                    chromeOptions.AddArgument("--no-sandbox");
                    chromeOptions.AddArgument("--whitelisted-ips=''");
                    chromeOptions.AddArgument("--disable-dev-shm-usage");
                }

                if (data.ConfigSettings.BrowserSettings.Headless)
                {
                    chromeOptions.AddArgument("--headless");
                }

                // TODO: Readd support for chrome extensions

                if (data.ConfigSettings.BrowserSettings.DismissDialogs)
                {
                    chromeOptions.AddArgument("--disable-notifications");
                }

                var chromeArguments = Helpers.CommandLineArgumentParser.Parse(data.ConfigSettings.BrowserSettings.CommandLineArgs);
                if (chromeArguments.Length > 0)
                {
                    chromeOptions.AddArguments(chromeArguments);
                }

                if (data.UseProxy)
                {
                    // TODO: Add support for auth proxies using yove
                    if (data.Proxy == null)
                    {
                        throw new InvalidOperationException("A proxy is required when UseProxy is enabled");
                    }

                    chromeOptions.AddArgument($"--proxy-server={data.Proxy.Type.ToString().ToLower()}://{data.Proxy.Host}:{data.Proxy.Port}");
                }

                data.SetObject("selenium", new ChromeDriver(chromeService, chromeOptions));
                break;

            case SeleniumBrowserType.Firefox:
                var firefoxOptions = new FirefoxOptions();
                var firefoxService = FirefoxDriverService.CreateDefaultService();
                var firefoxProfile = new FirefoxProfile();

                firefoxService.SuppressInitialDiagnosticInformation = true;
                firefoxService.HideCommandPromptWindow = true;
                firefoxOptions.AddArgument("--log-level=3");
                firefoxOptions.BinaryLocation = provider.FirefoxBinaryLocation;

                if (Helpers.Utils.IsDocker())
                {
                    firefoxOptions.AddArgument("--whitelisted-ips=''");
                }

                if (data.ConfigSettings.BrowserSettings.Headless)
                {
                    firefoxOptions.AddArgument("--headless");
                }

                if (data.ConfigSettings.BrowserSettings.DismissDialogs)
                {
                    firefoxProfile.SetPreference("dom.webnotifications.enabled", false);
                }

                var firefoxArguments = Helpers.CommandLineArgumentParser.Parse(data.ConfigSettings.BrowserSettings.CommandLineArgs);
                if (firefoxArguments.Length > 0)
                {
                    firefoxOptions.AddArguments(firefoxArguments);
                }

                if (data.UseProxy)
                {
                    firefoxProfile.SetPreference("network.proxy.type", 1);
                    if (data.Proxy == null)
                    {
                        throw new InvalidOperationException("A proxy is required when UseProxy is enabled");
                    }

                    if (data.Proxy.Type == ProxyType.Http)
                    {
                        firefoxProfile.SetPreference("network.proxy.http", data.Proxy.Host);
                        firefoxProfile.SetPreference("network.proxy.http_port", data.Proxy.Port);
                        firefoxProfile.SetPreference("network.proxy.ssl", data.Proxy.Host);
                        firefoxProfile.SetPreference("network.proxy.ssl_port", data.Proxy.Port);
                    }
                    else
                    {
                        firefoxProfile.SetPreference("network.proxy.socks", data.Proxy.Host);
                        firefoxProfile.SetPreference("network.proxy.socks_port", data.Proxy.Port);

                        if (data.Proxy.Type == ProxyType.Socks4)
                        {
                            firefoxProfile.SetPreference("network.proxy.socks_version", 4);
                        }
                        else if (data.Proxy.Type == ProxyType.Socks5)
                        {
                            firefoxProfile.SetPreference("network.proxy.socks_version", 5);
                        }
                    }
                }

                firefoxOptions.Profile = firefoxProfile;
                data.SetObject("selenium", new FirefoxDriver(firefoxService, firefoxOptions, new TimeSpan(0, 1, 0)));
                break;
        }

        data.Logger.Log("Opened!", LogColors.White);
    }
}
