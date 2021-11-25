using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that can perform an action on an element inside an HTML page.
    /// </summary>
    public class SBlockElementAction : BlockBase
    {
        /// <summary>
        /// The available ways to address an element.
        /// </summary>
        public enum ElementLocator
        {
            /// <summary>The id of the element.</summary>
            Id,

            /// <summary>The class of the element.</summary>
            Class,

            /// <summary>The name of the element.</summary>
            Name,

            /// <summary>The tag of the element.</summary>
            Tag,

            /// <summary>The CSS selector of the element.</summary>
            Selector,

            /// <summary>The xpath of the element.</summary>
            XPath
        }

        /// <summary>
        /// The actions that can be performed on an element.
        /// </summary>
        public enum ElementAction
        {
            /// <summary>Clears the text of an input element.</summary>
            Clear,

            /// <summary>Sends keystrokes to an input element.</summary>
            SendKeys,

            /// <summary>Types keystrokes into an input element with random delays between each keystroke, like a human would.</summary>
            SendKeysHuman,

            /// <summary>Clicks an element.</summary>
            Click,

            /// <summary>Submits a form element.</summary>
            Submit,

            /// <summary>Selects an option from a select element by visible text.</summary>
            SelectOptionByText,

            /// <summary>Selects an option from a select element by index.</summary>
            SelectOptionByIndex,

            /// <summary>Selects an option from a select element by value.</summary>
            SelectOptionByValue,

            /// <summary>Gets the text inside an element.</summary>
            GetText,

            /// <summary>Gets a given attribute of an element.</summary>
            GetAttribute,

            /// <summary>Checks if the element is currently displayed on the page.</summary>
            IsDisplayed,

            /// <summary>Checks if the element is enabled on the page.</summary>
            IsEnabled,

            /// <summary>Checks if the element is selected.</summary>
            IsSelected,

            /// <summary>Retrieves the X coordinate of the top-left corner of the element.</summary>
            LocationX,

            /// <summary>Retrieves the Y coordinate of the top-left corner of the element.</summary>
            LocationY,

            /// <summary>Retrieves the width of the element.</summary>
            SizeX,

            /// <summary>Retrieves the height of the element.</summary>
            SizeY,

            /// <summary>Takes a screenshot of the element and saves it as an image.</summary>
            Screenshot,

            /// <summary>Takes a screenshot of the element and saves it as a base64-encoded string.</summary>
            ScreenshotBase64,

            /// <summary>Switches to the iframe element.</summary>
            SwitchToFrame,

            /// <summary>Waits until the element appears in the DOM (up to a specified timeout).</summary>
            WaitForElement
        }

        /// <summary>The element locator.</summary>
        public ElementLocator Locator { get; set; } = ElementLocator.Id;

        /// <summary>The value of the locator.</summary>
        public string ElementString { get; set; } = "";

        /// <summary>The index of the element in case the locator matches more than one.</summary>
        public int ElementIndex { get; set; } = 0;

        /// <summary>The action to be performed on the element.</summary>
        public ElementAction Action { get; set; } = ElementAction.Clear;

        /// <summary>The input data.</summary>
        public string Input { get; set; } = "";

        /// <summary>The name of the output variable.</summary>
        public string OutputVariable { get; set; } = "";

        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get; set; } = false;

        /// <summary>Whether the GetText and GetAttribute actions should be executed on all the elements matched by the locator and return a list of values.</summary>
        public bool Recursive { get; set; } = false;

        /// <summary>
        /// Creates an element action block.
        /// </summary>
        public SBlockElementAction()
        {
            Label = "ELEMENT ACTION";
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
             * ELEMENTACTION ID "id" 0 RECURSIVE? ACTION ["INPUT"] [-> VAR/CAP "OUTPUT"]
             * */

            Locator = (ElementLocator)LineParser.ParseEnum(ref input, "LOCATOR", typeof(ElementLocator));
            ElementString = LineParser.ParseLiteral(ref input, "STRING");
            if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                LineParser.SetBool(ref input, this);
            else if (LineParser.Lookahead(ref input) == TokenType.Integer)
                ElementIndex = LineParser.ParseInt(ref input, "INDEX");
            Action = (ElementAction)LineParser.ParseEnum(ref input, "ACTION", typeof(ElementAction));

            if (input == string.Empty) return this;
            if (LineParser.Lookahead(ref input) == TokenType.Literal)
                Input = LineParser.ParseLiteral(ref input, "INPUT");

            // Try to parse the arrow, otherwise just return the block as is with default var name and var / cap choice
            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == string.Empty)
                return this;

            // Parse the VAR / CAP
            try
            {
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }

            // Parse the variable/capture name
            try { OutputVariable = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("ELEMENTACTION")
                .Token(Locator)
                .Literal(ElementString);

            if (Recursive) writer.Boolean(Recursive, "Recursive");
            else if (ElementIndex != 0) writer.Integer(ElementIndex, "ElementIndex");

            writer
                .Indent()
                .Token(Action)
                .Literal(Input, "Input");

            if (!writer.CheckDefault(OutputVariable, "OutputVariable"))
            {
                writer
                    .Arrow()
                    .Token(IsCapture ? "CAP" : "VAR")
                    .Literal(OutputVariable);
            }

            return writer.ToString();
        }

        /// <inheritdoc />
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            var browser = data.TryGetObject<WebDriver>("selenium");

            if (browser == null)
            {
                throw new Exception("Open a browser first!");
            }

            // Find the element
            IWebElement element = null;
            ReadOnlyCollection<IWebElement> elements = null;

            if (Action != ElementAction.WaitForElement)
            {
                elements = FindElements(browser, ReplaceValues(ElementString, ls));

                if (ElementIndex + 1 > elements.Count)
                {
                    throw new Exception("Cannot find the element on the page");
                }
            }

            var replacedInput = ReplaceValues(Input, ls);
            var outputs = new List<string>();

            switch (Action)
            {
                case ElementAction.Clear:
                    element.Clear();
                    break;

                case ElementAction.SendKeys:
                    element.SendKeys(replacedInput);
                    break;

                case ElementAction.Click:
                    element.Click();
                    UpdateSeleniumData(data);
                    break;

                case ElementAction.Submit:
                    element.Submit();
                    UpdateSeleniumData(data);
                    break;

                case ElementAction.SelectOptionByText:
                    new SelectElement(element).SelectByText(replacedInput);
                    break;

                case ElementAction.SelectOptionByIndex:
                    new SelectElement(element).SelectByIndex(int.Parse(replacedInput));
                    break;

                case ElementAction.SelectOptionByValue:
                    new SelectElement(element).SelectByValue(replacedInput);
                    break;

                case ElementAction.GetText:
                    if (Recursive)
                    {
                        foreach (var elem in elements)
                        {
                            outputs.Add(elem.Text);
                        }
                    }
                    else
                    {
                        outputs.Add(element.Text);
                    }
                    break;

                case ElementAction.GetAttribute:
                    if (Recursive)
                    {
                        foreach (var elem in elements)
                        {
                            outputs.Add(elem.GetAttribute(replacedInput));
                        }
                    }
                    else
                    {
                        outputs.Add(element.GetAttribute(replacedInput));
                    }
                    break;

                case ElementAction.IsDisplayed:
                    outputs.Add(element.Displayed.ToString());
                    break;

                case ElementAction.IsEnabled:
                    outputs.Add(element.Enabled.ToString());
                    break;

                case ElementAction.IsSelected:
                    outputs.Add(element.Selected.ToString());
                    break;

                case ElementAction.LocationX:
                    outputs.Add(element.Location.X.ToString());
                    break;

                case ElementAction.LocationY:
                    outputs.Add(element.Location.Y.ToString());
                    break;

                case ElementAction.SizeX:
                    outputs.Add(element.Size.Width.ToString());
                    break;

                case ElementAction.SizeY:
                    outputs.Add(element.Size.Height.ToString());
                    break;

                case ElementAction.Screenshot:
                    var image = TakeElementScreenshot(browser, element);
                    image.Save(Utils.GetScreenshotPath(data));
                    image.Dispose();
                    break;

                case ElementAction.ScreenshotBase64:
                    var img = TakeElementScreenshot(browser, element);
                    var ms = new MemoryStream();
                    img.Save(ms, ImageFormat.Jpeg);
                    var base64 = Convert.ToBase64String(ms.ToArray());
                    outputs.Add(base64);
                    img.Dispose();
                    ms.Dispose();
                    break;

                case ElementAction.SwitchToFrame:
                    browser.SwitchTo().Frame(element);
                    break;

                case ElementAction.WaitForElement:
                    var waited = 0; // Currently waited milliseconds
                    var timeout = 10000;
                    try
                    {
                        timeout = int.Parse(replacedInput) * 1000;
                    }
                    catch
                    {

                    }
                    var found = false;

                    while (waited < timeout)
                    {
                        try
                        {
                            FindElements(browser, ReplaceValues(ElementString, ls));
                            element = elements[0];
                            found = true;
                            break;
                        }
                        catch
                        {
                            waited += 200;
                            await Task.Delay(200);
                        }
                    }

                    if (!found)
                    {
                        throw new TimeoutException("Timed out while waiting for the element");
                    }

                    break;

                case ElementAction.SendKeysHuman:
                    foreach (var c in replacedInput)
                    {
                        element.SendKeys(c.ToString());
                        await Task.Delay(data.Random.Next(100, 300));
                    }
                    break;
            }

            data.Logger.Log(string.Format("Executed action {0} on the element with input {1}", Action, replacedInput), LogColors.White);

            if (outputs.Count > 0)
            {
                InsertVariable(ls, IsCapture, Recursive, outputs, OutputVariable, "", "", false, true);
            }
        }

        private static Bitmap TakeElementScreenshot(IWebDriver driver, IWebElement element)
        {
            var sc = (driver as ITakesScreenshot).GetScreenshot();
            using var ms = new MemoryStream(sc.AsByteArray);
            using var img = Image.FromStream(ms) as Bitmap;
            return img.Clone(new Rectangle(element.Location, element.Size), img.PixelFormat);
        }

        private ReadOnlyCollection<IWebElement> FindElements(WebDriver browser, string identifier) => Locator switch
        {
            ElementLocator.Id => browser.FindElements(By.Id(identifier)),
            ElementLocator.Class => browser.FindElements(By.ClassName(identifier)),
            ElementLocator.Name => browser.FindElements(By.Name(identifier)),
            ElementLocator.Tag => browser.FindElements(By.TagName(identifier)),
            ElementLocator.Selector => browser.FindElements(By.CssSelector(identifier)),
            ElementLocator.XPath => browser.FindElements(By.XPath(identifier)),
            _ => throw new NotImplementedException()
        };
    }
}
