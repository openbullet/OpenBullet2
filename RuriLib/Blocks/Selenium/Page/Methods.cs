using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Blocks.Selenium.Page
{
    [BlockCategory("Page", "Blocks for interacting with a selenium browser page", "#bdda57")]
    public static class Methods
    {
        [Block("Navigates to a given URL in the current page", name = "Navigate To")]
        public static void SeleniumNavigateTo(BotData data, string url = "https://example.com", int timeout = 30000)
        {
            data.Logger.LogHeader();

            var browser = GetBrowser(data);
            browser.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(timeout);
            browser.Navigate().GoToUrl(url);
            UpdateSeleniumData(data);

            data.Logger.Log($"Navigated to {url}", LogColors.JuneBud);
        }

        [Block("Clears the page cookies", name = "Clear Cookies")]
        public static void SeleniumClearCookies(BotData data)
        {
            data.Logger.LogHeader();

            GetBrowser(data).Manage().Cookies.DeleteAllCookies();
            data.Logger.Log($"Deleted all cookies from the page", LogColors.JuneBud);
        }

        [Block("Sends a key to the page", name = "Page Type")]
        public static void SeleniumPageType(BotData data, string text)
        {
            data.Logger.LogHeader();

            new Actions(GetBrowser(data))
                .SendKeys(text)
                .Perform();

            UpdateSeleniumData(data);

            data.Logger.Log($"Typed {text}", LogColors.JuneBud);
        }

        [Block("Presses and releases a key in the browser page", name = "Key Press in Page",
            extraInfo = "Full list of keys here: https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriver/Keys.cs")]
        public static void SeleniumPageKeyPress(BotData data, string key)
        {
            data.Logger.LogHeader();

            new Actions(GetBrowser(data))
                .SendKeys(GetKeyCode(key))
                .Perform();

            UpdateSeleniumData(data);

            data.Logger.Log($"Pressed and released {key}", LogColors.JuneBud);

            // Full list of keys: https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriver/Keys.cs
        }

        [Block("Presses a key in the browser page without releasing it", name = "Key Down in Page",
            extraInfo = "Full list of keys here: https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriver/Keys.cs")]
        public static void SeleniumPageKeyDown(BotData data, string key)
        {
            data.Logger.LogHeader();

            new Actions(GetBrowser(data))
                .KeyDown(GetKeyCode(key))
                .Perform();

            UpdateSeleniumData(data);

            // Full list of keys: https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriver/Keys.cs
        }

        [Block("Releases a key that was previously pressed in the browser page", name = "Key Up in Page",
            extraInfo = "Full list of keys here: https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriver/Keys.cs")]
        public static void SeleniumKeyUp(BotData data, string key)
        {
            data.Logger.LogHeader();

            new Actions(GetBrowser(data))
                .KeyUp(GetKeyCode(key))
                .Perform();

            UpdateSeleniumData(data);

            data.Logger.Log($"Released {key}", LogColors.JuneBud);

            // Full list of keys: https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriver/Keys.cs
        }

        [Block("Takes a screenshot of the entire browser page and saves it to an output file", name = "Screenshot Page")]
        public static void SeleniumScreenshotPage(BotData data, string file)
        {
            data.Logger.LogHeader();

            var screenshot = GetBrowser(data).GetScreenshot();
            screenshot.SaveAsFile(file);

            data.Logger.Log($"Took a screenshot of the page and saved it to {file}", LogColors.JuneBud);
        }

        [Block("Takes a screenshot of the entire browser page and converts it to a base64 string", name = "Screenshot Page Base64")]
        public static string SeleniumScreenshotPageBase64(BotData data)
        {
            data.Logger.LogHeader();

            var screenshot = GetBrowser(data).GetScreenshot();

            data.Logger.Log("Took a screenshot of the page as base64", LogColors.JuneBud);
            return screenshot.AsBase64EncodedString;
        }

        [Block("Scrolls the page by a given amount of pixels", name = "Scroll by")]
        public static void SeleniumScrollBy(BotData data, int x, int y)
        {
            data.Logger.LogHeader();

            new Actions(GetBrowser(data))
                .MoveByOffset(x, y)
                .Perform();

            data.Logger.Log($"Scrolled by {x} px to the right and {y} px to the bottom", LogColors.JuneBud);
        }

        [Block("Gets the full DOM of the page", name = "Get DOM")]
        public static string SeleniumGetDOM(BotData data)
        {
            data.Logger.LogHeader();

            var dom = GetBrowser(data).FindElement(By.TagName("body")).GetAttribute("innerHTML");

            data.Logger.Log("Got the full page DOM", LogColors.JuneBud);
            data.Logger.Log(dom, LogColors.JuneBud, true);
            return dom;
        }

        [Block("Gets the cookies for a given domain from the browser", name = "Get Cookies")]
        public static Dictionary<string, string> SeleniumGetCookies(BotData data, string domain)
        {
            data.Logger.LogHeader();

            var cookies = GetBrowser(data).Manage().Cookies.AllCookies
                .Where(c => c.Domain.Contains(domain, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            data.Logger.Log($"Got {cookies.Length} cookies for {domain}", LogColors.JuneBud);
            return cookies.ToDictionary(c => c.Name, c => c.Value);
        }

        [Block("Sets the cookies for a given domain in the browser page", name = "Set Cookies")]
        public static void SeleniumSetCookies(BotData data, string domain, Dictionary<string, string> cookies)
        {
            data.Logger.LogHeader();

            var browser = GetBrowser(data);

            foreach (var cookie in cookies)
            {
                browser.Manage().Cookies.AddCookie(new Cookie(cookie.Key, cookie.Value, domain, "/", DateTime.MaxValue));
            }

            data.Logger.Log($"Set {cookies.Count} cookies for {domain}", LogColors.JuneBud);
        }

        [Block("Switches to the main frame of the page", name = "Switch to Main Frame")]
        public static void SeleniumSwitchToMainFrame(BotData data)
        {
            data.Logger.LogHeader();

            GetBrowser(data).SwitchTo().DefaultContent();
            data.Logger.Log($"Switched to main frame", LogColors.JuneBud);
        }

        [Block("Switches to the alert frame of the page", name = "Switch to Alert")]
        public static void SeleniumSwitchToAlert(BotData data)
        {
            data.Logger.LogHeader();

            GetBrowser(data).SwitchTo().Alert();
            data.Logger.Log($"Switched to alert frame", LogColors.JuneBud);
        }

        [Block("Switches to the parent frame", name = "Switch to Parent Frame")]
        public static void SeleniumSwitchToParent(BotData data)
        {
            data.Logger.LogHeader();

            GetBrowser(data).SwitchTo().ParentFrame();
            data.Logger.Log($"Switched to parent frame", LogColors.JuneBud);
        }

        [Block("Evaluates a js expression in the current page and returns a json response", name = "Execute JS")]
        public static string SeleniumExecuteJs(BotData data, [MultiLine] string expression)
        {
            data.Logger.LogHeader();

            var scriptResult = GetBrowser(data).ExecuteScript(expression);
            var json = scriptResult?.ToString() ?? "undefined";
            UpdateSeleniumData(data);

            data.Logger.Log($"Evaluated {expression}", LogColors.JuneBud);
            data.Logger.Log($"Got result: {json}", LogColors.JuneBud);

            return json;
        }

        private static WebDriver GetBrowser(BotData data)
                => data.TryGetObject<WebDriver>("selenium") ?? throw new Exception("The browser is not open!");

        private static void UpdateSeleniumData(BotData data)
        {
            var browser = data.TryGetObject<WebDriver>("selenium");

            if (browser != null)
            {
                data.ADDRESS = browser.Url;
                data.SOURCE = browser.PageSource;
            }
        }

        private static string GetKeyCode(string key)
        {
            var keyFields = typeof(Keys).GetFields();
            var matchingField = keyFields.FirstOrDefault(f => f.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));

            if (matchingField == null)
            {
                throw new Exception($"Invalid key name: {key}");
            }

            return matchingField.GetValue(null).ToString();
        }
    }
}
