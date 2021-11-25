using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using RuriLib.Attributes;
using RuriLib.Functions.Files;
using RuriLib.Functions.Puppeteer;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Selenium.Elements
{
    [BlockCategory("Elements", "Blocks for interacting with elements on a selenium browser page", "#bdda57")]
    public static class Methods
    {
        [Block("Sets the value of the specified attribute of an element", name = "Set Attribute Value")]
        public static void SeleniumSetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index,
            string attributeName, string value)
        {
            data.Logger.LogHeader();

            var elemScript = GetElementScript(findBy, identifier, index);
            var script = elemScript + $".setAttribute('{attributeName}', '{value}');";
            GetBrowser(data).ExecuteScript(script);
            UpdateSeleniumData(data);

            data.Logger.Log($"Set value {value} of attribute {attributeName} by executing {script}", LogColors.JuneBud);
        }

        [Block("Clears the text in an input field", name = "Clear Field")]
        public static void SeleniumClearField(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            element.Clear();

            data.Logger.Log("Cleared the field", LogColors.JuneBud);
        }

        [Block("Types text in an input field", name = "Type")]
        public static async Task SeleniumTypeElement(BotData data, FindElementBy findBy, string identifier, int index,
            string text, int timeBetweenKeystrokes = 0)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            
            foreach (var c in text)
            {
                element.SendKeys(c.ToString());
                await Task.Delay(timeBetweenKeystrokes);
            }

            UpdateSeleniumData(data);

            data.Logger.Log($"Typed {text}", LogColors.JuneBud);
        }

        [Block("Types text in an input field with human-like random delays", name = "Type Human")]
        public static async Task SeleniumTypeElementHuman(BotData data, FindElementBy findBy, string identifier, int index,
            string text)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);

            foreach (var c in text)
            {
                element.SendKeys(c.ToString());
                await Task.Delay(data.Random.Next(100, 300)); // Wait between 100 and 300 ms (average human type speed is 60 WPM ~ 360 CPM)
            }

            UpdateSeleniumData(data);

            data.Logger.Log($"Typed {text}", LogColors.JuneBud);
        }

        [Block("Clicks an element", name = "Click")]
        public static void SeleniumClick(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            element.Click();
            UpdateSeleniumData(data);

            data.Logger.Log("Clicked the element", LogColors.JuneBud);
        }

        [Block("Submits a form", name = "Submit")]
        public static void SeleniumSubmit(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            element.Submit();
            UpdateSeleniumData(data);

            data.Logger.Log($"Submitted the form", LogColors.JuneBud);
        }

        [Block("Selects a value in a select element", name = "Select")]
        public static void SeleniumSelect(BotData data, FindElementBy findBy, string identifier, int index, string value)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            new SelectElement(element).SelectByValue(value);
            UpdateSeleniumData(data);

            data.Logger.Log($"Selected value {value}", LogColors.JuneBud);
        }

        [Block("Selects a value by index in a select element", name = "Select by Index")]
        public static void SeleniumSelectByIndex(BotData data, FindElementBy findBy, string identifier, int index, int selectionIndex)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            new SelectElement(element).SelectByIndex(selectionIndex);
            UpdateSeleniumData(data);

            data.Logger.Log($"Selected value at index {selectionIndex}", LogColors.JuneBud);
        }

        [Block("Selects a value by text in a select element", name = "Select by Text")]
        public static void SeleniumSelectByText(BotData data, FindElementBy findBy, string identifier, int index, string text)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            new SelectElement(element).SelectByText(text);
            UpdateSeleniumData(data);

            data.Logger.Log($"Selected text {text}", LogColors.JuneBud);
        }

        [Block("Gets the value of an attribute of an element", name = "Get Attribute Value")]
        public static string SeleniumGetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index,
            string attributeName = "innerText")
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            var value = element.GetAttribute(attributeName);

            data.Logger.Log($"Got value {value} of attribute {attributeName}", LogColors.JuneBud);
            return value;
        }

        [Block("Gets the values of an attribute of multiple elements", name = "Get Attribute Value All")]
        public static List<string> SeleniumGetAttributeValueAll(BotData data, FindElementBy findBy, string identifier,
            string attributeName = "innerText")
        {
            data.Logger.LogHeader();

            var elements = GetElements(data, findBy, identifier);
            var values = elements.Select(e => e.GetAttribute(attributeName)).ToList();

            data.Logger.Log($"Got {values.Count} values for attribute {attributeName}", LogColors.JuneBud);
            return values;
        }

        [Block("Checks if an element is currently being displayed on the page", name = "Is Displayed")]
        public static bool SeleniumIsDisplayed(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            var displayed = element.Displayed;

            data.Logger.Log($"Found out the element is{(displayed ? "" : " not")} displayed", LogColors.JuneBud);
            return displayed;
        }

        [Block("Checks if an element is currently enabled", name = "Is Enabled")]
        public static bool SeleniumIsEnabled(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            var enabled = element.Enabled;

            data.Logger.Log($"Found out the element is{(enabled ? "" : " not")} enabled", LogColors.JuneBud);
            return enabled;
        }

        [Block("Checks if an element exists on the page", name = "Exists")]
        public static bool SeleniumExists(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            try
            {
                var element = GetElement(data, findBy, identifier, index);
                data.Logger.Log("The element exists", LogColors.JuneBud);
                return true;
            }
            catch
            {
                data.Logger.Log("The element does not exist", LogColors.JuneBud);
                return false;
            }
        }

        [Block("Gets the X coordinate of the element in pixels", name = "Get Position X")]
        public static int SeleniumGetPositionX(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            var x = element.Location.X;

            data.Logger.Log($"The X coordinate of the element is {x}", LogColors.JuneBud);
            return x;
        }

        [Block("Gets the Y coordinate of the element in pixels", name = "Get Position Y")]
        public static int SeleniumGetPositionY(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            var y = element.Location.Y;

            data.Logger.Log($"The Y coordinate of the element is {y}", LogColors.JuneBud);
            return y;
        }

        [Block("Gets the width of the element in pixels", name = "Get Width")]
        public static int SeleniumGetWidth(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            var width = element.Size.Width;

            data.Logger.Log($"The width of the element is {width}", LogColors.JuneBud);
            return width;
        }

        [Block("Gets the height of the element in pixels", name = "Get Height")]
        public static int SeleniumGetHeight(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var element = GetElement(data, findBy, identifier, index);
            var height = element.Size.Height;

            data.Logger.Log($"The height of the element is {height}", LogColors.JuneBud);
            return height;
        }

        [Block("Takes a screenshot of the element and saves it to an output file", name = "Screenshot Element")]
        public static void SeleniumScreenshotElement(BotData data, FindElementBy findBy, string identifier, int index, string fileName)
        {
            data.Logger.LogHeader();

            if (data.Providers.Security.RestrictBlocksToCWD)
                FileUtils.ThrowIfNotInCWD(fileName);

            var browser = GetBrowser(data);
            var element = GetElement(data, findBy, identifier, index);

            using var img = TakeElementScreenshot(browser, element);
            img.Save(fileName);

            data.Logger.Log($"Took a screenshot of the element and saved it to {fileName}", LogColors.JuneBud);
        }

        [Block("Takes a screenshot of the element and converts it to a base64 string", name = "Screenshot Element Base64")]
        public static string SeleniumScreenshotBase64(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var browser = GetBrowser(data);
            var element = GetElement(data, findBy, identifier, index);

            using var img = TakeElementScreenshot(browser, element);
            using var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Jpeg);
            var base64 = Convert.ToBase64String(ms.ToArray());

            data.Logger.Log($"Took a screenshot of the element as base64", LogColors.JuneBud);
            return base64;
        }

        [Block("Switches to a different iframe", name = "Switch to Frame")]
        public static void SeleniumSwitchToFrame(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var browser = GetBrowser(data);
            var element = GetElement(data, findBy, identifier, index);
            browser.SwitchTo().Frame(element);

            data.Logger.Log($"Switched to iframe", LogColors.JuneBud);
        }

        [Block("Waits for an element to appear on the page", name = "Wait for Element")]
        public static async Task SeleniumWaitForElement(BotData data, FindElementBy findBy, string identifier, int timeout = 30000)
        {
            data.Logger.LogHeader();

            var waited = 0;

            while (waited < timeout)
            {
                var elements = GetElements(data, findBy, identifier);

                if (elements.Any())
                {
                    data.Logger.Log($"Waited for element with {findBy} {identifier}", LogColors.JuneBud);
                    return;
                }
                
                waited += 200;
                await Task.Delay(200);
            }

            throw new TimeoutException($"Timed out while waiting for element with {findBy} {identifier}");
        }

        private static IWebElement GetElement(BotData data, FindElementBy findBy, string identifier, int index)
        {
            var elements = GetElements(data, findBy, identifier).ToArray();

            if (elements.Length < index + 1)
            {
                throw new Exception($"Expected at least {index + 1} elements to be found but {elements.Length} were found");
            }

            return elements[index];
        }

        private static IEnumerable<IWebElement> GetElements(BotData data, FindElementBy findBy, string identifier)
        {
            var browser = GetBrowser(data);

            return findBy switch
            {
                FindElementBy.Class => browser.FindElements(By.ClassName(identifier)),
                FindElementBy.Id => browser.FindElements(By.Id(identifier)),
                FindElementBy.Selector => browser.FindElements(By.CssSelector(identifier)),
                FindElementBy.XPath => browser.FindElements(By.XPath(identifier)),
                _ => throw new NotImplementedException(),
            };
        }

        private static string GetElementScript(FindElementBy findBy, string identifier, int index)
            => findBy == FindElementBy.XPath
            ? $"document.evaluate(\"{identifier.Replace("\"", "\\\"")}\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue"
            : $"document.querySelectorAll('{BuildSelector(findBy, identifier)}')[{index}]";

        private static string BuildSelector(FindElementBy findBy, string identifier)
            => findBy switch
            {
                FindElementBy.Id => '#' + identifier,
                FindElementBy.Class => '.' + string.Join('.', identifier.Split(' ')), // "class1 class2" => ".class1.class2"
                FindElementBy.Selector => identifier,
                _ => throw new NotSupportedException()
            };

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

        private static Bitmap TakeElementScreenshot(IWebDriver driver, IWebElement element)
        {
            var sc = (driver as ITakesScreenshot).GetScreenshot();
            using var ms = new MemoryStream(sc.AsByteArray);
            using var img = Image.FromStream(ms) as Bitmap;
            return img.Clone(new Rectangle(element.Location, element.Size), img.PixelFormat);
        }
    }
}
