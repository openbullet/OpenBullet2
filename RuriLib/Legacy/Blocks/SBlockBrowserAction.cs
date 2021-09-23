using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using RuriLib.Functions.Files;
using RuriLib.Functions.UserAgent;
using RuriLib.LS;
using RuriLib.ViewModels;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media;

namespace RuriLib
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

    /// <summary>
    /// A block that can interact with a selenium-driven browser.
    /// </summary>
    public class SBlockBrowserAction : BlockBase
    {
        private BrowserAction action = BrowserAction.Open;
        /// <summary>The action that is performed on the browser.</summary>
        public BrowserAction Action { get { return action; } set { action = value; OnPropertyChanged(); } }

        private string input = "";
        /// <summary>The input string.</summary>
        public string Input { get { return input; } set { input = value; OnPropertyChanged(); } }

        // Constructor
        /// <summary>
        /// Creates a BrowserAction block.
        /// </summary>
        public SBlockBrowserAction()
        {
            Label = "BROWSER ACTION";
        }

        /// <inheritdoc />
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
        public override void Process(BotData data)
        {
            base.Process(data);

            if (data.Driver == null && action != BrowserAction.Open)
            {
                data.Log(new LogEntry("Open a browser first!", Colors.White));
                throw new Exception("Browser not open");
            }

            var replacedInput = ReplaceValues(input, data);
            Actions keyActions = null;

            switch (action)
            {
                case BrowserAction.Open:
                    OpenBrowser(data);
                    try { UpdateSeleniumData(data); } catch { }
                    break;

                case BrowserAction.Close:
                    data.Driver.Close();
                    data.BrowserOpen = false;
                    break;

                case BrowserAction.Quit:
                    data.Driver.Quit();
                    data.BrowserOpen = false;
                    break;

                case BrowserAction.ClearCookies:
                    data.Driver.Manage().Cookies.DeleteAllCookies();
                    break;

                case BrowserAction.SendKeys:
                    keyActions = new Actions(data.Driver);
                    foreach(var s in replacedInput.Split(new string[] { "||" }, StringSplitOptions.None))
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
                                    keyActions.SendKeys(matchingField.GetValue(null).ToString());
                                }
                                else 
                                {
                                    keyActions.SendKeys(s);
                                }
                                break;
                        }
                    }
                    keyActions.Perform();
                    Thread.Sleep(1000);
                    if(replacedInput.Contains("<ENTER>") || replacedInput.Contains("<BACKSPACE>")) // These might lead to a page change
                        UpdateSeleniumData(data);
                    break;

                case BrowserAction.Screenshot:
                    var image = data.Driver.GetScreenshot();
                    Files.SaveScreenshot(image, data);
                    break;

                case BrowserAction.OpenNewTab:
                    ((IJavaScriptExecutor)data.Driver).ExecuteScript("window.open();");
                    data.Driver.SwitchTo().Window(data.Driver.WindowHandles.Last());
                    break;

                case BrowserAction.SwitchToTab:
                    data.Driver.SwitchTo().Window(data.Driver.WindowHandles[int.Parse(replacedInput)]);
                    UpdateSeleniumData(data);
                    break;

                case BrowserAction.CloseCurrentTab:
                    ((IJavaScriptExecutor)data.Driver).ExecuteScript("window.close();");
                    break;

                case BrowserAction.Refresh:
                    data.Driver.Navigate().Refresh();
                    break;

                case BrowserAction.Back:
                    data.Driver.Navigate().Back();
                    break;

                case BrowserAction.Forward:
                    data.Driver.Navigate().Forward();
                    break;

                case BrowserAction.Maximize:
                    data.Driver.Manage().Window.Maximize();
                    break;

                case BrowserAction.Minimize:
                    data.Driver.Manage().Window.Minimize();
                    break;

                case BrowserAction.FullScreen:
                    data.Driver.Manage().Window.FullScreen();
                    break;

                case BrowserAction.SetWidth:
                    data.Driver.Manage().Window.Size = new Size(int.Parse(replacedInput), data.Driver.Manage().Window.Size.Height);
                    break;

                case BrowserAction.SetHeight:
                    data.Driver.Manage().Window.Size = new Size(data.Driver.Manage().Window.Size.Width, int.Parse(replacedInput));
                    break;

                case BrowserAction.DOMtoSOURCE:
                    data.ResponseSource = data.Driver.FindElement(By.TagName("body")).GetAttribute("innerHTML");
                    break;

                case BrowserAction.GetCookies:
                    foreach(var cookie in data.Driver.Manage().Cookies.AllCookies)
                    {
                        try { data.Cookies.Add(cookie.Name, cookie.Value); } catch { }
                    }
                    break;

                case BrowserAction.SetCookies:
                    var baseURL = Regex.Match(ReplaceValues(input, data), "^(?:https?:\\/\\/)?(?:[^@\\/\n]+@)?([^:\\/?\n]+)").Groups[1].Value;
                    foreach (var cookie in data.Cookies)
                    {
                        try { data.Driver.Manage().Cookies.AddCookie(new Cookie(cookie.Key, cookie.Value, baseURL, "/", DateTime.MaxValue)); } catch { }
                    }
                    break;

                case BrowserAction.SwitchToDefault:
                    data.Driver.SwitchTo().DefaultContent();
                    break;

                case BrowserAction.SwitchToAlert:
                    data.Driver.SwitchTo().Alert();
                    break;

                case BrowserAction.SwitchToParentFrame:
                    data.Driver.SwitchTo().ParentFrame();
                    break;
            }

            data.Log(new LogEntry(string.Format("Executed browser action {0} on input {1}", action, ReplaceValues(input, data)), Colors.White));
        }

        /// <summary>
        /// Opens a browser of the given type (if not already open) and with the given settings.
        /// </summary>
        /// <param name="data">The BotData where the settings are stored.</param>
        public static void OpenBrowser(BotData data)
        {
            if (!data.BrowserOpen)
            {
                data.Log(new LogEntry("Opening browser...", Colors.White));

                switch (data.GlobalSettings.Selenium.Browser)
                {
                    case BrowserType.Chrome:
                        try
                        {
                            ChromeOptions chromeop = new ChromeOptions();
                            ChromeDriverService chromeservice = ChromeDriverService.CreateDefaultService();
                            chromeservice.SuppressInitialDiagnosticInformation = true;
                            chromeservice.HideCommandPromptWindow = true;   
                            chromeservice.EnableVerboseLogging = false;
                            chromeop.AddArgument("--log-level=3");
                            chromeop.BinaryLocation = data.GlobalSettings.Selenium.ChromeBinaryLocation;
                            if (data.GlobalSettings.Selenium.Headless || data.ConfigSettings.ForceHeadless) chromeop.AddArgument("--headless");
                            else if (data.GlobalSettings.Selenium.ChromeExtensions.Count > 0) // This should only be done when not headless
                                chromeop.AddExtensions(data.GlobalSettings.Selenium.ChromeExtensions
                                    .Where(ext => ext.EndsWith(".crx"))
                                    .Select(ext => Directory.GetCurrentDirectory() + "\\ChromeExtensions\\" + ext));
                            if (data.ConfigSettings.DisableNotifications) chromeop.AddArgument("--disable-notifications");
                            if (data.ConfigSettings.CustomCMDArgs != string.Empty) chromeop.AddArgument(data.ConfigSettings.CustomCMDArgs);
                            if (data.ConfigSettings.RandomUA) chromeop.AddArgument("--user-agent=" + UserAgent.Random(data.random));
                            else if (data.ConfigSettings.CustomUserAgent != string.Empty) chromeop.AddArgument("--user-agent=" + data.ConfigSettings.CustomUserAgent);

                            if (data.UseProxies) chromeop.AddArgument("--proxy-server=" + data.Proxy.Type.ToString().ToLower() + "://" + data.Proxy.Proxy);

                            data.Driver = new ChromeDriver(chromeservice, chromeop);
                        }
                        catch (Exception ex) { data.Log(new LogEntry(ex.ToString(), Colors.White)); return; }

                        break;

                    case BrowserType.Firefox:
                        try
                        {
                            FirefoxOptions fireop = new FirefoxOptions();
                            FirefoxDriverService fireservice = FirefoxDriverService.CreateDefaultService();
                            FirefoxProfile fireprofile = new FirefoxProfile();
                            
                            fireservice.SuppressInitialDiagnosticInformation = true;
                            fireservice.HideCommandPromptWindow = true;
                            fireop.AddArgument("--log-level=3");
                            fireop.BrowserExecutableLocation = data.GlobalSettings.Selenium.FirefoxBinaryLocation;
                            if (data.GlobalSettings.Selenium.Headless || data.ConfigSettings.ForceHeadless) fireop.AddArgument("--headless");
                            if (data.ConfigSettings.DisableNotifications) fireprofile.SetPreference("dom.webnotifications.enabled", false);
                            if (data.ConfigSettings.CustomCMDArgs != string.Empty) fireop.AddArgument(data.ConfigSettings.CustomCMDArgs);
                            if (data.ConfigSettings.RandomUA) fireprofile.SetPreference("general.useragent.override", UserAgent.Random(data.random));
                            else if (data.ConfigSettings.CustomUserAgent != string.Empty) fireprofile.SetPreference("general.useragent.override", data.ConfigSettings.CustomUserAgent);

                            if (data.UseProxies)
                            {
                                fireprofile.SetPreference("network.proxy.type", 1);
                                if (data.Proxy.Type == Extreme.Net.ProxyType.Http)
                                {
                                    fireprofile.SetPreference("network.proxy.http", data.Proxy.Host);
                                    fireprofile.SetPreference("network.proxy.http_port", int.Parse(data.Proxy.Port));
                                    fireprofile.SetPreference("network.proxy.ssl", data.Proxy.Host);
                                    fireprofile.SetPreference("network.proxy.ssl_port", int.Parse(data.Proxy.Port));
                                }
                                else
                                {
                                    fireprofile.SetPreference("network.proxy.socks", data.Proxy.Host);
                                    fireprofile.SetPreference("network.proxy.socks_port", int.Parse(data.Proxy.Port));
                                    if (data.Proxy.Type == Extreme.Net.ProxyType.Socks4)
                                        fireprofile.SetPreference("network.proxy.socks_version", 4);
                                    else if (data.Proxy.Type == Extreme.Net.ProxyType.Socks5)
                                        fireprofile.SetPreference("network.proxy.socks_version", 5);
                                }
                            }

                            fireop.Profile = fireprofile;
                            data.Driver = new FirefoxDriver(fireservice, fireop, new TimeSpan(0, 1, 0));
                            
                        }
                        catch(Exception ex) { data.Log(new LogEntry(ex.ToString(), Colors.White)); return; }
                        
                        break;
                }

                data.Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(data.GlobalSettings.Selenium.PageLoadTimeout);
                data.Log(new LogEntry("Opened!", Colors.White));
                data.BrowserOpen = true;
            }
            else
            {
                try
                {
                    UpdateSeleniumData(data);
                }
                catch { }
            }
        }
    }
}
